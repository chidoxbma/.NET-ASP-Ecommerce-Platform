using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using myApp.Data;
using myApp.Models;

namespace myApp.Areas.Admin.Pages.Categories;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Categorie Categorie { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var categorie = await _context.Categories.FindAsync(id);
        if (categorie is null)
        {
            return NotFound();
        }

        Categorie = categorie;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var categorie = await _context.Categories.FindAsync(Categorie.Id);
        if (categorie is null)
        {
            return NotFound();
        }

        _context.Categories.Remove(categorie);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
