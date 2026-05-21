using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("garant")]
    public class GarantModel
    {
        [Key]
        [Column("id_garant")]
        public int IdGarant { get; set; }

        [Column("id_membre")]
        [Required]
        public int IdMembre { get; set; }

        [Column("nom")]
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = "";

        [Column("prenom")]
        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = "";

        [Column("telephone")]
        [Required]
        [StringLength(30)]
        public string Telephone { get; set; } = "";

        [Column("email")]
        [StringLength(200)]
        public string? Email { get; set; }

        [Column("relation")]
        [StringLength(50)]
        public string? Relation { get; set; }

        [Column("adresse")]
        [StringLength(500)]
        public string? Adresse { get; set; }

        [Column("date_ajout")]
        public DateTime DateAjout { get; set; } = DateTime.Now;
    }
}
