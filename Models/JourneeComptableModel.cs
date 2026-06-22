using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("journee_comptable")]
    public class JourneeComptableModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("date_journee")]
        public DateOnly DateJournee { get; set; }

        [Column("statut")]
        public string Statut { get; set; } = "Ouverte"; // "Ouverte" / "Fermée"

        [Column("date_ouverture")]
        public DateTime DateOuverture { get; set; } = DateTime.Now;

        [Column("date_fermeture")]
        public DateTime? DateFermeture { get; set; }

        [Column("ouvert_par")]
        public string OuvertPar { get; set; } = "Système";
    }
}
