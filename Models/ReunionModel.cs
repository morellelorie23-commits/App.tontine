using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("reunion")]
    public class ReunionModel
    {
        [Key]
        [Column("id_reunion")]
        public int IdReunion { get; set; }

        [Column("id_cycle")]
        public int IdCycle { get; set; }

        [Column("id_tontine")]
        public int IdTontine { get; set; }

        [Column("date_reunion")]
        public DateTime DateReunion { get; set; }

        [Column("objet")]
        public string? Objet { get; set; }

        [Column("lieu")]
        public string? Lieu { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }
    }
}
