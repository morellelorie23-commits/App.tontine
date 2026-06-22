using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembreController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MembreController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembreModel>>> GetAll()
            => await _context.Membres.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<MembreModel>> GetById(int id)
        {
            var m = await _context.Membres.FindAsync(id);
            return m == null ? NotFound() : m;
        }

        [HttpPost]
        public async Task<ActionResult<MembreModel>> Create(MembreModel membre)
        {
            _context.Membres.Add(membre);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouveau membre",
                Description = $"Membre {membre.Nom} {membre.Prenom} enregistré",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = membre.IdMembre }, membre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MembreModel membre)
        {
            if (id != membre.IdMembre) return BadRequest();
            _context.Entry(membre).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Modification membre",
                Description = $"Fiche du membre {membre.Nom} {membre.Prenom} mise à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.Membres.FindAsync(id);
            if (m == null) return NotFound();
            _context.Membres.Remove(m);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Suppression membre",
                Description = $"Membre {m.Nom} {m.Prenom} supprimé",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("scores")]
        public async Task<IActionResult> GetAllScores()
        {
            var membres = await _context.Membres.Select(m => m.IdMembre).ToListAsync();
            var result  = new List<object>();
            foreach (var id in membres)
                result.Add(await ComputeScore(id));
            return Ok(result);
        }

        [HttpGet("{id}/score")]
        public async Task<IActionResult> GetScore(int id)
        {
            if (!await _context.Membres.AnyAsync(m => m.IdMembre == id)) return NotFound();
            return Ok(await ComputeScore(id));
        }

        private async Task<object> ComputeScore(int id)
        {
            var cotisations = await _context.Cotisations.Where(c => c.IdMembre == id).ToListAsync();
            var prets       = await _context.Prets.Where(p => p.IdMembre == id).ToListAsync();

            double ponctualite = 1.0;
            double regularite  = 1.0;
            double solvabilite = 1.0;

            if (cotisations.Any())
            {
                int total    = cotisations.Count;
                int payes    = cotisations.Count(c => c.Statut == "Payé");
                int enRetard = cotisations.Count(c => c.Statut == "En retard");
                ponctualite  = (double)payes    / total;
                regularite   = 1.0 - (double)enRetard / total;
            }

            if (prets.Any())
            {
                decimal totalPrete   = prets.Sum(p => p.Montant);
                decimal totalRembours = prets.Sum(p => p.MontantRemboursé);
                solvabilite = totalPrete > 0 ? Math.Min(1.0, (double)(totalRembours / totalPrete)) : 1.0;
            }

            double score = (ponctualite * 0.50 + regularite * 0.30 + solvabilite * 0.20) * 100;
            score = Math.Round(score, 1);

            string niveau = score >= 80 ? "Excellent" : score >= 60 ? "Bon" : score >= 40 ? "Moyen" : "Faible";
            string couleur = score >= 80 ? "#0F6E56" : score >= 60 ? "#F57F17" : score >= 40 ? "#E65100" : "#C62828";

            return new
            {
                IdMembre    = id,
                Score       = score,
                Niveau      = niveau,
                Couleur     = couleur,
                Ponctualite = Math.Round(ponctualite * 100, 1),
                Regularite  = Math.Round(regularite  * 100, 1),
                Solvabilite = Math.Round(solvabilite  * 100, 1),
                NbCotisations = cotisations.Count,
                NbPrets       = prets.Count
            };
        }

        [HttpGet("{id}/releve")]
        public async Task<IActionResult> GetReleve(int id)
        {
            var membre = await _context.Membres.FindAsync(id);
            if (membre == null) return NotFound();

            var cotisations = await _context.Cotisations
                .Where(c => c.IdMembre == id)
                .Include(c => c.Tontine)
                .Include(c => c.Cycle)
                .Select(c => new {
                    c.IdCotisation, c.Montant, c.DateCotisation, c.Statut, c.Notes,
                    LibelleTontine = c.Tontine != null ? c.Tontine.Libelle : "",
                    NomCycle       = c.Cycle   != null ? c.Cycle.NomCycle  : ""
                })
                .OrderByDescending(c => c.DateCotisation)
                .ToListAsync();

            var versements = await _context.Versements
                .Where(v => v.IdMembre == id)
                .Include(v => v.Tontine)
                .Include(v => v.Cycle)
                .Select(v => new {
                    v.IdVersement, v.Montant, v.DateVersement, v.Notes,
                    LibelleTontine = v.Tontine != null ? v.Tontine.Libelle : "",
                    NomCycle       = v.Cycle   != null ? v.Cycle.NomCycle  : ""
                })
                .OrderByDescending(v => v.DateVersement)
                .ToListAsync();

            var prets = await _context.Prets
                .Where(p => p.IdMembre == id)
                .OrderByDescending(p => p.DateCreation)
                .ToListAsync();

            return Ok(new
            {
                Membre = new { membre.IdMembre, membre.Nom, membre.Prenom, membre.Telephone, membre.Email, membre.Photo },
                Cotisations  = cotisations,
                Versements   = versements,
                Prets        = prets,
                TotalCotisations = cotisations.Sum(c => c.Montant),
                TotalVersements  = versements.Sum(v => v.Montant),
                TotalPrets       = prets.Sum(p => p.Montant)
            });
        }
    }
}
