using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MembreModel> Membres { get; set; }
        public DbSet<TontineModel> Tontines { get; set; }
        public DbSet<CycleModel> Cycles { get; set; }
        public DbSet<PosteModel> Postes { get; set; }
        public DbSet<PenaliteModel> Penalites { get; set; }
        public DbSet<CycleTontineModel> CycleTontines { get; set; }
        public DbSet<MembreCycleTontineModel> MembreCycleTontines { get; set; }
        public DbSet<MembrePosteCycleModel> MembrePosteCycles { get; set; }
        public DbSet<CycleTontinePenaliteModel> CycleTontinePenalites { get; set; }
        
    }
}