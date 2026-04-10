using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("cycle_tontine_penalite")]
    public class CycleTontinePenaliteModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_cycle")]
        public int? IdCycle { get; set; }

        [Column("id_tontine")]
        public int? IdTontine { get; set; }

        [Column("id_penalite")]
        public int? IdPenalite { get; set; }

        [Column("taux_avant")]
        public decimal? TauxAvant { get; set; }

        [Column("taux_apres")]
        public decimal? TauxApres { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }

        [ForeignKey("IdPenalite")]
        public PenaliteModel? Penalite { get; set; }
    }
}