using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("membre_cycle_tontine")]
    public class MembreCycleTontineModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_membre")]
        public int IdMembre { get; set; }

        [Column("id_cycle")]
        public int IdCycle { get; set; }

        [Column("id_tontine")]
        public int IdTontine { get; set; }

        [Column("matricule")]
        public string? Matricule { get; set; }

        [Column("numero_ordre")]
        public int? NumeroOrdre { get; set; }

        // Nombre de parts souscrites pour cette inscription (1 = plein, 0.5 = demi, 2 = double)
        [Column("nombre_parts")]
        public decimal NombreParts { get; set; } = 1m;

        [ForeignKey("IdMembre")]
        public MembreModel? Membre { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }
    }
}