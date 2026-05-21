using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tontine.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMotDePasseCompte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mot_de_passe",
                table: "compte_utilisateur",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mot_de_passe",
                table: "compte_utilisateur");
        }
    }
}
