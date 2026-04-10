using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CycleTontineController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CycleTontineController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CycleTontineModel>>> GetAll()
            => await _context.CycleTontines
                .Include(ct => ct.Cycle)
                .Include(ct => ct.Tontine)
                .ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<CycleTontineModel>> GetById(int id)
        {
            var ct = await _context.CycleTontines
                .Include(ct => ct.Cycle)
                .Include(ct => ct.Tontine)
                .FirstOrDefaultAsync(ct => ct.Id == id);
            return ct == null ? NotFound() : ct;
        }

        [HttpPost]
        public async Task<ActionResult<CycleTontineModel>> Create(CycleTontineModel ct)
        {
            _context.CycleTontines.Add(ct);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ct.Id }, ct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CycleTontineModel ct)
        {
            if (id != ct.Id) return BadRequest();
            _context.Entry(ct).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ct = await _context.CycleTontines.FindAsync(id);
            if (ct == null) return NotFound();
            _context.CycleTontines.Remove(ct);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}