using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("penalite")]
    public class PenaliteModel
    {
        [Key]
        [Column("id_penalite")]
        public int IdPenalite { get; set; }

        [Column("libelle")]
        public string? Libelle { get; set; }

        [Column("description")]
        public string? Description { get; set; }
    }
}