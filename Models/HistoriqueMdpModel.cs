using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("historique_mdp")]
    public class HistoriqueMdpModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_compte")]
        public int IdCompte { get; set; }

        [Column("mot_de_passe")]
        public string MotDePasse { get; set; } = "";

        [Column("date_modification")]
        public DateTime DateModification { get; set; }
    }
}
