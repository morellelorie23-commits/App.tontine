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
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAll()
        {
            var list = await _context.CycleTontines
                .Include(ct => ct.Cycle)
                .Include(ct => ct.Tontine)
                .Select(ct => new {
                    ct.Id,
                    ct.IdCycle,
                    NomCycle = ct.Cycle != null ? ct.Cycle.NomCycle : "",
                    ct.IdTontine,
                    LibelleTontine = ct.Tontine != null ? ct.Tontine.Libelle : ""
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CycleTontineModel>> GetById(int id)
        {
            var ct = await _context.CycleTontines.FindAsync(id);
            return ct == null ? NotFound() : ct;
        }

        [HttpPost]
        public async Task<ActionResult<CycleTontineModel>> Create(CycleTontineModel cycleTontine)
        {
            var existe = await _context.CycleTontines
                .AnyAsync(ct => ct.IdCycle == cycleTontine.IdCycle && ct.IdTontine == cycleTontine.IdTontine);
            if (existe)
                return BadRequest("Cette liaison Cycle-Tontine existe déjà.");

            _context.CycleTontines.Add(cycleTontine);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = cycleTontine.Id }, cycleTontine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CycleTontineModel cycleTontine)
        {
            if (id != cycleTontine.Id) return BadRequest();
            _context.Entry(cycleTontine).State = EntityState.Modified;
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

        [HttpPost("{id}/repartir")]
        public async Task<IActionResult> Repartir(int id)
        {
            var ct = await _context.CycleTontines
                .Include(x => x.Tontine)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (ct == null) return NotFound();

            var montant = ct.Tontine?.Montant ?? 0m;
            if (montant <= 0)
                return BadRequest("Le montant de la tontine n'est pas défini.");

            var membres = await _context.MembreCycleTontines
                .Where(m => m.IdCycle == ct.IdCycle && m.IdTontine == ct.IdTontine)
                .ToListAsync();

            if (!membres.Any())
                return BadRequest("Aucun membre inscrit dans ce cycle-tontine.");

            int crees = 0;
            foreach (var m in membres)
            {
                var dejaExiste = await _context.Cotisations.AnyAsync(c =>
                    c.IdMembre == m.IdMembre &&
                    c.IdCycle  == ct.IdCycle &&
                    c.IdTontine == ct.IdTontine);
                if (dejaExiste) continue;

                _context.Cotisations.Add(new CotisationModel
                {
                    IdMembre      = m.IdMembre,
                    IdTontine     = ct.IdTontine,
                    IdCycle       = ct.IdCycle,
                    Montant       = montant,
                    DateCotisation = DateTime.Now,
                    Statut        = "En attente"
                });
                crees++;
            }

            if (crees > 0)
            {
                _context.Journals.Add(new JournalActiviteModel
                {
                    Action      = "Répartition contributions",
                    Description = $"{crees} cotisation(s) créée(s) pour le cycle-tontine #{id}",
                    Utilisateur = "Système",
                    DateAction  = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return Ok(new { Crees = crees, Membres = membres.Count });
        }
    }
}
