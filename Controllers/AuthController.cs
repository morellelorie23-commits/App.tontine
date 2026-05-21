using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using tontine.WebAPI.Data;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context) => _context = context;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Nom) || string.IsNullOrWhiteSpace(req.MotDePasse))
                return BadRequest("Nom d'utilisateur et mot de passe requis.");

            var hash = HashPassword(req.MotDePasse);
            var compte = await _context.Comptes.FirstOrDefaultAsync(
                c => c.Nom == req.Nom && c.MotDePasse == hash && c.Statut == "Actif");

            if (compte == null)
                return Unauthorized("Nom d'utilisateur ou mot de passe incorrect.");

            return Ok(new
            {
                compte.IdCompte,
                compte.Nom,
                compte.Prenom,
                compte.Email,
                compte.Role,
                compte.Photo
            });
        }

        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }

    public class LoginRequest
    {
        public string Nom { get; set; } = "";
        public string MotDePasse { get; set; } = "";
    }
}
