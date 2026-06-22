using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosteController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PosteController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PosteModel>>> GetAll()
            => await _context.Postes.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<PosteModel>> GetById(int id)
        {
            var p = await _context.Postes.FindAsync(id);
            return p == null ? NotFound() : p;
        }

        [HttpPost]
        public async Task<ActionResult<PosteModel>> Create(PosteModel poste)
        {
            _context.Postes.Add(poste);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouveau poste",
                Description = $"Poste \"{poste.LibellePoste}\" créé",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = poste.IdPoste }, poste);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PosteModel poste)
        {
            if (id != poste.IdPoste) return BadRequest();
            _context.Entry(poste).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Poste modifié",
                Description = $"Poste \"{poste.LibellePoste}\" (id={id}) mis à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Postes.FindAsync(id);
            if (p == null) return NotFound();
            _context.Postes.Remove(p);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Poste supprimé",
                Description = $"Poste \"{p.LibellePoste}\" (id={id}) supprimé",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}