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
public class PanierController : Controller
{
    private readonly ApplicationDbContext _context;

    public PanierController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
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

        if (panier is null)
        {
            var emptyVm = new PanierIndexViewModel
            {
                Panier = null,
                Total = 0m
            };

            return View(emptyVm);
        }

        var total = panier.LignePaniers.Sum(lp => lp.Quantite * lp.Produit.Prix);

        var vm = new PanierIndexViewModel
        {
            Panier = panier,
            Total = total
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int produitId, int quantite = 1)
    {
        var utilisateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(utilisateurId))
        {
            return Challenge();
        }

        if (quantite < 1)
        {
            quantite = 1;
        }

        var produitExists = await _context.Produits.AnyAsync(p => p.Id == produitId);
        if (!produitExists)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return NotFound();
        }

        var panier = await _context.Paniers
            .Include(p => p.LignePaniers)
            .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateurId);

        if (panier is null)
        {
            panier = new Panier
            {
                UtilisateurId = utilisateurId,
                DateCreation = DateTime.UtcNow,
                LignePaniers = []
            };

            _context.Paniers.Add(panier);
        }

        var ligneExistante = panier.LignePaniers
            .FirstOrDefault(lp => lp.ProduitId == produitId);

        if (ligneExistante is not null)
        {
            ligneExistante.Quantite += quantite;
            TempData["SuccessMessage"] = "Cart updated.";
        }
        else
        {
            panier.LignePaniers.Add(new LignePanier
            {
                ProduitId = produitId,
                Quantite = quantite
            });
            TempData["SuccessMessage"] = "Product added to cart.";
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(int lignePanierId)
    {
        var utilisateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(utilisateurId))
        {
            return Challenge();
        }

        var ligne = await _context.LignePaniers
            .Include(lp => lp.Panier)
            .FirstOrDefaultAsync(lp => lp.Id == lignePanierId && lp.Panier.UtilisateurId == utilisateurId);

        if (ligne is null)
        {
            return NotFound();
        }

        _context.LignePaniers.Remove(ligne);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item removed from cart.";

        return RedirectToAction(nameof(Index));
    }
}
