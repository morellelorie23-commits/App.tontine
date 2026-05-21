using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tontine.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAmendeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajout conditionnel : montant_commission dans versement (peut déjà exister)
            migrationBuilder.Sql(@"
                SET @s = IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'versement' AND COLUMN_NAME = 'montant_commission') = 0,
                    'ALTER TABLE `versement` ADD COLUMN `montant_commission` decimal(18,4) NOT NULL DEFAULT 0',
                    'SELECT 1');
                PREPARE p FROM @s; EXECUTE p; DEALLOCATE PREPARE p;
            ");

            // Ajout conditionnel : mode_paiement dans cotisation (peut déjà exister)
            migrationBuilder.Sql(@"
                SET @s = IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'cotisation' AND COLUMN_NAME = 'mode_paiement') = 0,
                    'ALTER TABLE `cotisation` ADD COLUMN `mode_paiement` longtext NULL',
                    'SELECT 1');
                PREPARE p FROM @s; EXECUTE p; DEALLOCATE PREPARE p;
            ");

            // Tables comptables : création conditionnelle (peuvent déjà exister)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ecriture_comptable` (
                    `id_ecriture` int NOT NULL AUTO_INCREMENT,
                    `code_journal` longtext NOT NULL,
                    `id_cotisation` int NULL,
                    `id_versement` int NULL,
                    `id_tontine` int NULL,
                    `periode_comptable` longtext NOT NULL,
                    `date_ecriture` datetime(6) NOT NULL,
                    `numero_sequence` int NOT NULL,
                    `piece_justificative` longtext NOT NULL,
                    `libelle` longtext NOT NULL,
                    `statut` longtext NOT NULL,
                    `total_debit` decimal(18,4) NOT NULL,
                    `total_credit` decimal(18,4) NOT NULL,
                    PRIMARY KEY (`id_ecriture`)
                ) CHARACTER SET utf8mb4;
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `plan_comptable` (
                    `code_compte` varchar(50) NOT NULL,
                    `libelle` longtext NOT NULL,
                    `classe` int NOT NULL,
                    `sens_normal` longtext NOT NULL,
                    `description` longtext NULL,
                    `actif` tinyint(1) NOT NULL,
                    PRIMARY KEY (`code_compte`)
                ) ENGINE=InnoDB ROW_FORMAT=DYNAMIC CHARACTER SET utf8mb4;
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `ligne_ecriture` (
                    `id_ligne` int NOT NULL AUTO_INCREMENT,
                    `id_ecriture` int NOT NULL,
                    `numero_ligne` int NOT NULL,
                    `sens` longtext NOT NULL,
                    `compte_ohada` longtext NOT NULL,
                    `libelle_compte` longtext NOT NULL,
                    `libelle_ligne` longtext NOT NULL,
                    `montant` decimal(18,4) NOT NULL,
                    PRIMARY KEY (`id_ligne`),
                    CONSTRAINT `FK_ligne_ecriture_ecriture_comptable_id_ecriture`
                        FOREIGN KEY (`id_ecriture`) REFERENCES `ecriture_comptable` (`id_ecriture`) ON DELETE CASCADE
                ) CHARACTER SET utf8mb4;
            ");

            migrationBuilder.Sql(@"
                SET @s = IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
                     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ligne_ecriture' AND INDEX_NAME = 'IX_ligne_ecriture_id_ecriture') = 0,
                    'CREATE INDEX `IX_ligne_ecriture_id_ecriture` ON `ligne_ecriture` (`id_ecriture`)',
                    'SELECT 1');
                PREPARE p FROM @s; EXECUTE p; DEALLOCATE PREPARE p;
            ");

            migrationBuilder.CreateTable(
                name: "amende",
                columns: table => new
                {
                    id_amende = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cotisation = table.Column<int>(type: "int", nullable: false),
                    id_membre = table.Column<int>(type: "int", nullable: false),
                    id_cycle = table.Column<int>(type: "int", nullable: false),
                    id_tontine = table.Column<int>(type: "int", nullable: false),
                    id_penalite = table.Column<int>(type: "int", nullable: true),
                    taux_applique = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    montant_cotisation = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    montant_amende = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    date_calcul = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    statut = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_paiement = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_amende", x => x.id_amende);
                    table.ForeignKey(
                        name: "FK_amende_cotisation_id_cotisation",
                        column: x => x.id_cotisation,
                        principalTable: "cotisation",
                        principalColumn: "id_cotisation",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_amende_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_amende_membre_id_membre",
                        column: x => x.id_membre,
                        principalTable: "membre",
                        principalColumn: "id_membre",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_amende_penalite_id_penalite",
                        column: x => x.id_penalite,
                        principalTable: "penalite",
                        principalColumn: "id_penalite");
                    table.ForeignKey(
                        name: "FK_amende_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_amende_id_cotisation",
                table: "amende",
                column: "id_cotisation");

            migrationBuilder.CreateIndex(
                name: "IX_amende_id_cycle",
                table: "amende",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_amende_id_membre",
                table: "amende",
                column: "id_membre");

            migrationBuilder.CreateIndex(
                name: "IX_amende_id_penalite",
                table: "amende",
                column: "id_penalite");

            migrationBuilder.CreateIndex(
                name: "IX_amende_id_tontine",
                table: "amende",
                column: "id_tontine");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "amende");

            migrationBuilder.DropTable(
                name: "ligne_ecriture");

            migrationBuilder.DropTable(
                name: "plan_comptable");

            migrationBuilder.DropTable(
                name: "ecriture_comptable");

            migrationBuilder.DropColumn(
                name: "montant_commission",
                table: "versement");

            migrationBuilder.DropColumn(
                name: "mode_paiement",
                table: "cotisation");
        }
    }
}
