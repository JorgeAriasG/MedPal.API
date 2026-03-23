using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Data;
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
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public UserController(
            IUserRepository userRepository,
            IClinicRepository clinicRepository,
            IRoleRepository roleRepository,
            AppDbContext context,
            IMapper mapper,
            ITokenService tokenService,
            IUserService userService)
        {
            _userRepository = userRepository;
            _clinicRepository = clinicRepository;
            _roleRepository = roleRepository;
            _context = context;
            _mapper = mapper;
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "Users.ViewAll")]
        [Authorize(Policy = "ViewUsersPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userReadDTOs = _mapper.Map<IEnumerable<UserReadDTO>>(users);
            return Ok(userReadDTOs);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Users.ViewAll")]
        [Authorize(Policy = "ViewUsersPolicy")] // Fase 2: Multi-tenancy policy
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
        [Authorize(Policy = "ViewUsersPolicy")] // Fase 2: Multi-tenancy policy
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
        [Authorize(Policy = "ManageUsersPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult> AddUser(UserWriteDTO userWriteDto)
        {
            // Validar que el email no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(userWriteDto.Email))
            {
                return BadRequest(new { message = "El email es requerido" });
            }

            // Normalizar email: trim y convertir a minúsculas
            userWriteDto.Email = userWriteDto.Email.Trim().ToLower();

            // Verificar si el email ya existe
            var existingUser = await _userRepository.GetUserByEmailAsync(userWriteDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "El email ya está registrado en el sistema" });
            }

            // Fase 2: Extraer AccountId del JWT del usuario autenticado (sin query extra)
            var accountIdClaim = User.FindFirst("account_id");
            if (!int.TryParse(accountIdClaim?.Value, out int accountId))
            {
                return Unauthorized("Usuario no tiene AccountId asignado");
            }

            var user = _mapper.Map<User>(userWriteDto);
            user.AccountId = accountId;  // Asignar el AccountId del usuario que crea el nuevo usuario
            var createdUser = await _userRepository.AddUserAsync(user);

            // Fase 2: Si se especifica un rol, asignarlo al usuario
            if (userWriteDto.RoleId != 0)
            {
                var role = await _roleRepository.GetRoleByIdAsync(userWriteDto.RoleId);
                if (role == null)
                {
                    return BadRequest(new { message = $"El rol '{userWriteDto.RoleId}' no existe en el sistema" });
                }

                // Asignar el rol al usuario (si se proporciona clinicId, asignarlo a esa clínica, si no, a nivel global)
                await _roleRepository.AssignRoleToUserAsync(
                    createdUser.Id,
                    role.Id,
                    clinicId: userWriteDto.PrincipalClinicId,
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
            return Ok(userReadDTO);
        }

        /// <summary>
        /// <summary>
        /// Registra un nuevo usuario y crea automáticamente su Account.
        /// El usuario se crea como AccountAdmin de su nueva Account.
        /// Este endpoint es público para permitir que nuevas clínicas/hospitales se registren.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserReadDTO>> Register([FromBody] UserRegisterDTO registerDto)
        {
            // Validar que el DTO sea válido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Normalizar email: trim y convertir a minúsculas
            registerDto.Email = registerDto.Email?.Trim().ToLower() ?? string.Empty;

            // Verificar si el email ya existe
            var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "El email ya está registrado en el sistema" });
            }

            // Crear una nueva Account para la clínica/hospital que se registra
            // El nombre de la Account será el nombre del usuario/organización
            var newAccount = new Account
            {
                Name = registerDto.Name,
                Description = $"Cuenta de {registerDto.Name} - Creada al registrarse",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Guardar la nueva Account
            await _context.Accounts.AddAsync(newAccount);
            await _context.SaveChangesAsync();
            
            if (newAccount.Id == 0)
            {
                return BadRequest(new { message = "No se pudo crear la Account" });
            }

            var user = _mapper.Map<User>(registerDto);
            user.HasAcceptedPrivacyTerms = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.AccountId = newAccount.Id;  // ← Assign new accountId to the user

            // Crear el nuevo usuario asignado a la Account que acaba de crear
            // var newUser = new User
            // {
            //     Name = registerDto.Name,
            //     Email = registerDto.Email,
            //     PasswordHash = registerDto.Password,
            //     Specialty = registerDto.Specialty,
            //     ProfessionalLicenseNumber = registerDto.ProfessionalLicenseNumber,
            //     IsActive = true,
            //     IsDeleted = false,
            //     HasAcceptedPrivacyTerms = registerDto.AcceptPrivacyTerms,
            //     AccountId = newAccount.Id,  // ← Asignar a la Account que acaba de crear
            //     CreatedAt = DateTime.UtcNow,
            //     UpdatedAt = DateTime.UtcNow
            // };

            // Agregar el usuario a la base de datos
            var createdUser = await _userRepository.AddUserAsync(user);

            // Obtener el rol de AccountAdmin
            var accountAdminRole = await _roleRepository.GetRoleByNameAsync("AccountAdmin");
            if (accountAdminRole == null)
            {
                return BadRequest(new { message = "El rol AccountAdmin no está configurado en el sistema" });
            }

            // Asignar el rol AccountAdmin al nuevo usuario de forma global (para toda su Account)
            await _roleRepository.AssignRoleToUserAsync(createdUser.Id, accountAdminRole.Id, clinicId: null, expiresAt: null, assignedByUserId: null);

            // Generar token JWT para el nuevo usuario (incluye AccountId automáticamente)
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

        /// <summary>
        /// Obtiene el perfil completo del usuario autenticado.
        /// Retorna información personal, especialidad, roles y clínicas asociadas.
        /// </summary>
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

            // Obtener roles del usuario
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

            if(!userRoles.Contains(SystemRole.SuperAdmin.GetDisplayName())) {
                // Obtener clínicas del usuario
                var userClinics = await _userRepository.GetUserClinicsAsync(userId);
                List<ClinicBasicDTO> userClinicsDTO = _mapper.Map<List<ClinicBasicDTO>>(userClinics);
                profile.Clinics = userClinicsDTO;
            }


            return Ok(profile);
        }
    }
}