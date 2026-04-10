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

        [ForeignKey("IdMembre")]
        public MembreModel? Membre { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }
    }
}