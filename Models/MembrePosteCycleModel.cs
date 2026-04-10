using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tontine.WebAPI.Models
{
    [Table("membre_poste_cycle")]
    public class MembrePosteCycleModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_membre")]
        public int IdMembre { get; set; }

        [Column("id_poste")]
        public int IdPoste { get; set; }

        [Column("id_cycle")]
        public int IdCycle { get; set; }

        [Column("id_tontine")]
        public int IdTontine { get; set; }

        [Column("commentaire")]
        public string? Commentaire { get; set; }

        [ForeignKey("IdMembre")]
        public MembreModel? Membre { get; set; }

        [ForeignKey("IdPoste")]
        public PosteModel? Poste { get; set; }

        [ForeignKey("IdCycle")]
        public CycleModel? Cycle { get; set; }

        [ForeignKey("IdTontine")]
        public TontineModel? Tontine { get; set; }
    }
}