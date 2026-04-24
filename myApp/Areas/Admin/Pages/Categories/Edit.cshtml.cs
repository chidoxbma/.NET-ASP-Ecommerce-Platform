using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myApp.Data;
using myApp.Models;

namespace myApp.Areas.Admin.Pages.Categories;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Attach(Categorie).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _context.Categories.AnyAsync(c => c.Id == Categorie.Id);
            if (!exists)
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToPage("Index");
    }
}
