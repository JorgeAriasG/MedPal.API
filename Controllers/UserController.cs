using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository, IClinicRepository clinicRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _clinicRepository = clinicRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userReadDTOs = _mapper.Map<IEnumerable<UserReadDTO>>(users);
            return Ok(userReadDTOs);
        }

        [HttpGet("{id}")]
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
        public async Task<ActionResult> AddUser(UserWriteDTO userWriteDto)
        {
            var user = _mapper.Map<User>(userWriteDto);
            var createdUser = await _userRepository.AddUserAsync(user);
            var userReadDTO = _mapper.Map<UserReadDTO>(createdUser);

            return CreatedAtAction(nameof(GetUserById), new { id = userReadDTO.Id }, userReadDTO);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserReadDTO>> Login(UserLoginDTO userLoginDto)
        {
            var user = await _userRepository.ValidateUserAsync(userLoginDto.Email, userLoginDto.PasswordHash);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            var userReadDTO = _mapper.Map<UserReadDTO>(user);
            return Ok(userReadDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, UserWriteDTO userWriteDto)
        {
            var user = _mapper.Map<User>(userWriteDto);
            await _userRepository.UpdateUserAsync(id, user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await _userRepository.DeleteUserAsync(id);
            return NoContent();
        }
    }
}