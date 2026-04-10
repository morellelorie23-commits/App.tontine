using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("tontine")]
    public class TontineModel
    {
        [Key]
        [Column("id_tontine")]
        public int IdTontine { get; set; }

        [Column("libelle")]
        public string? Libelle { get; set; }

        [Column("montant")]
        public decimal? Montant { get; set; }

        [Column("frequence")]
        public string? Frequence { get; set; }
    }
}