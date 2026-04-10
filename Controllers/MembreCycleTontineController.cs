using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembreCycleTontineController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MembreCycleTontineController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembreCycleTontineModel>>> GetAll()
            => await _context.MembreCycleTontines
                .Include(m => m.Membre)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<MembreCycleTontineModel>> GetById(int id)
        {
            var m = await _context.MembreCycleTontines
                .Include(m => m.Membre)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .FirstOrDefaultAsync(m => m.Id == id);
            return m == null ? NotFound() : m;
        }

        [HttpPost]
        public async Task<ActionResult<MembreCycleTontineModel>> Create(MembreCycleTontineModel mct)
        {
            _context.MembreCycleTontines.Add(mct);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = mct.Id }, mct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MembreCycleTontineModel mct)
        {
            if (id != mct.Id) return BadRequest();
            _context.Entry(mct).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.MembreCycleTontines.FindAsync(id);
            if (m == null) return NotFound();
            _context.MembreCycleTontines.Remove(m);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}