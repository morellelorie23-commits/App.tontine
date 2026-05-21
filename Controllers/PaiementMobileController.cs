using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaiementMobileController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PaiementMobileController(AppDbContext context) => _context = context;

        [HttpGet("cotisation/{idCotisation}")]
        public async Task<IActionResult> GetByCotisation(int idCotisation)
        {
            var paiements = await _context.PaiementsMobiles
                .Where(p => p.IdCotisation == idCotisation)
                .OrderByDescending(p => p.DateCreation)
                .ToListAsync();
            return Ok(paiements);
        }

        [HttpGet("statut/{reference}")]
        public async Task<IActionResult> GetStatut(string reference)
        {
            var p = await _context.PaiementsMobiles
                .FirstOrDefaultAsync(x => x.Reference == reference);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpPost("initier")]
        public async Task<IActionResult> Initier([FromBody] InitierPaiementRequest req)
        {
            if (req.Montant <= 0 || string.IsNullOrWhiteSpace(req.Telephone))
                return BadRequest("Téléphone et montant obligatoires.");

            var reference = $"TT-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var paiement = new PaiementMobileModel
            {
                IdCotisation  = req.IdCotisation,
                Telephone     = req.Telephone,
                Operateur     = req.Operateur,
                Montant       = req.Montant,
                Reference     = reference,
                Statut        = "En attente",
                DateCreation  = DateTime.Now
            };
            _context.PaiementsMobiles.Add(paiement);

            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Paiement mobile initié",
                Description = $"Demande {req.Operateur} de {req.Montant:N0} FCFA pour le {req.Telephone} (réf: {reference})",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });

            await _context.SaveChangesAsync();

            // TODO: appel réel MTN MoMo Collections API / Orange Money WebPay ici
            // Pour l'instant, on simule une réponse en attente.

            return Ok(new { Reference = reference, Statut = "En attente", Message = "Demande envoyée. Confirmez sur votre téléphone." });
        }

        // Webhook MTN MoMo — appelé par MTN lors de la confirmation
        [HttpPost("webhook/mtn")]
        public async Task<IActionResult> WebhookMtn([FromBody] WebhookPayload payload)
            => await TraiterWebhook(payload, "MTN");

        // Webhook Orange Money — appelé par Orange lors de la confirmation
        [HttpPost("webhook/orange")]
        public async Task<IActionResult> WebhookOrange([FromBody] WebhookPayload payload)
            => await TraiterWebhook(payload, "Orange");

        private async Task<IActionResult> TraiterWebhook(WebhookPayload payload, string operateur)
        {
            var paiement = await _context.PaiementsMobiles
                .FirstOrDefaultAsync(p => p.Reference == payload.Reference);
            if (paiement == null) return NotFound();

            paiement.Statut           = payload.Succes ? "Confirmé" : "Échoué";
            paiement.DateConfirmation = DateTime.Now;
            paiement.MessageErreur    = payload.MessageErreur;

            if (payload.Succes && paiement.IdCotisation.HasValue)
            {
                var cotisation = await _context.Cotisations.FindAsync(paiement.IdCotisation.Value);
                if (cotisation != null)
                {
                    cotisation.Statut = "Payé";
                    cotisation.Notes  = (cotisation.Notes ?? "") + $" | Payé via {operateur} Mobile Money (réf: {paiement.Reference})";
                }
            }

            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = $"Paiement mobile {paiement.Statut.ToLower()}",
                Description = $"Réf {paiement.Reference} — {operateur} — {(payload.Succes ? "succès" : "échec")}",
                Utilisateur = "Système",
                DateAction  = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Simulation de confirmation (dev/test uniquement)
        [HttpPost("simuler/{reference}")]
        public async Task<IActionResult> Simuler(string reference, [FromQuery] bool succes = true)
        {
            var payload = new WebhookPayload(reference, succes);
            var paiement = await _context.PaiementsMobiles.FirstOrDefaultAsync(p => p.Reference == reference);
            if (paiement == null) return NotFound();
            return await TraiterWebhook(payload, paiement.Operateur);
        }
    }

    public record InitierPaiementRequest(int? IdCotisation, string Telephone, string Operateur, decimal Montant);
    public record WebhookPayload(string Reference, bool Succes, string? MessageErreur = null);
}
