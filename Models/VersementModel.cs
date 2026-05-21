using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("versement")]
    public class VersementModel
    {
        [Key]
        [Column("id_versement")]
        public int IdVersement { get; set; }

        [Column("id_membre")]
        public int IdMembre { get; set; }

        [Column("id_tontine")]
        public int IdTontine { get; set; }

        [Column("id_cycle")]
        public int IdCycle { get; set; }

        [Column("montant")]
        public decimal Montant { get; set; }

        [Column("date_versement")]
        public DateTime DateVersement { get; set; } = DateTime.Now;

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("montant_commission")]
        public decimal MontantCommission { get; set; } = 0;

        // Part déduite du pot (cotisation du bénéficiaire lui-même)
        [Column("montant_deduction")]
        public decimal MontantDeduction { get; set; } = 0;

        // Montant effectivement encaissé = Montant - MontantDeduction - MontantCommission
        [Column("montant_net")]
        public decimal MontantNet { get; set; } = 0;

        [ForeignKey("IdMembre")]
        public MembreModel? Membre { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }
    }
}
