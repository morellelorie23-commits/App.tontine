using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Helpers;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersementController : ControllerBase
    {
        private readonly AppDbContext _context;
        public VersementController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Versements
                .Include(v => v.Membre)
                .Include(v => v.Tontine)
                .Include(v => v.Cycle)
                .Select(v => new {
                    v.IdVersement,
                    v.IdMembre,
                    NomMembre      = v.Membre != null ? v.Membre.Nom + " " + v.Membre.Prenom : "",
                    v.IdTontine,
                    LibelleTontine = v.Tontine != null ? v.Tontine.Libelle : "",
                    v.IdCycle,
                    NomCycle       = v.Cycle != null ? v.Cycle.NomCycle : "",
                    v.Montant,
                    v.DateVersement,
                    v.Notes
                })
                .OrderByDescending(v => v.DateVersement)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var v = await _context.Versements.FindAsync(id);
            return v == null ? NotFound() : Ok(v);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VersementModel versement)
        {
            versement.DateVersement = DateTime.Now;

            var membre  = await _context.Membres.FindAsync(versement.IdMembre);
            var tontine = await _context.Tontines.FindAsync(versement.IdTontine);
            var nomM    = membre  != null ? $"{membre.Nom} {membre.Prenom}"  : $"Membre #{versement.IdMembre}";
            var nomT    = tontine != null ? tontine.Libelle ?? "" : $"Tontine #{versement.IdTontine}";

            // Calculer la déduction = montant_base × total des parts du membre dans ce cycle/tontine
            var montantBase = tontine?.Montant ?? 0m;
            var partsTotal  = await _context.MembreCycleTontines
                .Where(m => m.IdMembre  == versement.IdMembre
                         && m.IdCycle   == versement.IdCycle
                         && m.IdTontine == versement.IdTontine)
                .SumAsync(m => m.NombreParts);

            versement.MontantDeduction  = Math.Round(montantBase * partsTotal, 0);
            versement.MontantCommission = Math.Round((versement.Montant - versement.MontantDeduction) * ComptabiliteHelper.TauxCommission, 0);
            versement.MontantNet        = versement.Montant - versement.MontantDeduction - versement.MontantCommission;

            _context.Versements.Add(versement);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouveau versement",
                Description = $"Versement de {versement.Montant:N0} FCFA à {nomM} (commission {versement.MontantCommission:N0} FCFA)",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync(); // obtenir IdVersement

            // Écriture 1 — versement au bénéficiaire + commission transit
            await ComptabiliteHelper.EcritureVersement(_context, versement, nomM, nomT);

            // Écriture 2 — encaissement commission (transit → banque propre + TVA)
            await ComptabiliteHelper.EcritureCommission(
                _context, versement.IdVersement, versement.IdTontine,
                versement.MontantCommission, nomT);

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = versement.IdVersement }, versement);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VersementModel versement)
        {
            if (id != versement.IdVersement) return BadRequest();
            _context.Entry(versement).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Modification versement",
                Description = $"Versement #{id} modifié — montant : {versement.Montant:N0} FCFA",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var v = await _context.Versements.FindAsync(id);
            if (v == null) return NotFound();
            _context.Versements.Remove(v);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Suppression versement",
                Description = $"Versement #{id} supprimé (membre #{v.IdMembre})",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
