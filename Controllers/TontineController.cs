using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TontineController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TontineController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TontineModel>>> GetAll()
            => await _context.Tontines.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<TontineModel>> GetById(int id)
        {
            var t = await _context.Tontines.FindAsync(id);
            return t == null ? NotFound() : t;
        }

        [HttpPost]
        public async Task<ActionResult<TontineModel>> Create(TontineModel tontine)
        {
            _context.Tontines.Add(tontine);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Nouvelle tontine",
                Description = $"Tontine \"{tontine.Libelle}\" créée (montant : {tontine.Montant} FCFA)",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tontine.IdTontine }, tontine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TontineModel tontine)
        {
            if (id != tontine.IdTontine) return BadRequest();
            _context.Entry(tontine).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Tontine modifiée",
                Description = $"Tontine \"{tontine.Libelle}\" (id={id}) mise à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _context.Tontines.FindAsync(id);
            if (t == null) return NotFound();
            _context.Tontines.Remove(t);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Tontine supprimée",
                Description = $"Tontine \"{t.Libelle}\" (id={id}) supprimée",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}