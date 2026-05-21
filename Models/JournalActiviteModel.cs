using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("journal_activite")]
    public class JournalActiviteModel
    {
        [Key]
        [Column("id_journal")]
        public int IdJournal { get; set; }

        [Column("action")]
        public string Action { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; }

        [Column("utilisateur")]
        public string? Utilisateur { get; set; }

        [Column("date_action")]
        public DateTime DateAction { get; set; } = DateTime.Now;
    }
}
