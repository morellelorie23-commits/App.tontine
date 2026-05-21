using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("cotisation")]
    public class CotisationModel
    {
        [Key]
        [Column("id_cotisation")]
        public int IdCotisation { get; set; }

        [Column("id_membre")]
        public int IdMembre { get; set; }

        [Column("id_tontine")]
        public int IdTontine { get; set; }

        [Column("id_cycle")]
        public int IdCycle { get; set; }

        [Column("montant")]
        public decimal Montant { get; set; }

        [Column("date_cotisation")]
        public DateTime DateCotisation { get; set; } = DateTime.Now;

        [Column("statut")]
        public string Statut { get; set; } = "En attente";

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("id_mandataire")]
        public int? IdMandataire { get; set; }

        [Column("mode_paiement")]
        public string? ModePaiement { get; set; } = "Cash";

        [ForeignKey("IdMembre")]
        public MembreModel? Membre { get; set; }

        [ForeignKey("IdMandataire")]
        public MembreModel? Mandataire { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }
    }
}
