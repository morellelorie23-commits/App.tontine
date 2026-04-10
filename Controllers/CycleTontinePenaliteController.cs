using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CycleTontinePenaliteController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CycleTontinePenaliteController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CycleTontinePenaliteModel>>> GetAll()
            => await _context.CycleTontinePenalites
                .Include(c => c.Cycle)
                .Include(c => c.Tontine)
                .Include(c => c.Penalite)
                .ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<CycleTontinePenaliteModel>> GetById(int id)
        {
            var c = await _context.CycleTontinePenalites
                .Include(c => c.Cycle)
                .Include(c => c.Tontine)
                .Include(c => c.Penalite)
                .FirstOrDefaultAsync(c => c.Id == id);
            return c == null ? NotFound() : c;
        }

        [HttpPost]
        public async Task<ActionResult<CycleTontinePenaliteModel>> Create(CycleTontinePenaliteModel ctp)
        {
            _context.CycleTontinePenalites.Add(ctp);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ctp.Id }, ctp);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CycleTontinePenaliteModel ctp)
        {
            if (id != ctp.Id) return BadRequest();
            _context.Entry(ctp).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.CycleTontinePenalites.FindAsync(id);
            if (c == null) return NotFound();
            _context.CycleTontinePenalites.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}