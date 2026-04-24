using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myApp.Areas.Client.ViewModels;
using myApp.Data;
using myApp.Models;

namespace myApp.Areas.Client.Controllers;

[Area("Client")]
[Authorize]
public class CommandeController : Controller
{
    private readonly ApplicationDbContext _context;

    public CommandeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmer(CheckoutViewModel model)
    {
        // 209-210: Validate delivery info
        if (string.IsNullOrWhiteSpace(model.AdresseLivraison))
        {
            ModelState.AddModelError(nameof(model.AdresseLivraison), "Adresse livraison is required.");
        }

        if (string.IsNullOrWhiteSpace(model.VilleLivraison))
        {
            ModelState.AddModelError(nameof(model.VilleLivraison), "Ville livraison is required.");
        }

        if (!ModelState.IsValid)
        {
            return View("Checkout", model);
        }

        // 211: Current user ID
        var utilisateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(utilisateurId))
        {
            return Challenge();
        }

        // 212: Load cart items
        var panier = await _context.Paniers
            .Include(p => p.LignePaniers)
                .ThenInclude(lp => lp.Produit)
            .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateurId);

        // 213: Empty cart handling
        if (panier is null || panier.LignePaniers.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Panier", new { area = "Client" });
        }

        // 214: Transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 215-221: Create order
            var total = panier.LignePaniers.Sum(lp => lp.Quantite * lp.Produit.Prix);

            var commande = new Commande
            {
                UtilisateurId = utilisateurId,               // 216
                DateCommande = DateTime.UtcNow,              // 217
                AdresseLivraison = model.AdresseLivraison,   // 218
                VilleLivraison = model.VilleLivraison,       // 219
                Statut = StatutCommande.EnCours,             // 220
                Total = total                                // 221
            };

            // 222
            _context.Commandes.Add(commande);
            await _context.SaveChangesAsync();

            // 223-228
            var lignesCommande = panier.LignePaniers.Select(lp => new LigneCommande
            {
                CommandeId = commande.Id,        // 224
                ProduitId = lp.ProduitId,        // 225
                Quantite = lp.Quantite,          // 226
                PrixUnitaire = lp.Produit.Prix   // 227 (frozen price)
            }).ToList();

            _context.LigneCommandes.AddRange(lignesCommande);

            // 229: Clear cart
            _context.LignePaniers.RemoveRange(panier.LignePaniers);
            _context.Paniers.Remove(panier);

            // 230
            await _context.SaveChangesAsync();

            // 231
            await transaction.CommitAsync();

            // 232
            TempData["SuccessMessage"] = $"Order #{commande.Id} created successfully.";
            return RedirectToAction(nameof(ConfirmationCommande), new { commandeId = commande.Id });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmationCommande(int commandeId)
    {
        var utilisateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(utilisateurId))
        {
            return Challenge();
        }

        var commande = await _context.Commandes
            .Include(c => c.LigneCommandes)
                .ThenInclude(lc => lc.Produit)
            .FirstOrDefaultAsync(c => c.Id == commandeId);

        if (commande is null)
        {
            return NotFound();
        }

        if (commande.UtilisateurId != utilisateurId)
        {
            return Forbid();
        }

        return View("Confirmation", commande);
    }

    [HttpGet]
    public async Task<IActionResult> MesCommandes()
    {
        var utilisateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(utilisateurId))
        {
            return Challenge();
        }

        var commandes = await _context.Commandes
            .Where(c => c.UtilisateurId == utilisateurId)
            .Include(c => c.LigneCommandes)
                .ThenInclude(lc => lc.Produit)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync();

        return View(commandes);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var utilisateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(utilisateurId))
        {
            return Challenge();
        }

        var panier = await _context.Paniers
            .Include(p => p.LignePaniers)
                .ThenInclude(lp => lp.Produit)
            .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateurId);

        if (panier is null || panier.LignePaniers.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Panier", new { area = "Client" });
        }

        var vm = new CheckoutViewModel
        {
            Items = panier.LignePaniers.Select(lp => new CheckoutItemViewModel
            {
                NomProduit = lp.Produit.Nom,
                Quantite = lp.Quantite,
                PrixUnitaire = lp.Produit.Prix
            }).ToList(),
            Total = panier.LignePaniers.Sum(lp => lp.Quantite * lp.Produit.Prix)
        };

        return View(vm);
    }
}
