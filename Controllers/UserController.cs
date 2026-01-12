using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public UserController(
            IUserRepository userRepository,
            IClinicRepository clinicRepository,
            IRoleRepository roleRepository,
            IMapper mapper,
            ITokenService tokenService,
            IUserService userService)
        {
            _userRepository = userRepository;
            _clinicRepository = clinicRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "Users.ViewAll")]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userReadDTOs = _mapper.Map<IEnumerable<UserReadDTO>>(users);
            return Ok(userReadDTOs);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Users.ViewAll")]
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

        // TODO: Fix the AddUser method, current status is returning error when no clinic is provided or existing clinic is not found.
        [HttpPost]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> AddUser(UserWriteDTO userWriteDto)
        {
            var user = _mapper.Map<User>(userWriteDto);
            var createdUser = await _userRepository.AddUserAsync(user);
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
        /// Registra un nuevo usuario en el sistema con rol de Admin por defecto.
        /// Todos los nuevos usuarios se crean automáticamente como administradores.
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

            // Verificar si el email ya existe
            var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "El email ya está registrado en el sistema" });
            }

            // Crear el nuevo usuario
            var newUser = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = registerDto.Password,
                Specialty = registerDto.Specialty,
                ProfessionalLicenseNumber = registerDto.ProfessionalLicenseNumber,
                IsActive = true,
                IsDeleted = false,
                HasAcceptedPrivacyTerms = registerDto.AcceptPrivacyTerms,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Agregar el usuario a la base de datos
            var createdUser = await _userRepository.AddUserAsync(newUser);

            // Obtener el rol de Admin
            var adminRole = await _roleRepository.GetRoleByNameAsync("Admin");
            if (adminRole == null)
            {
                // Si no existe el rol Admin, devolver error
                return BadRequest(new { message = "El rol de administrador no está configurado en el sistema" });
            }

            // Asignar el rol de Admin al nuevo usuario de forma global (sin clínica específica)
            await _roleRepository.AssignRoleToUserAsync(createdUser.Id, adminRole.Id, clinicId: null, expiresAt: null, assignedByUserId: null);

            // Generar token JWT para el nuevo usuario
            var token = _tokenService.GenerateToken(createdUser);
            var userReadDTO = _mapper.Map<UserReadDTO>(createdUser);
            userReadDTO.Token = token;

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, userReadDTO);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Users.Manage")]
        public async Task<ActionResult> UpdateUser(int id, UserWriteDTO userWriteDto)
        {
            var user = _mapper.Map<User>(userWriteDto);
            user.Id = int.TryParse(_userService.UserId, out int userId) ? userId : 0;
            await _userRepository.UpdateUserAsync(id, user);
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

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Obtener roles del usuario
            var userRoles = user.UserRoles?
                .Where(ur => !ur.IsDeleted)
                .Select(ur => ur.Role?.Name)
                .Where(name => name != null)
                .ToList() ?? new List<string>();

            // Obtener clínicas del usuario
            var userClinics = user.UserClinics?
                .Where(uc => !uc.IsDeleted)
                .Select(uc => new ClinicBasicDTO
                {
                    Id = uc.ClinicId,
                    Name = uc.Clinic?.Name ?? "Clínica Desconocida"
                })
                .ToList() ?? new List<ClinicBasicDTO>();

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
                Roles = userRoles,
                Clinics = userClinics
            };

            return Ok(profile);
        }
    }
}