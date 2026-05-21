using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("ecriture_comptable")]
    public class EcritureComptableModel
    {
        [Key]
        [Column("id_ecriture")]
        public int IdEcriture { get; set; }

        [Column("code_journal")]
        public string CodeJournal { get; set; } = "";   // BQ | CA | VT | OD

        [Column("id_cotisation")]
        public int? IdCotisation { get; set; }

        [Column("id_versement")]
        public int? IdVersement { get; set; }

        [Column("id_tontine")]
        public int? IdTontine { get; set; }

        [Column("periode_comptable")]
        public string PeriodeComptable { get; set; } = "";  // "2025-01"

        [Column("date_ecriture")]
        public DateTime DateEcriture { get; set; } = DateTime.Now;

        [Column("numero_sequence")]
        public int NumeroSequence { get; set; }

        [Column("piece_justificative")]
        public string PieceJustificative { get; set; } = "";

        [Column("libelle")]
        public string Libelle { get; set; } = "";

        [Column("statut")]
        public string Statut { get; set; } = "comptabilisee";

        [Column("total_debit")]
        public decimal TotalDebit { get; set; }

        [Column("total_credit")]
        public decimal TotalCredit { get; set; }

        public List<LigneEcritureModel> Lignes { get; set; } = new();
    }
}
