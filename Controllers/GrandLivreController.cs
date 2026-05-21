using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tontine.WebAPI.Data;
using tontine.WebAPI.Helpers;

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
}
