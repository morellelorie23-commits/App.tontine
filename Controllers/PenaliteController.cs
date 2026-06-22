using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PenaliteController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PenaliteController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PenaliteModel>>> GetAll()
            => await _context.Penalites.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<PenaliteModel>> GetById(int id)
        {
            var p = await _context.Penalites.FindAsync(id);
            return p == null ? NotFound() : p;
        }

        [HttpPost]
        public async Task<ActionResult<PenaliteModel>> Create(PenaliteModel penalite)
        {
            _context.Penalites.Add(penalite);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouvelle pénalité",
                Description = $"Pénalité \"{penalite.Libelle}\" créée",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = penalite.IdPenalite }, penalite);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PenaliteModel penalite)
        {
            if (id != penalite.IdPenalite) return BadRequest();
            _context.Entry(penalite).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Pénalité modifiée",
                Description = $"Pénalité \"{penalite.Libelle}\" (id={id}) mise à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Penalites.FindAsync(id);
            if (p == null) return NotFound();
            _context.Penalites.Remove(p);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Pénalité supprimée",
                Description = $"Pénalité \"{p.Libelle}\" (id={id}) supprimée",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}