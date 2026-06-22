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
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAll()
        {
            var list = await _context.CycleTontinePenalites
                .Include(c => c.Cycle)
                .Include(c => c.Tontine)
                .Include(c => c.Penalite)
                .Select(c => new {
                    c.Id,
                    c.IdCycle,
                    NomCycle = c.Cycle != null ? c.Cycle.NomCycle : "",
                    c.IdTontine,
                    LibelleTontine = c.Tontine != null ? c.Tontine.Libelle : "",
                    c.IdPenalite,
                    LibellePenalite = c.Penalite != null ? c.Penalite.Libelle : "",
                    c.TauxAvant,
                    c.TauxApres
                })
                .ToListAsync();
            return Ok(list);
        }

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
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Pénalité liée au cycle",
                Description = $"Pénalité (id={ctp.IdPenalite}) associée au cycle (id={ctp.IdCycle}) / tontine (id={ctp.IdTontine})",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ctp.Id }, ctp);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CycleTontinePenaliteModel ctp)
        {
            if (id != ctp.Id) return BadRequest();
            _context.Entry(ctp).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Pénalité cycle modifiée",
                Description = $"Lien pénalité-cycle-tontine (id={id}) mis à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.CycleTontinePenalites.FindAsync(id);
            if (c == null) return NotFound();
            _context.CycleTontinePenalites.Remove(c);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Pénalité cycle supprimée",
                Description = $"Lien pénalité (id={c.IdPenalite}) / cycle (id={c.IdCycle}) supprimé",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}