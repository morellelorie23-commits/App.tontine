using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("cycle")]
    public class CycleModel
    {
        [Key]
        [Column("id_cycle")]
        public int IdCycle { get; set; }

        [Column("nom_cycle")]
        public string? NomCycle { get; set; }

        [Column("date_debut")]
        public DateOnly? DateDebut { get; set; }

        [Column("date_fin")]
        public DateOnly? DateFin { get; set; }

        [Column("statut")]
        public string? Statut { get; set; }
    }
}