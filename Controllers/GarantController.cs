using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GarantController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GarantController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGarants()
        {
            var garants = await _context.Garants.ToListAsync();
            var membres = await _context.Membres.ToDictionaryAsync(m => m.IdMembre, m => $"{m.Nom} {m.Prenom}");
            return garants.Select(g => (object)new
            {
                g.IdGarant, g.IdMembre,
                NomMembre = membres.TryGetValue(g.IdMembre, out var nom) ? nom : "",
                g.Nom, g.Prenom, g.Telephone, g.Email, g.Relation, g.Adresse, g.DateAjout
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GarantModel>> GetGarant(int id)
        {
            var g = await _context.Garants.FindAsync(id);
            return g == null ? NotFound() : g;
        }

        [HttpGet("membre/{idMembre}")]
        public async Task<ActionResult<GarantModel?>> GetByMembre(int idMembre)
        {
            var g = await _context.Garants.FirstOrDefaultAsync(g => g.IdMembre == idMembre);
            return g == null ? NotFound() : g;
        }

        [HttpGet("membre/{idMembre}/eligible")]
        public async Task<ActionResult<object>> CheckEligibilite(int idMembre)
        {
            var hasGarant = await _context.Garants.AnyAsync(g => g.IdMembre == idMembre);
            var pretActif = await _context.Prets.AnyAsync(p =>
                p.IdMembre == idMembre &&
                (p.Statut == "Approuvé" || p.Statut == "En retard" || p.Statut == "En attente"));

            return Ok(new
            {
                Eligible = hasGarant && !pretActif,
                HasGarant = hasGarant,
                PretActif = pretActif,
                Message = !hasGarant
                    ? "Ce membre n'a pas de garant enregistré."
                    : pretActif
                        ? "Ce membre a déjà un prêt en cours non remboursé."
                        : "Membre éligible au prêt."
            });
        }

        [HttpPost]
        public async Task<ActionResult<GarantModel>> CreateGarant(GarantModel garant)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Un membre ne peut avoir qu'un seul garant actif
            var existing = await _context.Garants.FirstOrDefaultAsync(g => g.IdMembre == garant.IdMembre);
            if (existing != null)
                return BadRequest("Ce membre a déjà un garant enregistré. Modifiez le garant existant.");

            garant.DateAjout = DateTime.Now;
            _context.Garants.Add(garant);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action = "Garant enregistré",
                Description = $"Garant {garant.Nom} {garant.Prenom} enregistré pour le membre #{garant.IdMembre}",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGarant), new { id = garant.IdGarant }, garant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGarant(int id, GarantModel garant)
        {
            if (id != garant.IdGarant) return BadRequest();
            _context.Entry(garant).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action = "Garant modifié",
                Description = $"Garant #{id} mis à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGarant(int id)
        {
            var g = await _context.Garants.FindAsync(id);
            if (g == null) return NotFound();
            _context.Garants.Remove(g);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action = "Garant supprimé",
                Description = $"Garant {g.Nom} {g.Prenom} retiré du membre #{g.IdMembre}",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
