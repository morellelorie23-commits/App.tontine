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
        public DbSet<CotisationModel> Cotisations { get; set; }
        public DbSet<VersementModel> Versements { get; set; }
        public DbSet<CompteUtilisateurModel> Comptes { get; set; }
        public DbSet<JournalActiviteModel> Journals { get; set; }
        public DbSet<Pret> Prets { get; set; }
        public DbSet<GarantModel> Garants { get; set; }
        public DbSet<HistoriqueMdpModel> HistoriquesMdp { get; set; }
        public DbSet<PaiementMobileModel> PaiementsMobiles { get; set; }
        public DbSet<ReunionModel> Reunions { get; set; }
        public DbSet<PlanComptableModel> PlanComptable { get; set; }
        public DbSet<EcritureComptableModel> EcrituresComptables { get; set; }
        public DbSet<LigneEcritureModel> LignesEcriture { get; set; }
        public DbSet<AmendeModel> Amendes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EcritureComptableModel>()
                .HasMany(e => e.Lignes)
                .WithOne(l => l.Ecriture)
                .HasForeignKey(l => l.IdEcriture)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}