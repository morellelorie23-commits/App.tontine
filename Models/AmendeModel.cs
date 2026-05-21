using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("amende")]
    public class AmendeModel
    {
        [Key]
        [Column("id_amende")]
        public int IdAmende { get; set; }

        [Column("id_cotisation")]
        public int IdCotisation { get; set; }

        [Column("id_membre")]
        public int IdMembre { get; set; }

        [Column("id_cycle")]
        public int IdCycle { get; set; }

        [Column("id_tontine")]
        public int IdTontine { get; set; }

        [Column("id_penalite")]
        public int? IdPenalite { get; set; }

        [Column("taux_applique")]
        public decimal TauxApplique { get; set; }

        [Column("montant_cotisation")]
        public decimal MontantCotisation { get; set; }

        [Column("montant_amende")]
        public decimal MontantAmende { get; set; }

        [Column("date_calcul")]
        public DateTime DateCalcul { get; set; } = DateTime.Now;

        [Column("statut")]
        public string Statut { get; set; } = "En attente";

        [Column("date_paiement")]
        public DateTime? DatePaiement { get; set; }

        [ForeignKey("IdCotisation")]
        public CotisationModel? Cotisation { get; set; }

        [ForeignKey("IdMembre")]
        public MembreModel? Membre { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }

        [ForeignKey("IdPenalite")]
        public PenaliteModel? Penalite { get; set; }
    }
}
