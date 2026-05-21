using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tontine.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPretTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "compte_utilisateur",
                columns: table => new
                {
                    id_compte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prenom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    statut = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_creation = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compte_utilisateur", x => x.id_compte);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cycle",
                columns: table => new
                {
                    id_cycle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom_cycle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_debut = table.Column<DateOnly>(type: "date", nullable: true),
                    date_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    statut = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cycle", x => x.id_cycle);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "journal_activite",
                columns: table => new
                {
                    id_journal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    utilisateur = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_action = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_activite", x => x.id_journal);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "membre",
                columns: table => new
                {
                    id_membre = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prenom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sexe = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_naissance = table.Column<DateOnly>(type: "date", nullable: true),
                    telephone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    adresse = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ville = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pays = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    profession = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numero_cni = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    photo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_inscription = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membre", x => x.id_membre);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "penalite",
                columns: table => new
                {
                    id_penalite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    libelle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_penalite", x => x.id_penalite);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "poste",
                columns: table => new
                {
                    id_poste = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    libelle_poste = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poste", x => x.id_poste);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pret",
                columns: table => new
                {
                    id_pret = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_membre = table.Column<int>(type: "int", nullable: false),
                    montant = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    taux = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    date_pret = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_remboursement = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    statut = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    montant_rembourse = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    date_creation = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pret", x => x.id_pret);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tontine",
                columns: table => new
                {
                    id_tontine = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    libelle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    montant = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    frequence = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tontine", x => x.id_tontine);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cotisation",
                columns: table => new
                {
                    id_cotisation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_membre = table.Column<int>(type: "int", nullable: false),
                    id_tontine = table.Column<int>(type: "int", nullable: false),
                    id_cycle = table.Column<int>(type: "int", nullable: false),
                    montant = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    date_cotisation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    statut = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotisation", x => x.id_cotisation);
                    table.ForeignKey(
                        name: "FK_cotisation_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cotisation_membre_id_membre",
                        column: x => x.id_membre,
                        principalTable: "membre",
                        principalColumn: "id_membre",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cotisation_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cycle_tontine",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cycle = table.Column<int>(type: "int", nullable: false),
                    id_tontine = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cycle_tontine", x => x.id);
                    table.ForeignKey(
                        name: "FK_cycle_tontine_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cycle_tontine_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cycle_tontine_penalite",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cycle = table.Column<int>(type: "int", nullable: true),
                    id_tontine = table.Column<int>(type: "int", nullable: true),
                    id_penalite = table.Column<int>(type: "int", nullable: true),
                    taux_avant = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    taux_apres = table.Column<decimal>(type: "decimal(65,30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cycle_tontine_penalite", x => x.id);
                    table.ForeignKey(
                        name: "FK_cycle_tontine_penalite_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle");
                    table.ForeignKey(
                        name: "FK_cycle_tontine_penalite_penalite_id_penalite",
                        column: x => x.id_penalite,
                        principalTable: "penalite",
                        principalColumn: "id_penalite");
                    table.ForeignKey(
                        name: "FK_cycle_tontine_penalite_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "membre_cycle_tontine",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_membre = table.Column<int>(type: "int", nullable: false),
                    id_cycle = table.Column<int>(type: "int", nullable: false),
                    id_tontine = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membre_cycle_tontine", x => x.id);
                    table.ForeignKey(
                        name: "FK_membre_cycle_tontine_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membre_cycle_tontine_membre_id_membre",
                        column: x => x.id_membre,
                        principalTable: "membre",
                        principalColumn: "id_membre",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membre_cycle_tontine_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "membre_poste_cycle",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_membre = table.Column<int>(type: "int", nullable: false),
                    id_poste = table.Column<int>(type: "int", nullable: false),
                    id_cycle = table.Column<int>(type: "int", nullable: false),
                    id_tontine = table.Column<int>(type: "int", nullable: false),
                    commentaire = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membre_poste_cycle", x => x.id);
                    table.ForeignKey(
                        name: "FK_membre_poste_cycle_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membre_poste_cycle_membre_id_membre",
                        column: x => x.id_membre,
                        principalTable: "membre",
                        principalColumn: "id_membre",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membre_poste_cycle_poste_id_poste",
                        column: x => x.id_poste,
                        principalTable: "poste",
                        principalColumn: "id_poste",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membre_poste_cycle_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "versement",
                columns: table => new
                {
                    id_versement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_membre = table.Column<int>(type: "int", nullable: false),
                    id_tontine = table.Column<int>(type: "int", nullable: false),
                    id_cycle = table.Column<int>(type: "int", nullable: false),
                    montant = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    date_versement = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_versement", x => x.id_versement);
                    table.ForeignKey(
                        name: "FK_versement_cycle_id_cycle",
                        column: x => x.id_cycle,
                        principalTable: "cycle",
                        principalColumn: "id_cycle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_versement_membre_id_membre",
                        column: x => x.id_membre,
                        principalTable: "membre",
                        principalColumn: "id_membre",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_versement_tontine_id_tontine",
                        column: x => x.id_tontine,
                        principalTable: "tontine",
                        principalColumn: "id_tontine",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_cotisation_id_cycle",
                table: "cotisation",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_cotisation_id_membre",
                table: "cotisation",
                column: "id_membre");

            migrationBuilder.CreateIndex(
                name: "IX_cotisation_id_tontine",
                table: "cotisation",
                column: "id_tontine");

            migrationBuilder.CreateIndex(
                name: "IX_cycle_tontine_id_cycle",
                table: "cycle_tontine",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_cycle_tontine_id_tontine",
                table: "cycle_tontine",
                column: "id_tontine");

            migrationBuilder.CreateIndex(
                name: "IX_cycle_tontine_penalite_id_cycle",
                table: "cycle_tontine_penalite",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_cycle_tontine_penalite_id_penalite",
                table: "cycle_tontine_penalite",
                column: "id_penalite");

            migrationBuilder.CreateIndex(
                name: "IX_cycle_tontine_penalite_id_tontine",
                table: "cycle_tontine_penalite",
                column: "id_tontine");

            migrationBuilder.CreateIndex(
                name: "IX_membre_cycle_tontine_id_cycle",
                table: "membre_cycle_tontine",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_membre_cycle_tontine_id_membre",
                table: "membre_cycle_tontine",
                column: "id_membre");

            migrationBuilder.CreateIndex(
                name: "IX_membre_cycle_tontine_id_tontine",
                table: "membre_cycle_tontine",
                column: "id_tontine");

            migrationBuilder.CreateIndex(
                name: "IX_membre_poste_cycle_id_cycle",
                table: "membre_poste_cycle",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_membre_poste_cycle_id_membre",
                table: "membre_poste_cycle",
                column: "id_membre");

            migrationBuilder.CreateIndex(
                name: "IX_membre_poste_cycle_id_poste",
                table: "membre_poste_cycle",
                column: "id_poste");

            migrationBuilder.CreateIndex(
                name: "IX_membre_poste_cycle_id_tontine",
                table: "membre_poste_cycle",
                column: "id_tontine");

            migrationBuilder.CreateIndex(
                name: "IX_versement_id_cycle",
                table: "versement",
                column: "id_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_versement_id_membre",
                table: "versement",
                column: "id_membre");

            migrationBuilder.CreateIndex(
                name: "IX_versement_id_tontine",
                table: "versement",
                column: "id_tontine");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compte_utilisateur");

            migrationBuilder.DropTable(
                name: "cotisation");

            migrationBuilder.DropTable(
                name: "cycle_tontine");

            migrationBuilder.DropTable(
                name: "cycle_tontine_penalite");

            migrationBuilder.DropTable(
                name: "journal_activite");

            migrationBuilder.DropTable(
                name: "membre_cycle_tontine");

            migrationBuilder.DropTable(
                name: "membre_poste_cycle");

            migrationBuilder.DropTable(
                name: "pret");

            migrationBuilder.DropTable(
                name: "versement");

            migrationBuilder.DropTable(
                name: "penalite");

            migrationBuilder.DropTable(
                name: "poste");

            migrationBuilder.DropTable(
                name: "cycle");

            migrationBuilder.DropTable(
                name: "membre");

            migrationBuilder.DropTable(
                name: "tontine");
        }
    }
}
