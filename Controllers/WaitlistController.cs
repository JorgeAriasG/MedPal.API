using AutoMapper;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaitlistController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public WaitlistController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] WaitlistRegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.Set<WaitlistEntry>()
                .AnyAsync(w => w.Email == dto.Email);

            if (exists)
                return Conflict(new { message = "Este correo ya está registrado en la lista de espera." });

            var entry = _mapper.Map<WaitlistEntry>(dto);
            entry.CreatedAt = DateTime.UtcNow;

            _context.Set<WaitlistEntry>().Add(entry);
            await _context.SaveChangesAsync();

            return Created(string.Empty, new { message = "Registro exitoso. Te contactaremos pronto." });
        }
    }
}
