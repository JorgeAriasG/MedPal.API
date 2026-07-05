using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Authorization;
using Stripe.Checkout;

namespace MedPal.API.Services.Implementations
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IStripeService _stripeService;
        private readonly IPendingRegistrationRepository _pendingRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationService> _logger;
        private readonly AppDbContext _context;

        public RegistrationService(
            IStripeService stripeService,
            IPendingRegistrationRepository pendingRepo,
            ISubscriptionRepository subscriptionRepo,
            IAccountRepository accountRepository,
            IUserRepository userRepository,
            IClinicRepository clinicRepository,
            IRoleRepository roleRepository,
            ITokenService tokenService,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<RegistrationService> logger,
            AppDbContext context)
        {
            _stripeService = stripeService;
            _pendingRepo = pendingRepo;
            _subscriptionRepo = subscriptionRepo;
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _clinicRepository = clinicRepository;
            _roleRepository = roleRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task<InitiateRegistrationResponseDTO> InitiateAsync(InitiateRegistrationRequestDTO request)
        {
            var planName = request.PlanName ?? "SOLO";
            var plan = await _subscriptionRepo.GetPlanByNameAsync(planName);
            if (plan == null)
                throw new InvalidOperationException($"Plan '{planName}' no encontrado");
            if (string.IsNullOrEmpty(plan.StripePriceId))
                throw new InvalidOperationException($"Plan '{planName}' no tiene precio de Stripe configurado");

            var session = await _stripeService.CreateRegistrationCheckoutSessionAsync(
                request.Email,
                request.Name,
                plan.StripePriceId,
                plan.TrialDays,
                "pending",
                planName);

            var registration = new PendingRegistration
            {
                Email = request.Email.Trim().ToLower(),
                RegistrationData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    request.Name,
                    request.Email,
                    request.Password,
                    request.ProfessionalLicenseNumber,
                    request.Specialty,
                    request.AcceptPrivacyTerms,
                    request.PlanName,
                }),
                StripeSessionId = session.Id,
                Status = "pending",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
            };
            registration = await _pendingRepo.CreateAsync(registration);

            var publishableKey = _configuration["Stripe:PublishableKey"] ?? "";

            return new InitiateRegistrationResponseDTO
            {
                ClientSecret = session.ClientSecret,
                SessionId = session.Id,
                PublishableKey = publishableKey,
            };
        }

        public async Task<CompleteRegistrationResponseDTO> CompleteAsync(string sessionId)
        {
            var pending = await _pendingRepo.GetBySessionIdAsync(sessionId);
            if (pending == null)
                throw new InvalidOperationException("Registro no encontrado");

            if (pending.Status != "pending")
            {
                var existing = await GetExistingAccountAsync(pending);
                if (existing != null)
                    return existing;
                throw new InvalidOperationException("El registro ya fue procesado");
            }

            var session = await _stripeService.VerifySessionAsync(sessionId);

            if (session.Status != "complete")
                throw new InvalidOperationException("El pago no fue completado");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await CreateAccountFromPendingAsync(pending, session);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                await _pendingRepo.UpdateStatusAsync(pending.Id, "failed");
                throw;
            }
        }

        public async Task<CompleteRegistrationResponseDTO?> CompleteFromWebhookAsync(string sessionId)
        {
            var pending = await _pendingRepo.GetBySessionIdAsync(sessionId);
            if (pending == null)
            {
                _logger.LogWarning("Webhook: no pending registration for session {SessionId}", sessionId);
                return null;
            }

            if (pending.Status != "pending")
            {
                _logger.LogInformation("Webhook: registration {Id} already processed ({Status})", pending.Id, pending.Status);
                return null;
            }

            var session = await _stripeService.VerifySessionAsync(sessionId);
            if (session.Status != "complete")
            {
                _logger.LogWarning("Webhook: session {SessionId} is not complete", sessionId);
                return null;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await CreateAccountFromPendingAsync(pending, session);
                await transaction.CommitAsync();
                _logger.LogInformation("Webhook: registration completed for {Email}", pending.Email);
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _pendingRepo.UpdateStatusAsync(pending.Id, "failed");
                _logger.LogError(ex, "Webhook: failed to complete registration for {Email}", pending.Email);
                return null;
            }
        }

        private async Task<CompleteRegistrationResponseDTO> CreateAccountFromPendingAsync(
            PendingRegistration pending, Session session)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<RegistrationData>(pending.RegistrationData)
                ?? throw new InvalidOperationException("Datos de registro inválidos");

            var newAccount = new Account
            {
                Name = data.Name,
                Description = $"Cuenta de {data.Name} - Creada al registrarse",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            newAccount = await _accountRepository.CreateAccountAsync(newAccount);

            var user = new User
            {
                Name = data.Name,
                Email = pending.Email,
                ProfessionalLicenseNumber = data.ProfessionalLicenseNumber,
                Specialty = data.Specialty ?? "General",
                HasAcceptedPrivacyTerms = data.AcceptPrivacyTerms,
                PasswordHash = data.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AccountId = newAccount.Id,
            };
            var createdUser = await _userRepository.AddUserAsync(user);

            var newClinic = new Clinic
            {
                Name = $"Consultorio de {data.Name}",
                Location = "Sin configurar",
                ContactInfo = pending.Email,
                AccountId = newAccount.Id,
                Open = new TimeOnly(9, 0),
                Close = new TimeOnly(18, 0),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await _clinicRepository.AddClinicAsync(createdUser.Id, newClinic);

            createdUser.ClinicId = newClinic.Id;
            await _userRepository.UpdateUserAsync(createdUser);

            var accountAdminRole = await _roleRepository.GetRoleByNameAsync("AccountAdmin");
            if (accountAdminRole != null)
            {
                await _roleRepository.AssignRoleToUserAsync(
                    createdUser.Id, accountAdminRole.Id,
                    clinicId: null, expiresAt: null, assignedByUserId: null);
            }

            var planName = data.PlanName ?? "SOLO";
            var plan = await _subscriptionRepo.GetPlanByNameAsync(planName)
                ?? await _subscriptionRepo.GetPlanByNameAsync("SOLO");

            var trialEnd = plan != null && plan.TrialDays > 0
                ? DateTime.UtcNow.AddDays(plan.TrialDays)
                : (DateTime?)null;

            var subscription = new Subscription
            {
                AccountId = newAccount.Id,
                SubscriptionPlanId = plan?.Id ?? 0,
                Status = trialEnd != null ? "trial" : "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = trialEnd ?? DateTime.UtcNow.AddDays(30),
                IsActive = true,
                TrialEndsAt = trialEnd,
                StripeCustomerId = session.CustomerId,
                StripeSubscriptionId = session.SubscriptionId,
                MaxTeamMembers = plan?.MaxTeamMembers ?? 1,
                MaxClinics = plan?.MaxClinics ?? 1,
                MaxActiveCalendars = plan?.MaxActiveCalendars ?? 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await _subscriptionRepo.CreateAsync(subscription);

            var token = _tokenService.GenerateToken(createdUser);

            await _pendingRepo.UpdateStatusAsync(pending.Id, "completed", newAccount.Id);

            return new CompleteRegistrationResponseDTO
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                Token = token,
                Role = "AccountAdmin",
                AccountId = newAccount.Id,
                ClinicId = newClinic.Id,
                Permissions = new(),
            };
        }

        private async Task<CompleteRegistrationResponseDTO?> GetExistingAccountAsync(PendingRegistration pending)
        {
            if (!pending.AccountId.HasValue) return null;

            var users = await _userRepository.GetAllUsersByAccountId(pending.AccountId.Value);
            var user = users.FirstOrDefault();
            if (user == null) return null;

            var userDto = _mapper.Map<UserReadDTO>(user);
            var token = _tokenService.GenerateToken(user);

            return new CompleteRegistrationResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                Role = userDto.Role,
                AccountId = pending.AccountId.Value,
                ClinicId = userDto.ClinicId,
                Permissions = new(),
            };
        }

        private class RegistrationData
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ProfessionalLicenseNumber { get; set; }
            public string? Specialty { get; set; }
            public bool AcceptPrivacyTerms { get; set; }
            public string? PlanName { get; set; }
        }
    }
}
