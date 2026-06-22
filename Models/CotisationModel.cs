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

        [Column("mt_attendu")]
        public decimal MtAttendu { get; set; } = 0;

        [Column("mt_enchere")]
        public decimal MtEnchere { get; set; } = 0;

        [Column("is_gagnant_enchere")]
        public bool IsGagnantEnchere { get; set; } = false;

        [Column("id_reunion")]
        public int? IdReunion { get; set; }

        [Column("penalite_seance")]
        public decimal PenaliteSeance { get; set; } = 0;

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
