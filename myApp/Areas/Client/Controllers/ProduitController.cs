using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myApp.Data;
using myApp.Areas.Client.ViewModels;

namespace myApp.Areas.Client.Controllers;

[Area("Client")]
public class ProduitController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProduitController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? categorieId)
    {
        var produitsQuery = _context.Produits
            .Include(p => p.Categorie)
            .AsQueryable();

        if (categorieId.HasValue)
        {
            produitsQuery = produitsQuery.Where(p => p.CategorieId == categorieId.Value);
        }

        var viewModel = new ProduitIndexViewModel
        {
            SelectedCategorieId = categorieId,
            Produits = await produitsQuery
                .OrderByDescending(p => p.Id)
                .ToListAsync(),
            Categories = await _context.Categories.ToListAsync()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var produit = await _context.Produits
            .Include(p => p.Categorie)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produit is null)
        {
            return NotFound();
        }

        return View(produit);
    }
}
