using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembreController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MembreController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembreModel>>> GetAll()
            => await _context.Membres.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<MembreModel>> GetById(int id)
        {
            var m = await _context.Membres.FindAsync(id);
            return m == null ? NotFound() : m;
        }

        [HttpPost]
        public async Task<ActionResult<MembreModel>> Create(MembreModel membre)
        {
            _context.Membres.Add(membre);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = membre.IdMembre }, membre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MembreModel membre)
        {
            if (id != membre.IdMembre) return BadRequest();
            _context.Entry(membre).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.Membres.FindAsync(id);
            if (m == null) return NotFound();
            _context.Membres.Remove(m);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}