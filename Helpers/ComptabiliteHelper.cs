using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Helpers
{
    /// <summary>
    /// Génère les écritures comptables OHADA en partie double.
    /// Règle fondamentale : Σ Débits = Σ Crédits — sinon exception.
    /// </summary>
    public static class ComptabiliteHelper
    {
        public const decimal TauxCommission = 0.02m;   // 2% du pot versé
        public const decimal TauxTva        = 0.1925m; // 19,25% Cameroun

        // ── Comptes fixes ────────────────────────────────────────────────
        public const string CptCaisseEspeces   = "571001";
        public const string CptWalletMtn       = "521100";
        public const string CptWalletOrange    = "521200";
        public const string CptBanqueSequestre = "521001";
        public const string CptBanqueApp       = "521002";
        public const string CptCommTransit     = "4672";   // préfixe — suffixe = idTontine
        public const string CptTva             = "4431";
        public const string CptProdCommission  = "706001";

        /// <summary>Retourne le compte séquestre du groupe : 4671001, 4671002…</summary>
        public static string CompteSequestre(int idTontine)
            => $"4671{idTontine:D3}";

        /// <summary>Retourne le compte commission transit du groupe : 4672001…</summary>
        public static string CompteCommissionTransit(int idTontine)
            => $"4672{idTontine:D3}";

        /// <summary>
        /// Sélectionne le compte trésorerie selon le mode de paiement.
        /// </summary>
        public static string CompteTresorerie(string modePaiement) => modePaiement switch
        {
            "Cash"         => CptCaisseEspeces,
            "MTN"          => CptWalletMtn,
            "Orange Money" => CptWalletOrange,
            _              => CptBanqueSequestre
        };

        public static string JournalCode(string modePaiement)
            => modePaiement == "Cash" ? "CA" : "BQ";

        // ────────────────────────────────────────────────────────────────
        // ENTRÉE PRINCIPALE — crée une EcritureComptable + ses lignes
        // Doit être appelé AVANT SaveChangesAsync() du controller.
        // ────────────────────────────────────────────────────────────────
        public static async Task<EcritureComptableModel> CreerEcriture(
            AppDbContext ctx,
            string codeJournal,
            string piece,
            string libelle,
            int? idTontine,
            int? idCotisation,
            int? idVersement,
            List<(string Sens, string Compte, string LibelleCompte, string LibelleLigne, decimal Montant)> lignes)
        {
            // Contrôle d'équilibre — OBLIGATOIRE
            var debit  = lignes.Where(l => l.Sens == "D").Sum(l => l.Montant);
            var credit = lignes.Where(l => l.Sens == "C").Sum(l => l.Montant);
            if (Math.Abs(debit - credit) > 0.01m)
                throw new InvalidOperationException(
                    $"Écriture déséquilibrée : D={debit:N0} ≠ C={credit:N0} FCFA");

            var periode = DateTime.Now.ToString("yyyy-MM");

            // Numéro séquentiel par journal + période
            var dernierSeq = await ctx.EcrituresComptables
                .Where(e => e.CodeJournal == codeJournal && e.PeriodeComptable == periode)
                .MaxAsync(e => (int?)e.NumeroSequence) ?? 0;

            var ecriture = new EcritureComptableModel
            {
                CodeJournal       = codeJournal,
                IdCotisation      = idCotisation,
                IdVersement       = idVersement,
                IdTontine         = idTontine,
                PeriodeComptable  = periode,
                DateEcriture      = DateTime.Now,
                NumeroSequence    = dernierSeq + 1,
                PieceJustificative = piece,
                Libelle           = libelle,
                TotalDebit        = debit,
                TotalCredit       = credit,
                Lignes = lignes.Select((l, i) => new LigneEcritureModel
                {
                    NumeroLigne   = i + 1,
                    Sens          = l.Sens,
                    CompteOhada   = l.Compte,
                    LibelleCompte = l.LibelleCompte,
                    LibelleLigne  = l.LibelleLigne,
                    Montant       = l.Montant
                }).ToList()
            };

            ctx.EcrituresComptables.Add(ecriture);
            return ecriture;
        }

        // ────────────────────────────────────────────────────────────────
        // FLUX A — Cotisation entrante (MoMo ou Cash)
        // D : compte trésorerie  /  C : compte séquestre tontine
        // ────────────────────────────────────────────────────────────────
        public static Task<EcritureComptableModel> EcritureCotisation(
            AppDbContext ctx, CotisationModel c, string modePaiement, string nomMembre, string nomTontine)
        {
            var cptTresorerie = CompteTresorerie(modePaiement);
            var cptSequestre  = CompteSequestre(c.IdTontine);
            var journal       = JournalCode(modePaiement);
            var libelleMode   = modePaiement == "Cash" ? "Espèces" : modePaiement;

            return CreerEcriture(ctx,
                codeJournal:  journal,
                piece:        $"COT-{c.IdCotisation:D6}",
                libelle:      $"Cotisation {libelleMode} — {nomMembre} — {nomTontine}",
                idTontine:    c.IdTontine,
                idCotisation: c.IdCotisation,
                idVersement:  null,
                lignes: new()
                {
                    (Sens: "D", Compte: cptTresorerie,
                     LibelleCompte: LibelleTresorerie(cptTresorerie),
                     LibelleLigne: $"Réception cotisation — {nomMembre}",
                     Montant: c.Montant),

                    (Sens: "C", Compte: cptSequestre,
                     LibelleCompte: $"Séquestre {nomTontine}",
                     LibelleLigne: $"Dépôt cotisation membre — {nomMembre}",
                     Montant: c.Montant)
                });
        }

        // ────────────────────────────────────────────────────────────────
        // FLUX B — Versement au bénéficiaire + commission transit
        // D : séquestre (pot brut)
        // C : trésorerie (net bénéficiaire) + commission transit
        // ────────────────────────────────────────────────────────────────
        public static Task<EcritureComptableModel> EcritureVersement(
            AppDbContext ctx, VersementModel v, string nomMembre, string nomTontine)
        {
            var potBrut      = v.Montant;                          // montant brut = ce qu'on verse
            var commission   = Math.Round(potBrut * TauxCommission, 0);
            var netBenef     = potBrut - commission;
            v.MontantCommission = commission;

            var cptSeq       = CompteSequestre(v.IdTontine);
            var cptTransit   = CompteCommissionTransit(v.IdTontine);

            return CreerEcriture(ctx,
                codeJournal:  "BQ",
                piece:        $"VRS-{v.IdVersement:D6}",
                libelle:      $"Versement pot — {nomMembre} — {nomTontine}",
                idTontine:    v.IdTontine,
                idCotisation: null,
                idVersement:  v.IdVersement,
                lignes: new()
                {
                    (Sens: "D", Compte: cptSeq,
                     LibelleCompte: $"Séquestre {nomTontine}",
                     LibelleLigne:  $"Déblocage pot bénéficiaire — {nomMembre}",
                     Montant: potBrut),

                    (Sens: "C", Compte: CptWalletMtn,
                     LibelleCompte: "Wallet MoMo séquestre",
                     LibelleLigne:  $"Paiement net bénéficiaire — {nomMembre}",
                     Montant: netBenef),

                    (Sens: "C", Compte: cptTransit,
                     LibelleCompte: "Commissions en transit",
                     LibelleLigne:  $"Commission 2% — pot {nomTontine}",
                     Montant: commission)
                });
        }

        // ────────────────────────────────────────────────────────────────
        // FLUX C — Encaissement commission (transit → compte app + TVA)
        // D : commission transit
        // C : banque propre app (HT) + TVA collectée
        // ────────────────────────────────────────────────────────────────
        public static Task<EcritureComptableModel> EcritureCommission(
            AppDbContext ctx, int idVersement, int idTontine, decimal montantCommission, string nomTontine)
        {
            var commHT  = Math.Round(montantCommission / (1 + TauxTva), 0);
            var tva     = montantCommission - commHT;
            var cptTrans = CompteCommissionTransit(idTontine);

            return CreerEcriture(ctx,
                codeJournal:  "VT",
                piece:        $"COM-{idVersement:D6}",
                libelle:      $"Encaissement commission — {nomTontine}",
                idTontine:    idTontine,
                idCotisation: null,
                idVersement:  idVersement,
                lignes: new()
                {
                    (Sens: "D", Compte: cptTrans,
                     LibelleCompte: "Commissions en transit",
                     LibelleLigne:  $"Virement commission — VRS-{idVersement:D6}",
                     Montant: montantCommission),

                    (Sens: "C", Compte: CptBanqueApp,
                     LibelleCompte: "Banque propre application",
                     LibelleLigne:  $"Commission HT encaissée",
                     Montant: commHT),

                    (Sens: "C", Compte: CptTva,
                     LibelleCompte: "TVA collectée (19,25%)",
                     LibelleLigne:  $"TVA sur commission — COM-{idVersement:D6}",
                     Montant: tva)
                });
        }

        private static string LibelleTresorerie(string compte) => compte switch
        {
            CptCaisseEspeces   => "Caisse principale (espèces)",
            CptWalletMtn       => "Wallet MTN MoMo séquestre",
            CptWalletOrange    => "Wallet Orange Money séquestre",
            _                  => "Banque séquestre"
        };
    }
}
