using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("pret")]
    public class Pret
    {
        [Key]
        [Column("id_pret")]
        public int IdPret { get; set; }

        [Column("id_membre")]
        public int IdMembre { get; set; }

        [Column("montant")]
        [Required]
        public decimal Montant { get; set; }

        [Column("taux")]
        public decimal Taux { get; set; } = 0;

        [Column("date_pret")]
        [Required]
        public DateTime DatePret { get; set; }

        [Column("date_remboursement")]
        public DateTime? DateRemboursement { get; set; }

        [Column("statut")]
        [StringLength(50)]
        public string Statut { get; set; } = "En attente"; // En attente, Approuvé, Remboursé, En retard

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("montant_rembourse")]
        public decimal MontantRemboursé { get; set; } = 0;

        [Column("date_creation")]
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}
