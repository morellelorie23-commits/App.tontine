using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("compte_utilisateur")]
    public class CompteUtilisateurModel
    {
        [Key]
        [Column("id_compte")]
        public int IdCompte { get; set; }

        [Column("nom")]
        public string Nom { get; set; } = "";

        [Column("prenom")]
        public string Prenom { get; set; } = "";

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("role")]
        public string Role { get; set; } = "Lecteur";

        [Column("statut")]
        public string Statut { get; set; } = "Actif";

        [Column("date_creation")]
        public DateOnly DateCreation { get; set; }

        [Column("mot_de_passe")]
        public string? MotDePasse { get; set; }

        [Column("photo")]
        public string? Photo { get; set; }
    }
}
