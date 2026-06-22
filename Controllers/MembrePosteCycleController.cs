using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembrePosteCycleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MembrePosteCycleController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAll()
        {
            var list = await _context.MembrePosteCycles
                .Include(m => m.Membre)
                .Include(m => m.Poste)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .Select(m => new {
                    m.Id,
                    m.IdMembre,
                    NomMembre = m.Membre != null ? m.Membre.Nom : "",
                    m.IdPoste,
                    LibellePoste = m.Poste != null ? m.Poste.LibellePoste : "",
                    m.IdCycle,
                    NomCycle = m.Cycle != null ? m.Cycle.NomCycle : "",
                    m.IdTontine,
                    LibelleTontine = m.Tontine != null ? m.Tontine.Libelle : "",
                    m.Commentaire
                })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembrePosteCycleModel>> GetById(int id)
        {
            var m = await _context.MembrePosteCycles
                .Include(m => m.Membre)
                .Include(m => m.Poste)
                .Include(m => m.Cycle)
                .Include(m => m.Tontine)
                .FirstOrDefaultAsync(m => m.Id == id);
            return m == null ? NotFound() : m;
        }

        [HttpPost]
        public async Task<ActionResult<MembrePosteCycleModel>> Create(MembrePosteCycleModel mpc)
        {
            _context.MembrePosteCycles.Add(mpc);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Poste attribué",
                Description = $"Membre (id={mpc.IdMembre}) affecté au poste (id={mpc.IdPoste}) pour le cycle (id={mpc.IdCycle}) / tontine (id={mpc.IdTontine})",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = mpc.Id }, mpc);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MembrePosteCycleModel mpc)
        {
            if (id != mpc.Id) return BadRequest();
            _context.Entry(mpc).State = EntityState.Modified;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Attribution poste modifiée",
                Description = $"Attribution membre-poste-cycle (id={id}) mise à jour",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.MembrePosteCycles.FindAsync(id);
            if (m == null) return NotFound();
            _context.MembrePosteCycles.Remove(m);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action      = "Attribution poste supprimée",
                Description = $"Membre (id={m.IdMembre}) retiré du poste (id={m.IdPoste}) pour le cycle (id={m.IdCycle})",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}