using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tontine.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSaisieSeanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_reunion",
                table: "cotisation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_gagnant_enchere",
                table: "cotisation",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "mt_attendu",
                table: "cotisation",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "mt_enchere",
                table: "cotisation",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "penalite_seance",
                table: "cotisation",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id_reunion",
                table: "cotisation");

            migrationBuilder.DropColumn(
                name: "is_gagnant_enchere",
                table: "cotisation");

            migrationBuilder.DropColumn(
                name: "mt_attendu",
                table: "cotisation");

            migrationBuilder.DropColumn(
                name: "mt_enchere",
                table: "cotisation");

            migrationBuilder.DropColumn(
                name: "penalite_seance",
                table: "cotisation");
        }
    }
}
