using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class Cie10Controller : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public Cie10Controller(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/cie10?search=diabetes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cie10CodeDTO>>> Search([FromQuery] string? search, [FromQuery] int limit = 50)
        {
            var query = _context.Cie10Codes.Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Code.ToLower().Contains(term) ||
                    c.Description.ToLower().Contains(term) ||
                    (c.Category != null && c.Category.ToLower().Contains(term)));
            }

            var codes = await query
                .OrderBy(c => c.Code)
                .Take(limit)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<Cie10CodeDTO>>(codes);
            return Ok(dtos);
        }

        // GET: api/cie10/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Cie10CodeDTO>> GetById(int id)
        {
            var code = await _context.Cie10Codes.FindAsync(id);
            if (code == null) return NotFound();
            return Ok(_mapper.Map<Cie10CodeDTO>(code));
        }

        // GET: api/cie10/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<Cie10CodeDTO>> GetByCode(string code)
        {
            var cie = await _context.Cie10Codes
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
            if (cie == null) return NotFound();
            return Ok(_mapper.Map<Cie10CodeDTO>(cie));
        }
    }
}
