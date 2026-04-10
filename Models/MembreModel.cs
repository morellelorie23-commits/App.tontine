using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("membre")]
    public class MembreModel
    {
        [Key]
        [Column("id_membre")]
        public int IdMembre { get; set; }

        [Required]
        [Column("nom")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [Column("prenom")]
        public string Prenom { get; set; } = string.Empty;

        [Column("sexe")]
        public string? Sexe { get; set; }

        [Column("date_naissance")]
        public DateOnly? DateNaissance { get; set; }

        [Column("telephone")]
        public string? Telephone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("adresse")]
        public string? Adresse { get; set; }

        [Column("ville")]
        public string? Ville { get; set; }

        [Column("pays")]
        public string? Pays { get; set; }

        [Column("profession")]
        public string? Profession { get; set; }

        [Column("numero_cni")]
        public string? NumeroCni { get; set; }

        [Column("photo")]
        public string? Photo { get; set; }

        [Column("date_inscription")]
        public DateTime DateInscription { get; set; } = DateTime.Now;
    }
}