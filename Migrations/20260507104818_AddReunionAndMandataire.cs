using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tontine.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddReunionAndMandataire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "matricule",
                table: "membre_cycle_tontine",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "numero_ordre",
                table: "membre_cycle_tontine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_mandataire",
                table: "cotisation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reunion",
                columns: table => new
                {
                    id_reunion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cycle = table.Column<int>(type: "int", nullable: false),
                    id_tontine = table.Column<int>(type: "int", nullable: false),
                    date_reunion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    objet = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lieu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reunion", x => x.id_reunion);
                    table.ForeignKey(
                        name: "FK_reunion_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reunion_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_cotisation_id_mandataire",
                table: "cotisation",
                column: "id_mandataire");

            migrationBuilder.CreateIndex(
                name: "IX_reunion_id_cycle",
                table: "reunion",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_reunion_id_tontine",
                table: "reunion",
                column: "id_tontine");

            migrationBuilder.AddForeignKey(
                name: "FK_cotisation_membre_id_mandataire",
                table: "cotisation",
                column: "id_mandataire",
                principalTable: "membre",
                principalColumn: "id_membre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cotisation_membre_id_mandataire",
                table: "cotisation");

            migrationBuilder.DropTable(
                name: "reunion");

            migrationBuilder.DropIndex(
                name: "IX_cotisation_id_mandataire",
                table: "cotisation");

            migrationBuilder.DropColumn(
                name: "matricule",
                table: "membre_cycle_tontine");

            migrationBuilder.DropColumn(
                name: "numero_ordre",
                table: "membre_cycle_tontine");

            migrationBuilder.DropColumn(
                name: "id_mandataire",
                table: "cotisation");
        }
    }
}
