using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myApp.Data;
using myApp.Models;

namespace myApp.Areas.Admin.Pages.Products;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public Produit Produit { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public List<SelectListItem> Categories { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync(string? mode)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == Produit.CategorieId);
        if (!categoryExists)
        {
            ModelState.AddModelError("Produit.CategorieId", "Invalid category.");
            await LoadCategoriesAsync();
            return Page();
        }

        if (ImageFile is not null)
        {
            Produit.ImageUrl = await SaveImageAsync(ImageFile);
        }

        _context.Produits.Add(Produit);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Product created successfully.";
        if (string.Equals(mode, "list", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Produit", new { area = "Client" });
        }

        return RedirectToPage("Create");
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
}
