using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReunionController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReunionController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Reunions
                .Include(r => r.Cycle)
                .Include(r => r.Tontine)
                .Select(r => new {
                    r.IdReunion,
                    r.IdCycle,
                    NomCycle       = r.Cycle != null ? r.Cycle.NomCycle : "",
                    r.IdTontine,
                    LibelleTontine = r.Tontine != null ? r.Tontine.Libelle : "",
                    r.DateReunion,
                    r.Objet,
                    r.Lieu,
                    r.Notes
                })
                .OrderByDescending(r => r.DateReunion)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _context.Reunions
                .Include(x => x.Cycle)
                .Include(x => x.Tontine)
                .Where(x => x.IdReunion == id)
                .Select(x => new {
                    x.IdReunion,
                    x.IdCycle,
                    NomCycle       = x.Cycle != null ? x.Cycle.NomCycle : "",
                    x.IdTontine,
                    LibelleTontine = x.Tontine != null ? x.Tontine.Libelle : "",
                    x.DateReunion,
                    x.Objet,
                    x.Lieu,
                    x.Notes
                })
                .FirstOrDefaultAsync();
            return r == null ? NotFound() : Ok(r);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReunionModel reunion)
        {
            _context.Reunions.Add(reunion);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouvelle réunion",
                Description = $"Réunion du {reunion.DateReunion:dd/MM/yyyy} — {reunion.Objet}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = reunion.IdReunion }, reunion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ReunionModel reunion)
        {
            if (id != reunion.IdReunion) return BadRequest();
            _context.Entry(reunion).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Modification réunion",
                Description = $"Réunion #{id} modifiée",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _context.Reunions.FindAsync(id);
            if (r == null) return NotFound();
            _context.Reunions.Remove(r);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Suppression réunion",
                Description = $"Réunion #{id} supprimée",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
