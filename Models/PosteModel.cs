using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("poste")]
    public class PosteModel
    {
        [Key]
        [Column("id_poste")]
        public int IdPoste { get; set; }

        [Required]
        [Column("libelle_poste")]
        public string LibellePoste { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }
    }
}