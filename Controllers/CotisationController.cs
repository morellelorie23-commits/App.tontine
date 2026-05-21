using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Helpers;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CotisationController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CotisationController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Cotisations
                .Include(c => c.Membre)
                .Include(c => c.Tontine)
                .Include(c => c.Cycle)
                .Include(c => c.Mandataire)
                .Select(c => new {
                    c.IdCotisation,
                    c.IdMembre,
                    NomMembre       = c.Membre != null ? c.Membre.Nom + " " + c.Membre.Prenom : "",
                    c.IdTontine,
                    LibelleTontine  = c.Tontine != null ? c.Tontine.Libelle : "",
                    c.IdCycle,
                    NomCycle        = c.Cycle != null ? c.Cycle.NomCycle : "",
                    c.Montant,
                    c.DateCotisation,
                    c.Statut,
                    c.Notes,
                    c.IdMandataire,
                    NomMandataire   = c.Mandataire != null ? c.Mandataire.Nom + " " + c.Mandataire.Prenom : "",
                    c.ModePaiement
                })
                .OrderByDescending(c => c.DateCotisation)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _context.Cotisations
                .Include(x => x.Membre)
                .Include(x => x.Tontine)
                .Include(x => x.Cycle)
                .Include(x => x.Mandataire)
                .Where(x => x.IdCotisation == id)
                .Select(x => new {
                    x.IdCotisation,
                    x.IdMembre,
                    NomMembre      = x.Membre != null ? x.Membre.Nom + " " + x.Membre.Prenom : "",
                    x.IdTontine,
                    LibelleTontine = x.Tontine != null ? x.Tontine.Libelle : "",
                    x.IdCycle,
                    NomCycle       = x.Cycle != null ? x.Cycle.NomCycle : "",
                    x.Montant,
                    x.DateCotisation,
                    x.Statut,
                    x.Notes,
                    x.IdMandataire,
                    NomMandataire  = x.Mandataire != null ? x.Mandataire.Nom + " " + x.Mandataire.Prenom : "",
                    x.ModePaiement
                })
                .FirstOrDefaultAsync();
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CotisationModel cotisation)
        {
            cotisation.DateCotisation = DateTime.Now;
            cotisation.ModePaiement ??= "Cash";
            _context.Cotisations.Add(cotisation);

            // Résoudre les noms pour le libellé comptable
            var membre  = await _context.Membres.FindAsync(cotisation.IdMembre);
            var tontine = await _context.Tontines.FindAsync(cotisation.IdTontine);
            var nomM    = membre  != null ? $"{membre.Nom} {membre.Prenom}"  : $"Membre #{cotisation.IdMembre}";
            var nomT    = tontine != null ? tontine.Libelle ?? "" : $"Tontine #{cotisation.IdTontine}";

            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouvelle cotisation",
                Description = $"Cotisation {cotisation.ModePaiement} de {cotisation.Montant:N0} FCFA — {nomM}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });

            await _context.SaveChangesAsync(); // obtenir IdCotisation

            await ComptabiliteHelper.EcritureCotisation(
                _context, cotisation, cotisation.ModePaiement, nomM, nomT);

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = cotisation.IdCotisation }, cotisation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CotisationModel cotisation)
        {
            if (id != cotisation.IdCotisation) return BadRequest();
            _context.Entry(cotisation).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Modification cotisation",
                Description = $"Cotisation #{id} modifiée — statut : {cotisation.Statut}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Cotisations.FindAsync(id);
            if (c == null) return NotFound();
            _context.Cotisations.Remove(c);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Suppression cotisation",
                Description = $"Cotisation #{id} supprimée (membre #{c.IdMembre})",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/payer-cash")]
        public async Task<IActionResult> PayerCash(int id)
        {
            var c = await _context.Cotisations.FindAsync(id);
            if (c == null) return NotFound();

            var ancienStatut = c.Statut;
            c.Statut       = "Payé";
            c.ModePaiement = "Cash";

            var membre  = await _context.Membres.FindAsync(c.IdMembre);
            var tontine = await _context.Tontines.FindAsync(c.IdTontine);
            var nomM    = membre  != null ? $"{membre.Nom} {membre.Prenom}"  : $"#{c.IdMembre}";
            var nomT    = tontine != null ? tontine.Libelle ?? "" : $"#{c.IdTontine}";

            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Paiement cash",
                Description = $"Cotisation #{id} marquée Payée en espèces — {nomM}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Générer écriture comptable uniquement si elle n'existe pas déjà
            var dejaComptabilise = await _context.EcrituresComptables
                .AnyAsync(e => e.IdCotisation == id);
            if (!dejaComptabilise)
            {
                await ComptabiliteHelper.EcritureCotisation(_context, c, "Cash", nomM, nomT);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }

        [HttpGet("livre/{idCycle}/{idTontine}")]
        public async Task<IActionResult> GetLivre(int idCycle, int idTontine)
        {
            var depots = await _context.Cotisations
                .Include(c => c.Membre)
                .Where(c => c.IdCycle == idCycle && c.IdTontine == idTontine && c.Statut == "Payé")
                .Select(c => new {
                    Date        = c.DateCotisation,
                    Type        = "Dépôt",
                    Description = "Cotisation",
                    c.Montant,
                    NomMembre     = c.Membre != null ? c.Membre.Nom + " " + c.Membre.Prenom : "",
                    ModePaiement  = c.ModePaiement ?? "Cash"
                })
                .ToListAsync();

            var retraits = await _context.Versements
                .Include(v => v.Membre)
                .Where(v => v.IdCycle == idCycle && v.IdTontine == idTontine)
                .Select(v => new {
                    Date        = v.DateVersement,
                    Type        = "Retrait",
                    Description = "Versement bénéficiaire",
                    v.Montant,
                    NomMembre    = v.Membre != null ? v.Membre.Nom + " " + v.Membre.Prenom : "",
                    ModePaiement = "Espèces"
                })
                .ToListAsync();

            var lignes = depots
                .Select(d => new { d.Date, d.Type, d.Description, d.Montant, d.NomMembre, d.ModePaiement })
                .Concat(retraits.Select(r => new { r.Date, r.Type, r.Description, r.Montant, r.NomMembre, r.ModePaiement }))
                .OrderBy(l => l.Date)
                .ToList();

            decimal solde = 0;
            var lignesAvecSolde = lignes.Select(l =>
            {
                solde += l.Type == "Dépôt" ? l.Montant : -l.Montant;
                return new { l.Date, l.Type, l.Description, l.Montant, l.NomMembre, l.ModePaiement, Solde = solde };
            }).ToList();

            return Ok(new
            {
                TotalDepots   = depots.Sum(d => d.Montant),
                TotalRetraits = retraits.Sum(r => r.Montant),
                SoldeActuel   = depots.Sum(d => d.Montant) - retraits.Sum(r => r.Montant),
                Lignes        = lignesAvecSolde
            });
        }

        [HttpPost("appliquerpenalites")]
        public async Task<IActionResult> AppliquerPenalites()
        {
            var now = DateTime.Now;
            var retard = await _context.Cotisations
                .Where(c => c.Statut == "En attente" && c.DateCotisation < now)
                .ToListAsync();
            foreach (var c in retard)
                c.Statut = "En retard";
            if (retard.Any())
            {
                _context.Journals.Add(new JournalActiviteModel
                {
                    Action      = "Penalites appliquees",
                    Description = $"{retard.Count} cotisation(s) marquee(s) En retard automatiquement",
                    Utilisateur = "Systeme",
                    DateAction  = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            return Ok(new { MisAJour = retard.Count });
        }
    }
}
