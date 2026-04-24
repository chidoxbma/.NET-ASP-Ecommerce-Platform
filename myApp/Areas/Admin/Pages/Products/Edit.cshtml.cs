using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myApp.Data;
using myApp.Models;

namespace myApp.Areas.Admin.Pages.Products;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public Produit Produit { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public List<SelectListItem> Categories { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var produit = await _context.Produits.FindAsync(id);
        if (produit is null)
        {
            return NotFound();
        }

        Produit = produit;
        await LoadCategoriesAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var existingProduit = await _context.Produits.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Produit.Id);
        if (existingProduit is null)
        {
            return NotFound();
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == Produit.CategorieId);
        if (!categoryExists)
        {
            ModelState.AddModelError("Produit.CategorieId", "Invalid category.");
            await LoadCategoriesAsync();
            return Page();
        }

        Produit.ImageUrl = existingProduit.ImageUrl;

        if (ImageFile is not null)
        {
            var newImageUrl = await SaveImageAsync(ImageFile);
            DeleteImageIfExists(existingProduit.ImageUrl);
            Produit.ImageUrl = newImageUrl;
        }

        _context.Attach(Produit).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _context.Produits.AnyAsync(p => p.Id == Produit.Id);
            if (!exists)
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToPage("Index");
    }

    private async Task LoadCategoriesAsync()
    {
        Categories = await _context.Categories
            .OrderBy(c => c.Nom)
            .Select(c => new SelectListItem(c.Nom, c.Id.ToString()))
            .ToListAsync();
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var uploadsRoot = Path.Combine(_environment.WebRootPath, "images", "products");
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/products/{fileName}";
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
