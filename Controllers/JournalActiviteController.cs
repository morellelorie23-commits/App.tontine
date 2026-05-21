using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JournalActiviteController : ControllerBase
    {
        private readonly AppDbContext _context;
        public JournalActiviteController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Journals
                .OrderByDescending(j => j.DateAction)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var j = await _context.Journals.FindAsync(id);
            return j == null ? NotFound() : Ok(j);
        }

        [HttpPost]
        public async Task<IActionResult> Create(JournalActiviteModel entry)
        {
            entry.DateAction = DateTime.Now;
            _context.Journals.Add(entry);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entry.IdJournal }, entry);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var j = await _context.Journals.FindAsync(id);
            if (j == null) return NotFound();
            _context.Journals.Remove(j);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
