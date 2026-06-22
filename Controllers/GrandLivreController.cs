using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Helpers;
using tontine.WebAPI.Models;

namespace tontine.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrandLivreController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GrandLivreController(AppDbContext context) => _context = context;

        // ── Grand Livre d'un groupe (toutes écritures du compte séquestre) ──
        [HttpGet("groupe/{idTontine}")]
        public async Task<IActionResult> GrandLivreGroupe(int idTontine, [FromQuery] string? periode = null)
        {
            var tontine = await _context.Tontines.FindAsync(idTontine);
            if (tontine == null) return NotFound();

            var compteSeq = ComptabiliteHelper.CompteSequestre(idTontine);

            var query = _context.LignesEcriture
                .Include(l => l.Ecriture)
                .Where(l => l.CompteOhada == compteSeq);

            if (!string.IsNullOrEmpty(periode))
                query = query.Where(l => l.Ecriture!.PeriodeComptable == periode);

            var lignes = await query
                .OrderBy(l => l.Ecriture!.DateEcriture)
                .ThenBy(l => l.Ecriture!.NumeroSequence)
                .ThenBy(l => l.NumeroLigne)
                .Select(l => new
                {
                    l.Ecriture!.DateEcriture,
                    l.Ecriture.CodeJournal,
                    l.Ecriture.PieceJustificative,
                    LibelleEcriture  = l.Ecriture.Libelle,
                    l.LibelleLigne,
                    l.Sens,
                    l.Montant,
                    l.CompteOhada,
                    l.LibelleCompte,
                    l.Ecriture.PeriodeComptable
                })
                .ToListAsync();

            // Solde progressif
            decimal solde = 0;
            var lignesAvecSolde = lignes.Select(l =>
            {
                // Compte 4671XXX : sens normal = Crédit → le solde monte sur C
                solde += l.Sens == "C" ? l.Montant : -l.Montant;
                return new
                {
                    l.DateEcriture,
                    l.CodeJournal,
                    l.PieceJustificative,
                    l.LibelleEcriture,
                    l.LibelleLigne,
                    l.Sens,
                    l.Montant,
                    SoldeProgressif = solde,
                    l.PeriodeComptable
                };
            }).ToList();

            var totalDepots   = lignes.Where(l => l.Sens == "C").Sum(l => l.Montant);
            var totalRetraits = lignes.Where(l => l.Sens == "D").Sum(l => l.Montant);

            return Ok(new
            {
                IdTontine     = idTontine,
                NomTontine    = tontine.Libelle,
                CompteSequestre = compteSeq,
                TotalDepots   = totalDepots,
                TotalRetraits = totalRetraits,
                SoldeActuel   = totalDepots - totalRetraits,
                Lignes        = lignesAvecSolde
            });
        }

        // ── Journal par code (BQ, CA, VT, OD) ───────────────────────────
        [HttpGet("journal/{codeJournal}")]
        public async Task<IActionResult> Journal(string codeJournal, [FromQuery] string? periode = null)
        {
            var query = _context.EcrituresComptables
                .Include(e => e.Lignes)
                .Where(e => e.CodeJournal == codeJournal.ToUpper());

            if (!string.IsNullOrEmpty(periode))
                query = query.Where(e => e.PeriodeComptable == periode);

            var ecritures = await query
                .OrderBy(e => e.DateEcriture)
                .ThenBy(e => e.NumeroSequence)
                .Select(e => new
                {
                    e.IdEcriture,
                    e.CodeJournal,
                    e.DateEcriture,
                    e.PeriodeComptable,
                    e.NumeroSequence,
                    e.PieceJustificative,
                    e.Libelle,
                    e.TotalDebit,
                    e.TotalCredit,
                    e.IdCotisation,
                    e.IdVersement,
                    e.IdTontine,
                    Lignes = e.Lignes.OrderBy(l => l.NumeroLigne).Select(l => new
                    {
                        l.NumeroLigne,
                        l.Sens,
                        l.CompteOhada,
                        l.LibelleCompte,
                        l.LibelleLigne,
                        l.Montant
                    })
                })
                .ToListAsync();

            return Ok(new
            {
                CodeJournal   = codeJournal.ToUpper(),
                Periode       = periode,
                NombreEcritures = ecritures.Count,
                TotalDebits   = ecritures.Sum(e => e.TotalDebit),
                TotalCredits  = ecritures.Sum(e => e.TotalCredit),
                Ecritures     = ecritures
            });
        }

        // ── Balance des comptes séquestre (tous groupes) ─────────────────
        [HttpGet("balance")]
        public async Task<IActionResult> Balance([FromQuery] string? periode = null)
        {
            var query = _context.LignesEcriture.Include(l => l.Ecriture).AsQueryable();

            if (!string.IsNullOrEmpty(periode))
                query = query.Where(l => l.Ecriture!.PeriodeComptable == periode);

            var lignes = await query.ToListAsync();

            var balance = lignes
                .GroupBy(l => new { l.CompteOhada, l.LibelleCompte })
                .Select(g =>
                {
                    var debit  = g.Where(l => l.Sens == "D").Sum(l => l.Montant);
                    var credit = g.Where(l => l.Sens == "C").Sum(l => l.Montant);
                    return new
                    {
                        Compte        = g.Key.CompteOhada,
                        Libelle       = g.Key.LibelleCompte,
                        TotalDebit    = debit,
                        TotalCredit   = credit,
                        SoldeDebiteur = debit > credit ? debit - credit : 0,
                        SoldeCredit   = credit > debit ? credit - debit : 0
                    };
                })
                .OrderBy(b => b.Compte)
                .ToList();

            return Ok(new
            {
                Periode       = periode ?? "all",
                TotalDebits   = balance.Sum(b => b.TotalDebit),
                TotalCredits  = balance.Sum(b => b.TotalCredit),
                Comptes       = balance
            });
        }

        // ── Relevé individuel d'un membre dans un groupe ─────────────────
        [HttpGet("releve-membre/{idMembre}/{idTontine}")]
        public async Task<IActionResult> ReleveIndividuel(int idMembre, int idTontine)
        {
            var membre  = await _context.Membres.FindAsync(idMembre);
            var tontine = await _context.Tontines.FindAsync(idTontine);
            if (membre == null || tontine == null) return NotFound();

            var cotisations = await _context.Cotisations
                .Where(c => c.IdMembre == idMembre && c.IdTontine == idTontine && c.Statut == "Payé")
                .OrderBy(c => c.DateCotisation)
                .Select(c => new
                {
                    Date        = c.DateCotisation,
                    Type        = "Cotisation",
                    c.Montant,
                    c.ModePaiement,
                    Reference   = $"COT-{c.IdCotisation:D6}"
                })
                .ToListAsync();

            var versements = await _context.Versements
                .Where(v => v.IdMembre == idMembre && v.IdTontine == idTontine)
                .OrderBy(v => v.DateVersement)
                .Select(v => new
                {
                    Date        = v.DateVersement,
                    Type        = "Versement reçu",
                    Montant     = v.Montant - v.MontantCommission,
                    ModePaiement = "MoMo",
                    Reference   = $"VRS-{v.IdVersement:D6}"
                })
                .ToListAsync();

            var operations = cotisations
                .Select(c => new { c.Date, c.Type, c.Montant, c.ModePaiement, c.Reference })
                .Concat(versements.Select(v => new { v.Date, v.Type, v.Montant, v.ModePaiement, v.Reference }))
                .OrderBy(o => o.Date)
                .ToList();

            decimal solde = 0;
            var releve = operations.Select(o =>
            {
                solde += o.Type == "Cotisation" ? o.Montant : -o.Montant;
                return new { o.Date, o.Type, o.Montant, o.ModePaiement, o.Reference, Solde = solde };
            }).ToList();

            return Ok(new
            {
                Membre       = $"{membre.Nom} {membre.Prenom}",
                Tontine      = tontine.Libelle,
                TotalCotise  = cotisations.Sum(c => c.Montant),
                TotalRecu    = versements.Sum(v => v.Montant),
                Operations   = releve
            });
        }

        // ── Journée comptable ─────────────────────────────────────────────
        [HttpGet("journee/courante")]
        public async Task<IActionResult> JourneeCourante()
        {
            var j = await _context.JourneesComptables
                .Where(j => j.Statut == "Ouverte")
                .OrderByDescending(j => j.DateOuverture)
                .FirstOrDefaultAsync();
            return Ok(j);
        }

        [HttpPost("journee/ouvrir")]
        public async Task<IActionResult> OuvrirJournee([FromBody] OuvrirJourneeRequest req)
        {
            var dejaOuverte = await _context.JourneesComptables
                .AnyAsync(j => j.Statut == "Ouverte");
            if (dejaOuverte)
                return BadRequest("Une journée est déjà ouverte.");

            var journee = new JourneeComptableModel
            {
                DateJournee   = req.DateJournee,
                Statut        = "Ouverte",
                DateOuverture = DateTime.Now,
                OuvertPar     = req.OuvertPar ?? "Système"
            };
            _context.JourneesComptables.Add(journee);
            _context.Journals.Add(new JournalActiviteModel
            {
                Action = "Journée ouverte",
                Description = $"Journée comptable du {req.DateJournee:dd/MM/yyyy} ouverte",
                Utilisateur = req.OuvertPar ?? "Système",
                DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(journee);
        }

        [HttpPost("journee/fermer/{id}")]
        public async Task<IActionResult> FermerJournee(int id)
        {
            var journee = await _context.JourneesComptables.FindAsync(id);
            if (journee == null) return NotFound();
            if (journee.Statut == "Fermée") return BadRequest("Journée déjà fermée.");

            journee.Statut = "Fermée";
            journee.DateFermeture = DateTime.Now;
            _context.Journals.Add(new JournalActiviteModel
            {
                Action = "Journée fermée",
                Description = $"Journée comptable du {journee.DateJournee:dd/MM/yyyy} fermée",
                Utilisateur = "Système",
                DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(journee);
        }

        // ── Saisie manuelle d'écriture ────────────────────────────────────
        [HttpPost("ecriture-manuelle")]
        public async Task<IActionResult> EcritureManuelle([FromBody] EcritureManuelleRequest req)
        {
            if (req.Lignes == null || req.Lignes.Count < 2)
                return BadRequest("Au moins 2 lignes requises.");

            var totalD = req.Lignes.Where(l => l.Sens == "D").Sum(l => l.Montant);
            var totalC = req.Lignes.Where(l => l.Sens == "C").Sum(l => l.Montant);
            if (Math.Abs(totalD - totalC) > 0.01m)
                return BadRequest($"Déséquilibre : Débits={totalD}, Crédits={totalC}");

            var periode = req.DateEcriture.ToString("yyyy-MM");
            var seq = await _context.EcrituresComptables
                .Where(e => e.CodeJournal == req.CodeJournal && e.PeriodeComptable == periode)
                .CountAsync() + 1;

            var ecriture = new EcritureComptableModel
            {
                CodeJournal       = req.CodeJournal,
                DateEcriture      = req.DateEcriture,
                PeriodeComptable  = periode,
                NumeroSequence    = seq,
                PieceJustificative = req.PieceJustificative,
                Libelle           = req.Libelle,
                Statut            = "Validé",
                TotalDebit        = totalD,
                TotalCredit       = totalC
            };
            _context.EcrituresComptables.Add(ecriture);
            await _context.SaveChangesAsync();

            int num = 1;
            foreach (var l in req.Lignes)
            {
                _context.LignesEcriture.Add(new LigneEcritureModel
                {
                    IdEcriture    = ecriture.IdEcriture,
                    NumeroLigne   = num++,
                    Sens          = l.Sens,
                    CompteOhada   = l.CompteOhada,
                    LibelleCompte = l.LibelleCompte,
                    LibelleLigne  = l.LibelleLigne,
                    Montant       = l.Montant
                });
            }
            _context.Journals.Add(new JournalActiviteModel
            {
                Action = "Écriture manuelle",
                Description = $"Écriture {req.CodeJournal} - {req.Libelle}",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(new { ecriture.IdEcriture, ecriture.PieceJustificative });
        }

        // ── Transfert de caisse ───────────────────────────────────────────
        [HttpPost("transfert-caisse")]
        public async Task<IActionResult> TransfertCaisse([FromBody] TransfertCaisseRequest req)
        {
            if (req.Montant <= 0) return BadRequest("Montant invalide.");
            if (req.CompteDepart == req.CompteDestination)
                return BadRequest("Caisse départ et destination identiques.");

            var periode = req.DateOperation.ToString("yyyy-MM");
            var seq = await _context.EcrituresComptables
                .Where(e => e.CodeJournal == "OD" && e.PeriodeComptable == periode)
                .CountAsync() + 1;

            var piece = $"TRF-{DateTime.Now:yyyyMMddHHmmss}";
            var ecriture = new EcritureComptableModel
            {
                CodeJournal = "OD", DateEcriture = req.DateOperation,
                PeriodeComptable = periode, NumeroSequence = seq,
                PieceJustificative = req.NumPiece ?? piece,
                Libelle = req.Motif ?? $"Transfert {req.CompteDepart}→{req.CompteDestination}",
                Statut = "Validé", TotalDebit = req.Montant, TotalCredit = req.Montant
            };
            _context.EcrituresComptables.Add(ecriture);
            await _context.SaveChangesAsync();

            _context.LignesEcriture.Add(new LigneEcritureModel
            {
                IdEcriture = ecriture.IdEcriture, NumeroLigne = 1, Sens = "D",
                CompteOhada = req.CompteDestination, LibelleCompte = req.LibelleDestination ?? req.CompteDestination,
                LibelleLigne = "Entrée caisse destinatrice", Montant = req.Montant
            });
            _context.LignesEcriture.Add(new LigneEcritureModel
            {
                IdEcriture = ecriture.IdEcriture, NumeroLigne = 2, Sens = "C",
                CompteOhada = req.CompteDepart, LibelleCompte = req.LibelleDepart ?? req.CompteDepart,
                LibelleLigne = "Sortie caisse départ", Montant = req.Montant
            });
            _context.Journals.Add(new JournalActiviteModel
            {
                Action = "Transfert caisse",
                Description = $"Transfert {req.Montant:N0} de {req.CompteDepart} vers {req.CompteDestination}",
                Utilisateur = "Système", DateAction = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(new { ecriture.IdEcriture, Piece = ecriture.PieceJustificative });
        }

        // ── Relevé de caisse ──────────────────────────────────────────────
        [HttpGet("releve-caisse")]
        public async Task<IActionResult> ReleveCaisse(
            [FromQuery] string? compte = null,
            [FromQuery] string? dateDebut = null,
            [FromQuery] string? dateFin = null)
        {
            var query = _context.LignesEcriture.Include(l => l.Ecriture).AsQueryable();

            if (!string.IsNullOrEmpty(compte))
                query = query.Where(l => l.CompteOhada == compte);

            if (DateTime.TryParse(dateDebut, out var dDebut))
                query = query.Where(l => l.Ecriture!.DateEcriture >= dDebut);

            if (DateTime.TryParse(dateFin, out var dFin))
                query = query.Where(l => l.Ecriture!.DateEcriture <= dFin.AddDays(1).AddSeconds(-1));

            var lignes = await query
                .OrderBy(l => l.Ecriture!.DateEcriture)
                .ThenBy(l => l.Ecriture!.NumeroSequence)
                .Select(l => new
                {
                    l.Ecriture!.DateEcriture,
                    l.CompteOhada,
                    Intitule     = l.LibelleCompte,
                    Piece        = l.Ecriture.PieceJustificative,
                    Motif        = l.LibelleLigne,
                    l.Sens,
                    l.Montant
                })
                .ToListAsync();

            decimal solde = 0;
            var releve = lignes.Select(l =>
            {
                solde += l.Sens == "D" ? l.Montant : -l.Montant;
                return new
                {
                    l.DateEcriture, l.CompteOhada, l.Intitule, l.Piece, l.Motif,
                    Debit  = l.Sens == "D" ? l.Montant : 0,
                    Credit = l.Sens == "C" ? l.Montant : 0,
                    Solde  = solde
                };
            }).ToList();

            return Ok(new
            {
                Compte    = compte,
                DateDebut = dateDebut,
                DateFin   = dateFin,
                Lignes    = releve,
                SoldeFinal = solde,
                TotalDebit  = lignes.Where(l => l.Sens == "D").Sum(l => l.Montant),
                TotalCredit = lignes.Where(l => l.Sens == "C").Sum(l => l.Montant)
            });
        }

        // ── Balance générale avec soldes initiaux et finaux ───────────────
        [HttpGet("balance-generale")]
        public async Task<IActionResult> BalanceGenerale(
            [FromQuery] string? dateDebut = null,
            [FromQuery] string? dateFin = null)
        {
            DateTime.TryParse(dateDebut, out var dDebut);
            var dFin = DateTime.TryParse(dateFin, out var df) ? df.AddDays(1).AddSeconds(-1) : DateTime.MaxValue;

            var toutesLignes = await _context.LignesEcriture.Include(l => l.Ecriture).ToListAsync();

            var comptes = toutesLignes
                .GroupBy(l => new { l.CompteOhada, l.LibelleCompte })
                .Select(g =>
                {
                    var avantPeriode = g.Where(l => l.Ecriture!.DateEcriture < dDebut).ToList();
                    var pendant      = g.Where(l => l.Ecriture!.DateEcriture >= dDebut && l.Ecriture.DateEcriture <= dFin).ToList();

                    var diD = avantPeriode.Where(l => l.Sens == "D").Sum(l => l.Montant);
                    var diC = avantPeriode.Where(l => l.Sens == "C").Sum(l => l.Montant);
                    var mvD = pendant.Where(l => l.Sens == "D").Sum(l => l.Montant);
                    var mvC = pendant.Where(l => l.Sens == "C").Sum(l => l.Montant);

                    var sfD = diD + mvD;
                    var sfC = diC + mvC;

                    return new
                    {
                        CompteOhada = g.Key.CompteOhada,
                        Intitule    = g.Key.LibelleCompte,
                        SoldeInitialDebit   = diD > diC ? diD - diC : (decimal)0,
                        SoldeInitialCredit  = diC > diD ? diC - diD : (decimal)0,
                        MouvementDebit  = mvD,
                        MouvementCredit = mvC,
                        SoldeFinalDebit   = sfD > sfC ? sfD - sfC : (decimal)0,
                        SoldeFinalCredit  = sfC > sfD ? sfC - sfD : (decimal)0,
                    };
                })
                .OrderBy(c => c.CompteOhada)
                .ToList();

            return Ok(new
            {
                DateDebut = dateDebut,
                DateFin   = dateFin,
                Comptes   = comptes,
                Totaux = new
                {
                    TotalSoldeInitialDebit   = comptes.Sum(c => c.SoldeInitialDebit),
                    TotalSoldeInitialCredit  = comptes.Sum(c => c.SoldeInitialCredit),
                    TotalMouvementDebit  = comptes.Sum(c => c.MouvementDebit),
                    TotalMouvementCredit = comptes.Sum(c => c.MouvementCredit),
                    TotalSoldeFinalDebit   = comptes.Sum(c => c.SoldeFinalDebit),
                    TotalSoldeFinalCredit  = comptes.Sum(c => c.SoldeFinalCredit),
                }
            });
        }

        // ── Balance client ────────────────────────────────────────────────
        [HttpGet("balance-client")]
        public async Task<IActionResult> BalanceClient(
            [FromQuery] string? dateDebut = null,
            [FromQuery] string? dateFin = null,
            [FromQuery] string? prefixeCompte = null)
        {
            DateTime.TryParse(dateDebut, out var dDebut);
            var dFin = DateTime.TryParse(dateFin, out var df) ? df.AddDays(1).AddSeconds(-1) : DateTime.MaxValue;

            var query = _context.LignesEcriture.Include(l => l.Ecriture).AsQueryable();
            if (!string.IsNullOrEmpty(prefixeCompte))
                query = query.Where(l => l.CompteOhada.StartsWith(prefixeCompte));

            var toutesLignes = await query.ToListAsync();

            var comptes = toutesLignes
                .GroupBy(l => new { l.CompteOhada, l.LibelleCompte })
                .Select(g =>
                {
                    var avant   = g.Where(l => l.Ecriture!.DateEcriture < dDebut).ToList();
                    var pendant = g.Where(l => l.Ecriture!.DateEcriture >= dDebut && l.Ecriture.DateEcriture <= dFin).ToList();

                    var diD = avant.Where(l => l.Sens == "D").Sum(l => l.Montant);
                    var diC = avant.Where(l => l.Sens == "C").Sum(l => l.Montant);
                    var mvD = pendant.Where(l => l.Sens == "D").Sum(l => l.Montant);
                    var mvC = pendant.Where(l => l.Sens == "C").Sum(l => l.Montant);
                    var sfD = diD + mvD; var sfC = diC + mvC;

                    return new
                    {
                        CompteOhada = g.Key.CompteOhada,
                        Intitule    = g.Key.LibelleCompte,
                        SoldeInitialDebit   = diD > diC ? diD - diC : (decimal)0,
                        SoldeInitialCredit  = diC > diD ? diC - diD : (decimal)0,
                        MouvementDebit = mvD, MouvementCredit = mvC,
                        SoldeFinalDebit   = sfD > sfC ? sfD - sfC : (decimal)0,
                        SoldeFinalCredit  = sfC > sfD ? sfC - sfD : (decimal)0,
                    };
                })
                .OrderBy(c => c.CompteOhada)
                .ToList();

            return Ok(new
            {
                DateDebut = dateDebut, DateFin = dateFin,
                PrefixeCompte = prefixeCompte,
                Comptes = comptes,
                Totaux = new
                {
                    TotalSoldeInitialDebit   = comptes.Sum(c => c.SoldeInitialDebit),
                    TotalSoldeInitialCredit  = comptes.Sum(c => c.SoldeInitialCredit),
                    TotalMouvementDebit  = comptes.Sum(c => c.MouvementDebit),
                    TotalMouvementCredit = comptes.Sum(c => c.MouvementCredit),
                    TotalSoldeFinalDebit   = comptes.Sum(c => c.SoldeFinalDebit),
                    TotalSoldeFinalCredit  = comptes.Sum(c => c.SoldeFinalCredit),
                }
            });
        }

        // ── État de rapprochement (solde virtuel app vs solde réel) ──────
        [HttpGet("rapprochement/{idTontine}")]
        public async Task<IActionResult> Rapprochement(int idTontine, [FromQuery] decimal? soldeReelBanque = null)
        {
            var tontine     = await _context.Tontines.FindAsync(idTontine);
            if (tontine == null) return NotFound();
            var compteSeq   = ComptabiliteHelper.CompteSequestre(idTontine);

            // Solde calculé depuis les écritures
            var lignesSeq = await _context.LignesEcriture
                .Where(l => l.CompteOhada == compteSeq)
                .ToListAsync();

            var soldeCredits = lignesSeq.Where(l => l.Sens == "C").Sum(l => l.Montant);
            var soldeDebits  = lignesSeq.Where(l => l.Sens == "D").Sum(l => l.Montant);
            var soldeLivre   = soldeCredits - soldeDebits;

            // Vérification depuis les transactions
            var totalCotisations = await _context.Cotisations
                .Where(c => c.IdTontine == idTontine && c.Statut == "Payé")
                .SumAsync(c => (decimal?)c.Montant) ?? 0;

            var totalVersements = await _context.Versements
                .Where(v => v.IdTontine == idTontine)
                .SumAsync(v => (decimal?)v.Montant) ?? 0;

            var soldeTransactions = totalCotisations - totalVersements;
            var ecartInterne      = soldeLivre - soldeTransactions;
            var ecartBanque       = soldeReelBanque.HasValue ? soldeReelBanque.Value - soldeLivre : (decimal?)null;

            return Ok(new
            {
                Tontine          = tontine.Libelle,
                CompteSequestre  = compteSeq,
                SoldeLivreComptable   = soldeLivre,
                SoldeTransactions     = soldeTransactions,
                EcartInterne          = ecartInterne,
                SoldeReelBanque       = soldeReelBanque,
                EcartBanque           = ecartBanque,
                Equilibre             = Math.Abs(ecartInterne) < 1,
                TotalCotisations      = totalCotisations,
                TotalVersements       = totalVersements
            });
        }
    }

    public class OuvrirJourneeRequest
    {
        public DateOnly DateJournee { get; set; }
        public string? OuvertPar   { get; set; }
    }

    public class LigneEcritureRequest
    {
        public string Sens          { get; set; } = "D";
        public string CompteOhada   { get; set; } = "";
        public string LibelleCompte { get; set; } = "";
        public string LibelleLigne  { get; set; } = "";
        public decimal Montant      { get; set; }
    }

    public class EcritureManuelleRequest
    {
        public string CodeJournal       { get; set; } = "OD";
        public DateTime DateEcriture    { get; set; } = DateTime.Now;
        public string PieceJustificative { get; set; } = "";
        public string Libelle           { get; set; } = "";
        public List<LigneEcritureRequest> Lignes { get; set; } = new();
    }

    public class TransfertCaisseRequest
    {
        public string CompteDepart       { get; set; } = "";
        public string? LibelleDepart     { get; set; }
        public string CompteDestination  { get; set; } = "";
        public string? LibelleDestination { get; set; }
        public decimal Montant           { get; set; }
        public DateTime DateOperation    { get; set; } = DateTime.Now;
        public string? NumPiece          { get; set; }
        public string? Motif             { get; set; }
    }
}
