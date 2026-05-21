using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembreCycleTontineController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MembreCycleTontineController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAll()
        {
            var list = await _context.MembreCycleTontines
                .Include(m => m.Membre)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .Select(m => new {
                    m.Id,
                    m.IdMembre,
                    NomMembre      = m.Membre != null ? m.Membre.Nom + " " + m.Membre.Prenom : "",
                    m.IdCycle,
                    NomCycle       = m.Cycle != null ? m.Cycle.NomCycle : "",
                    m.IdTontine,
                    LibelleTontine = m.Tontine != null ? m.Tontine.Libelle : "",
                    m.Matricule,
                    m.NumeroOrdre,
                    m.NombreParts
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembreCycleTontineModel>> GetById(int id)
        {
            var m = await _context.MembreCycleTontines
                .Include(m => m.Membre)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .FirstOrDefaultAsync(m => m.Id == id);
            return m == null ? NotFound() : m;
        }

        [HttpGet("tours/{idCycle}/{idTontine}")]
        public async Task<IActionResult> GetTours(int idCycle, int idTontine)
        {
            var membres = await _context.MembreCycleTontines
                .Include(m => m.Membre)
                .Where(m => m.IdCycle == idCycle && m.IdTontine == idTontine)
                .OrderBy(m => m.NumeroOrdre)
                .ToListAsync();

            var versements = await _context.Versements
                .Where(v => v.IdCycle == idCycle && v.IdTontine == idTontine)
                .ToListAsync();

            var result = membres.Select(m =>
            {
                var v = versements.FirstOrDefault(v => v.IdMembre == m.IdMembre);
                return new
                {
                    Rang          = m.NumeroOrdre,
                    m.Matricule,
                    m.IdMembre,
                    NomMembre     = m.Membre != null ? m.Membre.Nom + " " + m.Membre.Prenom : "",
                    PhotoUrl      = m.Membre != null && m.Membre.Photo != null ? "/uploads/" + m.Membre.Photo : null,
                    m.NombreParts,
                    ARecu         = v != null,
                    DateVersement = v?.DateVersement,
                    MontantVerse  = v?.Montant,
                    MontantNet    = v?.MontantNet,
                    Statut        = v != null ? "Reçu" : "En attente"
                };
            }).ToList();

            return Ok(result);
        }

        // Retourne le montant de cotisation attendu pour un membre (montant_base × total de ses parts)
        [HttpGet("cotisation-attendue/{idMembre}/{idCycle}/{idTontine}")]
        public async Task<IActionResult> GetCotisationAttendue(int idMembre, int idCycle, int idTontine)
        {
            var tontine    = await _context.Tontines.FindAsync(idTontine);
            var partsTotal = await _context.MembreCycleTontines
                .Where(m => m.IdMembre == idMembre && m.IdCycle == idCycle && m.IdTontine == idTontine)
                .SumAsync(m => m.NombreParts);

            var montantBase    = tontine?.Montant ?? 0m;
            var montantAttendu = montantBase * partsTotal;

            return Ok(new { partsTotal, montantBase, montantAttendu });
        }

        [HttpPost]
        public async Task<ActionResult<MembreCycleTontineModel>> Create(MembreCycleTontineModel mct)
        {
            _context.MembreCycleTontines.Add(mct);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Membre inscrit au cycle",
                Description = $"Membre (id={mct.IdMembre}) inscrit à la tontine (id={mct.IdTontine}) pour le cycle (id={mct.IdCycle}), matricule {mct.Matricule}, ordre {mct.NumeroOrdre}",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = mct.Id }, mct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MembreCycleTontineModel mct)
        {
            if (id != mct.Id) return BadRequest();
            _context.Entry(mct).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Inscription cycle modifiée",
                Description = $"Inscription membre-cycle-tontine (id={id}) mise à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.MembreCycleTontines.FindAsync(id);
            if (m == null) return NotFound();
            _context.MembreCycleTontines.Remove(m);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Inscription cycle supprimée",
                Description = $"Membre (id={m.IdMembre}) retiré du cycle (id={m.IdCycle}) / tontine (id={m.IdTontine})",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}