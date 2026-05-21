using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PretController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PretController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPrets()
        {
            var prets = await _context.Prets.ToListAsync();

            // Auto-marquer En retard si date dépassée
            var now = DateTime.Now;
            bool changed = false;
            foreach (var p in prets.Where(p => p.Statut == "Approuvé" && p.DateRemboursement.HasValue && p.DateRemboursement < now))
            {
                p.Statut = "En retard";
                changed = true;
            }
            if (changed) await _context.SaveChangesAsync();

            var membres = await _context.Membres.ToDictionaryAsync(m => m.IdMembre, m => $"{m.Nom} {m.Prenom}");

            return prets.Select(p => (object)new
            {
                p.IdPret,
                p.IdMembre,
                NomMembre         = membres.TryGetValue(p.IdMembre, out var nom) ? nom : "",
                p.Montant,
                p.Taux,
                p.DatePret,
                p.DateRemboursement,
                p.Statut,
                p.Description,
                p.MontantRemboursé,
                p.DateCreation
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pret>> GetPret(int id)
        {
            var pret = await _context.Prets.FindAsync(id);
            if (pret == null) return NotFound();
            return pret;
        }

        [HttpGet("membre/{idMembre}")]
        public async Task<ActionResult<IEnumerable<Pret>>> GetPretsByMembre(int idMembre)
            => await _context.Prets.Where(p => p.IdMembre == idMembre).ToListAsync();

        [HttpPost]
        public async Task<ActionResult<Pret>> CreatePret(Pret pret)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Vérification éligibilité : garant obligatoire
            var hasGarant = await _context.Garants.AnyAsync(g => g.IdMembre == pret.IdMembre);
            if (!hasGarant)
                return BadRequest("Ce membre n'a pas de garant enregistré. Un garant est obligatoire pour obtenir un prêt.");

            // Vérification éligibilité : pas de prêt actif
            var pretActif = await _context.Prets.AnyAsync(p =>
                p.IdMembre == pret.IdMembre &&
                (p.Statut == "Approuvé" || p.Statut == "En retard" || p.Statut == "En attente"));
            if (pretActif)
                return BadRequest("Ce membre a déjà un prêt en cours non remboursé.");

            pret.DateCreation = DateTime.Now;
            _context.Prets.Add(pret);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouveau prêt",
                Description = $"Prêt de {pret.Montant:N0} FCFA accordé au membre #{pret.IdMembre}",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPret), new { id = pret.IdPret }, pret);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePret(int id, Pret pret)
        {
            if (id != pret.IdPret) return BadRequest();
            _context.Entry(pret).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePret(int id)
        {
            var pret = await _context.Prets.FindAsync(id);
            if (pret == null) return NotFound();
            _context.Prets.Remove(pret);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/approuver")]
        public async Task<IActionResult> Approuver(int id)
        {
            var pret = await _context.Prets.FindAsync(id);
            if (pret == null) return NotFound();
            if (pret.Statut != "En attente") return BadRequest("Seuls les prêts en attente peuvent être approuvés.");

            pret.Statut = "Approuvé";
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Prêt approuvé",
                Description = $"Prêt #{id} de {pret.Montant:N0} FCFA approuvé",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(pret);
        }

        [HttpPost("{id}/rembourser")]
        public async Task<IActionResult> Rembourser(int id)
        {
            var pret = await _context.Prets.FindAsync(id);
            if (pret == null) return NotFound();
            if (pret.Statut == "Remboursé") return BadRequest("Ce prêt est déjà remboursé.");

            pret.Statut           = "Remboursé";
            pret.MontantRemboursé = pret.Montant;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Prêt remboursé",
                Description = $"Prêt #{id} de {pret.Montant:N0} FCFA marqué comme remboursé",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(pret);
        }
    }
}
