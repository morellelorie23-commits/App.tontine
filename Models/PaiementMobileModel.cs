using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("paiement_mobile")]
    public class PaiementMobileModel
    {
        [Key]
        [Column("id_paiement")]
        public int IdPaiement { get; set; }

        [Column("id_cotisation")]
        public int? IdCotisation { get; set; }

        [Column("telephone")]
        [StringLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [Column("operateur")]
        [StringLength(20)]
        public string Operateur { get; set; } = "MTN"; // MTN | Orange

        [Column("reference")]
        [StringLength(100)]
        public string Reference { get; set; } = string.Empty;

        [Column("statut")]
        [StringLength(20)]
        public string Statut { get; set; } = "En attente"; // En attente | Confirmé | Échoué

        [Column("montant")]
        public decimal Montant { get; set; }

        [Column("date_creation")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Column("date_confirmation")]
        public DateTime? DateConfirmation { get; set; }

        [Column("message_erreur")]
        [StringLength(500)]
        public string? MessageErreur { get; set; }

        [ForeignKey("IdCotisation")]
        public CotisationModel? Cotisation { get; set; }
    }
}
