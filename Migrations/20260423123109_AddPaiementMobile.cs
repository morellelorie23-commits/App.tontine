using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tontine.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPaiementMobile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paiement_mobile",
                columns: table => new
                {
                    id_paiement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cotisation = table.Column<int>(type: "int", nullable: true),
                    telephone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    operateur = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    statut = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    montant = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    date_creation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_confirmation = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    message_erreur = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paiement_mobile", x => x.id_paiement);
                    table.ForeignKey(
                        name: "FK_paiement_mobile_cotisation_id_cotisation",
                        column: x => x.id_cotisation,
                        principalTable: "cotisation",
                        principalColumn: "id_cotisation");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_paiement_mobile_id_cotisation",
                table: "paiement_mobile",
                column: "id_cotisation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "paiement_mobile");
        }
    }
}
