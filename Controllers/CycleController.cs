using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CycleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CycleController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CycleModel>>> GetAll()
            => await _context.Cycles.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<CycleModel>> GetById(int id)
        {
            var c = await _context.Cycles.FindAsync(id);
            return c == null ? NotFound() : c;
        }

        [HttpPost]
        public async Task<ActionResult<CycleModel>> Create(CycleModel cycle)
        {
            _context.Cycles.Add(cycle);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = cycle.IdCycle }, cycle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CycleModel cycle)
        {
            if (id != cycle.IdCycle) return BadRequest();
            _context.Entry(cycle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Cycles.FindAsync(id);
            if (c == null) return NotFound();
            _context.Cycles.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}