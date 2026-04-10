using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosteController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PosteController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PosteModel>>> GetAll()
            => await _context.Postes.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<PosteModel>> GetById(int id)
        {
            var p = await _context.Postes.FindAsync(id);
            return p == null ? NotFound() : p;
        }

        [HttpPost]
        public async Task<ActionResult<PosteModel>> Create(PosteModel poste)
        {
            _context.Postes.Add(poste);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = poste.IdPoste }, poste);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PosteModel poste)
        {
            if (id != poste.IdPoste) return BadRequest();
            _context.Entry(poste).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Postes.FindAsync(id);
            if (p == null) return NotFound();
            _context.Postes.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}