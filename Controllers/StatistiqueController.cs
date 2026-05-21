using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatistiqueController : ControllerBase
    {
        private readonly AppDbContext _context;
        public StatistiqueController(AppDbContext context) => _context = context;

        [HttpGet("tontine/{id}")]
        public async Task<IActionResult> GetStatsTontine(int id)
        {
            var tontine = await _context.Tontines.FindAsync(id);
            if (tontine == null) return NotFound();

            var cotisations = await _context.Cotisations.Where(c => c.IdTontine == id).ToListAsync();
            var versements  = await _context.Versements.Where(v => v.IdTontine == id).ToListAsync();
            var participants = await _context.MembreCycleTontines
                .Where(m => m.IdTontine == id)
                .Select(m => m.IdMembre)
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                IdTontine          = id,
                Libelle            = tontine.Libelle,
                Frequence          = tontine.Frequence,
                MontantBase        = tontine.Montant ?? 0,
                NbParticipants     = participants,
                TotalCotisations   = cotisations.Sum(c => c.Montant),
                NbCotisations      = cotisations.Count,
                CotisationsPayees  = cotisations.Count(c => c.Statut == "Payé"),
                CotisationsAttente = cotisations.Count(c => c.Statut == "En attente"),
                CotisationsRetard  = cotisations.Count(c => c.Statut == "En retard"),
                TotalVersements    = versements.Sum(v => v.Montant),
                NbVersements       = versements.Count
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var membres   = await _context.Membres.ToListAsync();
            var tontines  = await _context.Tontines.ToListAsync();
            var cycles    = await _context.Cycles.ToListAsync();
            var postes    = await _context.Postes.ToListAsync();
            var penalites = await _context.Penalites.ToListAsync();
            var cotisations = await _context.Cotisations.ToListAsync();
            var versements  = await _context.Versements.ToListAsync();
            var prets       = await _context.Prets.ToListAsync();
            var amendes     = await _context.Amendes.ToListAsync();
            var journaux    = await _context.Journals
                .OrderByDescending(j => j.DateAction)
                .Take(10)
                .ToListAsync();

            var parSexe = membres
                .GroupBy(m => string.IsNullOrEmpty(m.Sexe) ? "Non renseigné" : (m.Sexe == "M" ? "Masculin" : "Féminin"))
                .ToDictionary(g => g.Key, g => g.Count());

            var parFrequence = tontines
                .GroupBy(t => string.IsNullOrEmpty(t.Frequence) ? "Non définie" : t.Frequence)
                .ToDictionary(g => g.Key, g => g.Count());

            var parStatut = cycles
                .GroupBy(c => string.IsNullOrEmpty(c.Statut) ? "Non défini" : c.Statut)
                .ToDictionary(g => g.Key, g => g.Count());

            var sixMoisAvant = DateTime.Now.AddMonths(-5);
            var inscriptionsParMois = membres
                .Where(m => m.DateInscription >= sixMoisAvant)
                .GroupBy(m => new { m.DateInscription.Year, m.DateInscription.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Mois  = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Total = g.Count()
                })
                .ToList();

            var parPays = membres
                .Where(m => !string.IsNullOrEmpty(m.Pays))
                .GroupBy(m => m.Pays!)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());

            var journalRecents = journaux.Select(j => new
            {
                j.IdJournal,
                j.Action,
                j.Description,
                j.Utilisateur,
                j.DateAction
            }).ToList();

            // Alertes
            var now = DateTime.Now;
            var pretsEnRetard       = prets.Count(p => p.Statut == "En retard");
            var cotisationsEnRetard = cotisations.Count(c => c.Statut == "En retard");
            var cotisationsEnAttente = cotisations.Count(c => c.Statut == "En attente");

            // Cotisations par mois (6 derniers mois)
            var sixMoisAvant2 = DateTime.Now.AddMonths(-5);
            var cotisationsParMois = cotisations
                .Where(c => c.DateCotisation >= sixMoisAvant2)
                .GroupBy(c => new { c.DateCotisation.Year, c.DateCotisation.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Mois   = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Total  = g.Count(),
                    Montant = g.Sum(c => c.Montant)
                })
                .ToList();

            return Ok(new
            {
                TotalMembres    = membres.Count,
                TotalTontines   = tontines.Count,
                TotalCycles     = cycles.Count,
                CyclesActifs    = cycles.Count(c => c.Statut == "Actif" || c.Statut == "En cours"),
                TotalPostes     = postes.Count,
                TotalPenalites  = penalites.Count,
                MontantTotal    = tontines.Sum(t => t.Montant ?? 0),
                TotalCotisations = cotisations.Sum(c => c.Montant),
                TotalVersements  = versements.Sum(v => v.Montant),
                PretsEnCours     = prets.Count(p => p.Statut != "Remboursé"),
                PretsEnRetard    = pretsEnRetard,
                CotisationsEnRetard  = cotisationsEnRetard,
                CotisationsEnAttente = cotisationsEnAttente,
                CotisationsPayees    = cotisations.Count(c => c.Statut == "Payé"),
                NbCotisations        = cotisations.Count,
                AmendesEnAttente          = amendes.Count(a => a.Statut == "En attente"),
                MontantAmendesEnAttente   = amendes.Where(a => a.Statut == "En attente").Sum(a => a.MontantAmende),
                ParSexe          = parSexe,
                ParFrequence     = parFrequence,
                ParStatut        = parStatut,
                InscriptionsParMois = inscriptionsParMois,
                CotisationsParMois  = cotisationsParMois,
                ParPays          = parPays,
                JournalRecents   = journalRecents
            });
        }
    }
}
