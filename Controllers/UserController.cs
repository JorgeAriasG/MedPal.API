using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Enums;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public UserController(
            IUserRepository userRepository,
            IClinicRepository clinicRepository,
            IRoleRepository roleRepository,
            IAccountRepository accountRepository,
            IMapper mapper,
            ITokenService tokenService,
            IUserService userService)
        {
            _userRepository = userRepository;
            _clinicRepository = clinicRepository;
            _roleRepository = roleRepository;
            _accountRepository = accountRepository;
            _mapper = mapper;
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "Users.ViewAll")]
        [Authorize(Policy = "ViewUsersPolicy")]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userReadDTOs = _mapper.Map<IEnumerable<UserReadDTO>>(users);
            return Ok(userReadDTOs);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Users.ViewAll")]
        [Authorize(Policy = "ViewUsersPolicy")]
        public async Task<ActionResult<UserReadDTO>> GetUserById(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userReadDTO = _mapper.Map<UserReadDTO>(user);
            return Ok(userReadDTO);
        }

        [HttpGet("account")]
        [Authorize(Policy = "Users.ViewAll")]
        [Authorize(Policy = "ViewUsersPolicy")]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetAllUsersByAccountId()
        {
            var users = await _userRepository.GetAllUsersByAccountId(int.Parse(_userService.AccountId));
            var userReadDTOs = _mapper.Map<IEnumerable<UserReadDTO>>(users);
            
            if(!userReadDTOs.Any())
            {
                return NotFound();
            }
            
            return Ok(userReadDTOs);
        }

        [HttpPost]
        [Authorize(Policy = "Users.Manage")]
        [Authorize(Policy = "ManageUsersPolicy")]
        public async Task<ActionResult> AddUser(UserWriteDTO userWriteDto)
        {
            if (string.IsNullOrWhiteSpace(userWriteDto.Email))
            {
                return BadRequest(new { message = "El email es requerido" });
            }

            userWriteDto.Email = userWriteDto.Email.Trim().ToLower();

            var existingUser = await _userRepository.GetUserByEmailAsync(userWriteDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "El email ya está registrado en el sistema" });
            }

            var accountIdClaim = User.FindFirst("account_id");
            if (!int.TryParse(accountIdClaim?.Value, out int accountId))
            {
                return Unauthorized("Usuario no tiene AccountId asignado");
            }

            var user = _mapper.Map<User>(userWriteDto);
            user.AccountId = accountId;
            var createdUser = await _userRepository.AddUserAsync(user);

            if (userWriteDto.RoleId != 0)
            {
                var role = await _roleRepository.GetRoleByIdAsync(userWriteDto.RoleId);
                if (role == null)
                {
                    return BadRequest(new { message = $"El rol '{userWriteDto.RoleId}' no existe en el sistema" });
                }

                await _roleRepository.AssignRoleToUserAsync(
                    createdUser.Id,
                    role.Id,
                    clinicId: userWriteDto.ClinicId,
                    expiresAt: null,
                    assignedByUserId: int.TryParse(_userService.UserId, out int userId) ? userId : null
                );
            }

            var userReadDTO = _mapper.Map<UserReadDTO>(createdUser);
            return CreatedAtAction(nameof(GetUserById), new { id = userReadDTO.Id }, userReadDTO);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserReadDTO>> Login(UserLoginDTO userLoginDto)
        {
            var user = await _userRepository.ValidateUserAsync(userLoginDto.Email, userLoginDto.Password);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            await _userRepository.UpdateUserLastAccessAtAsync(user.Id);
            var token = _tokenService.GenerateToken(user);
            var userReadDTO = _mapper.Map<UserReadDTO>(user);
            userReadDTO.Token = token;
            userReadDTO.Role = user.UserRoles?.FirstOrDefault()?.Role?.Name ?? "Doctor";

            return Ok(userReadDTO);
        }

        /// <summary>
        /// Registra un nuevo usuario y crea automáticamente su Account, Clínica y roles.
        /// El usuario se crea como AccountAdmin + ClinicAdmin de su nueva Account/Clínica.
        /// La clínica se crea con horario default (9:00–18:00).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserReadDTO>> Register([FromBody] UserRegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            registerDto.Email = registerDto.Email?.Trim().ToLower() ?? string.Empty;

            var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "El email ya está registrado en el sistema" });
            }

            var newAccount = new Account
            {
                Name = registerDto.Name,
                Description = $"Cuenta de {registerDto.Name} - Creada al registrarse",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            newAccount = await _accountRepository.CreateAccountAsync(newAccount);

            if (newAccount.Id == 0)
            {
                return BadRequest(new { message = "No se pudo crear la Account" });
            }

            var user = _mapper.Map<User>(registerDto);
            user.HasAcceptedPrivacyTerms = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.AccountId = newAccount.Id;

            var createdUser = await _userRepository.AddUserAsync(user);

            var newClinic = new Clinic
            {
                Name = $"Consultorio de {registerDto.Name}",
                Location = "Sin configurar",
                ContactInfo = registerDto.Email,
                AccountId = newAccount.Id,
                Open = new TimeOnly(9, 0),
                Close = new TimeOnly(18, 0),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _clinicRepository.AddClinicAsync(createdUser.Id, newClinic);

            createdUser.ClinicId = newClinic.Id;
            await _userRepository.UpdateUserAsync(createdUser);

            var accountAdminRole = await _roleRepository.GetRoleByNameAsync("AccountAdmin");
            if (accountAdminRole == null)
            {
                return BadRequest(new { message = "El rol AccountAdmin no está configurado en el sistema" });
            }

            await _roleRepository.AssignRoleToUserAsync(
                createdUser.Id, accountAdminRole.Id,
                clinicId: null, expiresAt: null, assignedByUserId: null);

            var clinicAdminRole = await _roleRepository.GetRoleByNameAsync("ClinicAdmin");
            if (clinicAdminRole != null)
            {
                await _roleRepository.AssignRoleToUserAsync(
                    createdUser.Id, clinicAdminRole.Id,
                    clinicId: newClinic.Id, expiresAt: null, assignedByUserId: null);
            }

            var token = _tokenService.GenerateToken(createdUser);
            var userReadDTO = _mapper.Map<UserReadDTO>(createdUser);
            userReadDTO.Token = token;

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, userReadDTO);
        }

        [HttpPut]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> UpdateUser(UserUpdateDTO userUpdateDto)
        {
            var user = _mapper.Map<User>(userUpdateDto);
            user.Id = int.TryParse(_userService.UserId, out int userId) ? userId : 0;
            await _userRepository.UpdateUserAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await _userRepository.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPost("soft-delete/{id}")]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> SoftDeleteUser(int id, [FromBody] int deletedByUserId)
        {
            deletedByUserId = int.TryParse(_userService.UserId, out int userId) ? userId : 0;
            await _userRepository.SoftDeleteUserAsync(id, deletedByUserId);
            return NoContent();
        }

        [HttpPost("restore/{id}")]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> RestoreUser(int id)
        {
            id = int.TryParse(_userService.UserId, out int userId) ? userId : 0;
            await _userRepository.RestoreUserAsync(id);
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileDTO>> GetMyProfile()
        {
            var userIdString = _userService.UserId;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Usuario no identificado");
            }

            var user = await _userRepository.GetOwnProfileAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var userRoles = await _userRepository.GetUserRolesAsync(userId);

            var profile = new UserProfileDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Specialty = user.Specialty,
                ProfessionalLicenseNumber = user.ProfessionalLicenseNumber,
                IsActive = user.IsActive,
                LastAccessAt = user.LastAccessAt ?? DateTime.UtcNow,
                HasAcceptedPrivacyTerms = user.HasAcceptedPrivacyTerms,
                CreatedAt = user.CreatedAt,
                Roles = userRoles
            };

            if(!userRoles.Contains(SystemRole.SuperAdmin.GetDisplayName()))
            {
                var clinic = await _userRepository.GetUserClinicAsync(userId);
                if (clinic != null)
                {
                    profile.Clinics = new List<ClinicBasicDTO>
                    {
                        new ClinicBasicDTO { Id = clinic.Id, Name = clinic.Name }
                    };
                }
            }

            return Ok(profile);
        }
    }
}
