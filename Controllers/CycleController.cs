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
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouveau cycle",
                Description = $"Cycle \"{cycle.NomCycle}\" créé avec le statut {cycle.Statut}",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
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

        [HttpPost("{id}/demarrer")]
        public async Task<IActionResult> Demarrer(int id)
        {
            var c = await _context.Cycles.FindAsync(id);
            if (c == null) return NotFound();
            if (c.Statut == "Actif") return BadRequest("Ce cycle est déjà actif.");
            if (c.Statut == "Terminé") return BadRequest("Un cycle terminé ne peut pas être relancé.");

            c.Statut = "Actif";
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Cycle démarré",
                Description = $"Cycle \"{c.NomCycle}\" passé en statut Actif",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(c);
        }

        [HttpPost("{id}/cloture")]
        public async Task<IActionResult> Cloturer(int id)
        {
            var c = await _context.Cycles.FindAsync(id);
            if (c == null) return NotFound();
            if (c.Statut == "Terminé") return BadRequest("Ce cycle est déjà terminé.");

            c.Statut = "Terminé";
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Cycle clôturé",
                Description = $"Cycle \"{c.NomCycle}\" clôturé",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(c);
        }
    }
}
