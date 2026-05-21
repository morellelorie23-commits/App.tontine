using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("ligne_ecriture")]
    public class LigneEcritureModel
    {
        [Key]
        [Column("id_ligne")]
        public int IdLigne { get; set; }

        [Column("id_ecriture")]
        public int IdEcriture { get; set; }

        [Column("numero_ligne")]
        public int NumeroLigne { get; set; }

        [Column("sens")]
        public string Sens { get; set; } = "D";   // "D" débit | "C" crédit

        [Column("compte_ohada")]
        public string CompteOhada { get; set; } = "";

        [Column("libelle_compte")]
        public string LibelleCompte { get; set; } = "";

        [Column("libelle_ligne")]
        public string LibelleLigne { get; set; } = "";

        [Column("montant")]
        public decimal Montant { get; set; }

        [ForeignKey("IdEcriture")]
        public EcritureComptableModel? Ecriture { get; set; }
    }
}
