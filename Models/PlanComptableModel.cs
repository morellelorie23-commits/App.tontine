using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("plan_comptable")]
    public class PlanComptableModel
    {
        [Key]
        [Column("code_compte")]
        public string CodeCompte { get; set; } = "";

        [Column("libelle")]
        public string Libelle { get; set; } = "";

        [Column("classe")]
        public int Classe { get; set; }

        [Column("sens_normal")]
        public string SensNormal { get; set; } = "D";

        [Column("description")]
        public string? Description { get; set; }

        [Column("actif")]
        public bool Actif { get; set; } = true;
    }
}
