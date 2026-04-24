using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myApp.Data;
using myApp.Models;

namespace myApp.Areas.Admin.Pages.Products;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DeleteModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public Produit Produit { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var produit = await _context.Produits
            .Include(p => p.Categorie)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produit is null)
        {
            return NotFound();
        }

        Produit = produit;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var produit = await _context.Produits.FindAsync(Produit.Id);
        if (produit is null)
        {
            return NotFound();
        }

        _context.Produits.Remove(produit);
        await _context.SaveChangesAsync();

        DeleteImageIfExists(produit.ImageUrl);

        return RedirectToPage("Index");
    }

    private void DeleteImageIfExists(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
