using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;
using tontine.WebAPI.Controllers;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompteUtilisateurController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CompteUtilisateurController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Comptes
                .OrderBy(c => c.Nom)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _context.Comptes.FindAsync(id);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompteUtilisateurModel compte)
        {
            compte.DateCreation = DateOnly.FromDateTime(DateTime.Now);
            if (!string.IsNullOrWhiteSpace(compte.MotDePasse))
                compte.MotDePasse = AuthController.HashPassword(compte.MotDePasse);
            _context.Comptes.Add(compte);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = compte.IdCompte }, compte);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CompteUtilisateurModel compte)
        {
            if (id != compte.IdCompte) return BadRequest();
            _context.Entry(compte).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Comptes.FindAsync(id);
            if (c == null) return NotFound();
            _context.Comptes.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/changepassword")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest req)
        {
            var compte = await _context.Comptes.FindAsync(id);
            if (compte == null) return NotFound();

            var ancienHash = AuthController.HashPassword(req.AncienMotDePasse);
            if (compte.MotDePasse != ancienHash)
                return BadRequest("Mot de passe actuel incorrect.");

            var nouveauHash = AuthController.HashPassword(req.NouveauMotDePasse);

            // Vérifier que le nouveau mot de passe n'a pas déjà été utilisé (5 derniers)
            var derniers = await _context.HistoriquesMdp
                .Where(h => h.IdCompte == id)
                .OrderByDescending(h => h.DateModification)
                .Take(5)
                .Select(h => h.MotDePasse)
                .ToListAsync();

            if (derniers.Contains(nouveauHash) || compte.MotDePasse == nouveauHash)
                return BadRequest("Ce mot de passe a déjà été utilisé récemment. Veuillez en choisir un nouveau.");

            // Sauvegarder l'ancien mot de passe dans l'historique
            _context.HistoriquesMdp.Add(new tontine.WebAPI.Models.HistoriqueMdpModel
            {
                IdCompte          = id,
                MotDePasse        = compte.MotDePasse ?? "",
                DateModification  = DateTime.Now
            });

            compte.MotDePasse = nouveauHash;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class ChangePasswordRequest
    {
        public string AncienMotDePasse { get; set; } = "";
        public string NouveauMotDePasse { get; set; } = "";
    }
}
