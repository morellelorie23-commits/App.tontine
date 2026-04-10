using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembrePosteCycleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MembrePosteCycleController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembrePosteCycleModel>>> GetAll()
            => await _context.MembrePosteCycles
                .Include(m => m.Membre)
                .Include(m => m.Poste)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<MembrePosteCycleModel>> GetById(int id)
        {
            var m = await _context.MembrePosteCycles
                .Include(m => m.Membre)
                .Include(m => m.Poste)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .FirstOrDefaultAsync(m => m.Id == id);
            return m == null ? NotFound() : m;
        }

        [HttpPost]
        public async Task<ActionResult<MembrePosteCycleModel>> Create(MembrePosteCycleModel mpc)
        {
            _context.MembrePosteCycles.Add(mpc);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = mpc.Id }, mpc);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MembrePosteCycleModel mpc)
        {
            if (id != mpc.Id) return BadRequest();
            _context.Entry(mpc).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.MembrePosteCycles.FindAsync(id);
            if (m == null) return NotFound();
            _context.MembrePosteCycles.Remove(m);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}