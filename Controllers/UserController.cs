using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
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
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public UserController(
            IUserRepository userRepository,
            IClinicRepository clinicRepository,
            IMapper mapper,
            ITokenService tokenService,
            IUserService userService)
        {
            _userRepository = userRepository;
            _clinicRepository = clinicRepository;
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
            var user = await _userRepository.ValidateUserAsync(userLoginDto.Email, userLoginDto.PasswordHash);
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
    }
}