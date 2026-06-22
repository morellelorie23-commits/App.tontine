using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaisieSeanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SaisieSeanceController(AppDbContext context) => _context = context;

        // GET api/SaisieSeance/membres?idTontine=1&idCycle=1
        [HttpGet("membres")]
        public async Task<IActionResult> GetMembres(int idTontine, int idCycle)
        {
            var tontine = await _context.Tontines.FindAsync(idTontine);
            if (tontine == null) return NotFound();

            var inscriptions = await _context.MembreCycleTontines
                .Include(m => m.Membre)
                .Where(m => m.IdTontine == idTontine && m.IdCycle == idCycle)
                .OrderBy(m => m.NumeroOrdre)
                .ToListAsync();

            var membres = inscriptions.Select(ins => new
            {
                idMembre  = ins.IdMembre,
                nomPrenom = ins.Membre != null ? ins.Membre.Nom + " " + ins.Membre.Prenom : "",
                mtAttendu = (tontine.Montant ?? 0) * ins.NombreParts
            });

            return Ok(membres);
        }

        // GET api/SaisieSeance/reunions?idTontine=1&idCycle=1
        [HttpGet("reunions")]
        public async Task<IActionResult> GetReunions(int idTontine, int idCycle)
        {
            var reunions = await _context.Reunions
                .Where(r => r.IdTontine == idTontine && r.IdCycle == idCycle)
                .OrderByDescending(r => r.DateReunion)
                .Select(r => new { r.IdReunion, r.DateReunion, r.Objet, r.Lieu })
                .ToListAsync();
            return Ok(reunions);
        }

        // GET api/SaisieSeance/data?idTontine=1&idReunion=2&idCycle=1
        [HttpGet("data")]
        public async Task<IActionResult> GetData(int idTontine, int idReunion, int idCycle)
        {
            var tontine = await _context.Tontines.FindAsync(idTontine);
            if (tontine == null) return NotFound("Tontine introuvable");

            // Membres inscrits, triés par numéro d'ordre
            var inscriptions = await _context.MembreCycleTontines
                .Include(m => m.Membre)
                .Where(m => m.IdTontine == idTontine && m.IdCycle == idCycle)
                .OrderBy(m => m.NumeroOrdre)
                .ToListAsync();

            // Cotisations déjà saisies pour cette séance
            var cotisationsSeance = await _context.Cotisations
                .Where(c => c.IdTontine == idTontine && c.IdCycle == idCycle && c.IdReunion == idReunion)
                .ToListAsync();

            // Solde caisse = total cotisations payées - total versements nets
            var totalCotise = await _context.Cotisations
                .Where(c => c.IdTontine == idTontine && c.IdCycle == idCycle && c.Statut == "Paye")
                .SumAsync(c => (decimal?)c.Montant) ?? 0;

            var totalVerse = await _context.Versements
                .Where(v => v.IdTontine == idTontine && v.IdCycle == idCycle)
                .SumAsync(v => (decimal?)v.MontantNet) ?? 0;

            var soldeCaisse = totalCotise - totalVerse;

            // Historique bénéficiaires du cycle
            var dejabeneficiaires = await _context.Versements
                .Include(v => v.Membre)
                .Where(v => v.IdTontine == idTontine && v.IdCycle == idCycle)
                .OrderBy(v => v.DateVersement)
                .Select(v => new
                {
                    nomPrenom = v.Membre != null ? v.Membre.Nom + " " + v.Membre.Prenom : "",
                    dateVersement = v.DateVersement.ToString("dd/MM/yyyy"),
                    montantNet = v.MontantNet
                })
                .ToListAsync();

            // Lignes tableau membres
            var lignes = inscriptions.Select(ins =>
            {
                var cot = cotisationsSeance.FirstOrDefault(c => c.IdMembre == ins.IdMembre);
                return new
                {
                    idMembre      = ins.IdMembre,
                    nomPrenom     = ins.Membre != null ? ins.Membre.Nom + " " + ins.Membre.Prenom : "",
                    mtAttendu     = (tontine.Montant ?? 0) * ins.NombreParts,
                    mtCotise      = cot?.Montant ?? 0,
                    penalite      = cot?.PenaliteSeance ?? 0,
                    mtEnchere     = cot?.MtEnchere ?? 0,
                    isGagnantEnchere = cot?.IsGagnantEnchere ?? false
                };
            }).ToList();

            return Ok(new { soldeCaisse, lignes, dejabeneficiaires });
        }

        // POST api/SaisieSeance/enregistrer
        [HttpPost("enregistrer")]
        public async Task<IActionResult> Enregistrer([FromBody] SaisieSeanceRequest req)
        {
            if (req.Lignes.Count(l => l.IsGagnantEnchere) > 1)
                return BadRequest("Un seul membre peut être gagnant de l'enchère par séance.");

            var mode = (string m) => m is "Cash" or "MTN" or "Orange Money" ? m : "Cash";

            foreach (var ligne in req.Lignes)
            {
                var existing = await _context.Cotisations
                    .FirstOrDefaultAsync(c => c.IdTontine == req.IdTontine
                        && c.IdCycle == req.IdCycle
                        && c.IdReunion == req.IdReunion
                        && c.IdMembre == ligne.IdMembre);

                if (existing != null)
                {
                    existing.Montant          = ligne.MtCotise;
                    existing.MtAttendu        = ligne.MtAttendu;
                    existing.MtEnchere        = ligne.MtEnchere;
                    existing.IsGagnantEnchere = ligne.IsGagnantEnchere;
                    existing.PenaliteSeance   = ligne.Penalite;
                    existing.ModePaiement     = mode(ligne.ModePaiement);
                    existing.Statut           = ligne.MtCotise >= ligne.MtAttendu ? "Paye" : "En attente";
                }
                else
                {
                    _context.Cotisations.Add(new CotisationModel
                    {
                        IdMembre         = ligne.IdMembre,
                        IdTontine        = req.IdTontine,
                        IdCycle          = req.IdCycle,
                        IdReunion        = req.IdReunion,
                        Montant          = ligne.MtCotise,
                        MtAttendu        = ligne.MtAttendu,
                        MtEnchere        = ligne.MtEnchere,
                        IsGagnantEnchere = ligne.IsGagnantEnchere,
                        PenaliteSeance   = ligne.Penalite,
                        DateCotisation   = DateTime.Now,
                        ModePaiement     = mode(ligne.ModePaiement),
                        Statut           = ligne.MtCotise >= ligne.MtAttendu ? "Paye" : "En attente"
                    });
                }
            }

            // Sauvegarde intermédiaire pour récupérer les IDs des cotisations
            await _context.SaveChangesAsync();

            // Créer les enregistrements PaiementMobile pour MTN / Orange Money
            foreach (var ligne in req.Lignes.Where(l => l.ModePaiement == "MTN" || l.ModePaiement == "Orange Money"))
            {
                var cot = await _context.Cotisations.FirstOrDefaultAsync(c =>
                    c.IdTontine == req.IdTontine && c.IdCycle == req.IdCycle &&
                    c.IdReunion == req.IdReunion && c.IdMembre == ligne.IdMembre);

                if (cot != null)
                {
                    _context.PaiementsMobiles.Add(new PaiementMobileModel
                    {
                        IdCotisation  = cot.IdCotisation,
                        Operateur     = ligne.ModePaiement == "MTN" ? "MTN" : "Orange",
                        Telephone     = ligne.Telephone ?? "",
                        Reference     = $"SEA-{req.IdReunion}-{ligne.IdMembre}-{DateTime.Now:yyyyMMddHHmm}",
                        Montant       = ligne.MtCotise,
                        Statut        = "Confirmé",
                        DateCreation  = DateTime.Now,
                        DateConfirmation = DateTime.Now
                    });
                }
            }

            // Versement bénéficiaire
            if (req.IdBeneficiaire.HasValue && req.MontantBeneficie > 0)
            {
                _context.Versements.Add(new VersementModel
                {
                    IdMembre      = req.IdBeneficiaire.Value,
                    IdTontine     = req.IdTontine,
                    IdCycle       = req.IdCycle,
                    Montant       = req.MontantBeneficie,
                    MontantNet    = req.MontantBeneficie - req.RetourCaisse,
                    DateVersement = DateTime.Now,
                    Notes         = $"Bénéficiaire séance — réunion #{req.IdReunion}"
                });
            }

            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "SaisieSeance",
                Description = $"Saisie séance réunion #{req.IdReunion}, tontine #{req.IdTontine} — {req.Lignes.Count} cotisation(s)",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Séance enregistrée avec succès." });
        }
    }

    public class SaisieSeanceRequest
    {
        public int IdTontine { get; set; }
        public int IdReunion { get; set; }
        public int IdCycle { get; set; }
        public int? IdBeneficiaire { get; set; }
        public decimal MontantBeneficie { get; set; }
        public decimal RetourCaisse { get; set; }
        public List<LigneSeanceRequest> Lignes { get; set; } = new();
    }

    public class LigneSeanceRequest
    {
        public int IdMembre { get; set; }
        public decimal MtAttendu { get; set; }
        public decimal MtCotise { get; set; }
        public decimal Penalite { get; set; }
        public decimal MtEnchere { get; set; }
        public bool IsGagnantEnchere { get; set; }
        public string ModePaiement { get; set; } = "Cash";
        public string Telephone { get; set; } = "";
    }
}
