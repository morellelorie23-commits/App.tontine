using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AmendeController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AmendeController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var amendes = await _db.Amendes
                .Include(a => a.Membre)
                .Include(a => a.Cycle)
                .Include(a => a.Tontine)
                .Include(a => a.Penalite)
                .Select(a => new {
                    a.IdAmende,
                    a.IdCotisation,
                    a.IdMembre,
                    a.IdCycle,
                    a.IdTontine,
                    a.IdPenalite,
                    a.TauxApplique,
                    a.MontantCotisation,
                    a.MontantAmende,
                    a.DateCalcul,
                    a.Statut,
                    a.DatePaiement,
                    NomMembre      = a.Membre != null ? a.Membre.Prenom + " " + a.Membre.Nom : "",
                    NomCycle       = a.Cycle != null ? a.Cycle.NomCycle : "",
                    LibelleTontine = a.Tontine != null ? a.Tontine.Libelle : "",
                    LibellePenalite = a.Penalite != null ? a.Penalite.Libelle : ""
                })
                .ToListAsync();

            return Ok(amendes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var amende = await _db.Amendes
                .Include(a => a.Membre)
                .Include(a => a.Cycle)
                .Include(a => a.Tontine)
                .Include(a => a.Penalite)
                .FirstOrDefaultAsync(a => a.IdAmende == id);

            if (amende == null) return NotFound();
            return Ok(amende);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AmendeModel amende)
        {
            amende.DateCalcul = DateTime.Now;
            _db.Amendes.Add(amende);
            await _db.SaveChangesAsync();

            _db.Journals.Add(new JournalActiviteModel
            {
                Action      = "Création amende",
                Description = $"Amende de {amende.MontantAmende:N0} FCFA créée pour le membre #{amende.IdMembre}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = amende.IdAmende }, amende);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AmendeModel updated)
        {
            var amende = await _db.Amendes.FindAsync(id);
            if (amende == null) return NotFound();

            amende.Statut       = updated.Statut;
            amende.DatePaiement = updated.DatePaiement;
            await _db.SaveChangesAsync();

            _db.Journals.Add(new JournalActiviteModel
            {
                Action      = "Modification amende",
                Description = $"Amende #{id} mise à jour — statut : {amende.Statut}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var amende = await _db.Amendes.FindAsync(id);
            if (amende == null) return NotFound();

            _db.Amendes.Remove(amende);

            _db.Journals.Add(new JournalActiviteModel
            {
                Action      = "Suppression amende",
                Description = $"Amende #{id} supprimée",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // Génère les amendes pour toutes les cotisations "En retard" d'un cycle.
        // Applique TauxAvant si la cotisation est dans la période du cycle,
        // TauxApres si elle dépasse la date de fin du cycle.
        // Les cotisations déjà liées à une amende existante sont ignorées.
        [HttpPost("generer/{cycleId}")]
        public async Task<IActionResult> Generer(int cycleId)
        {
            var cycle = await _db.Cycles.FindAsync(cycleId);
            if (cycle == null) return NotFound("Cycle introuvable.");

            var cotisationsEnRetard = await _db.Cotisations
                .Where(c => c.IdCycle == cycleId && c.Statut == "En retard")
                .ToListAsync();

            if (!cotisationsEnRetard.Any())
                return Ok(new { generees = 0, message = "Aucune cotisation en retard pour ce cycle." });

            var existingIds = await _db.Amendes
                .Where(a => a.IdCycle == cycleId)
                .Select(a => a.IdCotisation)
                .ToListAsync();

            var ctpList = await _db.CycleTontinePenalites
                .Where(x => x.IdCycle == cycleId)
                .ToListAsync();

            int count = 0;
            foreach (var cot in cotisationsEnRetard)
            {
                if (existingIds.Contains(cot.IdCotisation)) continue;

                var ctp = ctpList.FirstOrDefault(x => x.IdTontine == cot.IdTontine);

                // TauxApres si la cotisation dépasse la date de fin du cycle
                bool estApresEcheance = cycle.DateFin.HasValue &&
                    cot.DateCotisation.Date > cycle.DateFin.Value.ToDateTime(TimeOnly.MinValue).Date;

                decimal taux = estApresEcheance
                    ? (ctp?.TauxApres ?? ctp?.TauxAvant ?? 5m)
                    : (ctp?.TauxAvant ?? 5m);

                _db.Amendes.Add(new AmendeModel
                {
                    IdCotisation     = cot.IdCotisation,
                    IdMembre         = cot.IdMembre,
                    IdCycle          = cot.IdCycle,
                    IdTontine        = cot.IdTontine,
                    IdPenalite       = ctp?.IdPenalite,
                    TauxApplique     = taux,
                    MontantCotisation = cot.Montant,
                    MontantAmende    = cot.Montant * (taux / 100m),
                    DateCalcul       = DateTime.Now,
                    Statut           = "En attente"
                });
                count++;
            }

            await _db.SaveChangesAsync();

            _db.Journals.Add(new JournalActiviteModel
            {
                Action      = "Génération amendes",
                Description = $"{count} amende(s) générée(s) pour le cycle #{cycleId}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return Ok(new { generees = count });
        }

        // Marque une amende comme payée
        [HttpPost("{id}/payer")]
        public async Task<IActionResult> Payer(int id)
        {
            var amende = await _db.Amendes.FindAsync(id);
            if (amende == null) return NotFound();

            amende.Statut       = "Payée";
            amende.DatePaiement = DateTime.Now;
            await _db.SaveChangesAsync();

            _db.Journals.Add(new JournalActiviteModel
            {
                Action      = "Paiement amende",
                Description = $"Amende #{id} marquée comme payée — {amende.MontantAmende:N0} FCFA",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
