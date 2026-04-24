using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using myApp.Data;
using myApp.Models;

namespace myApp.Areas.Admin.Pages.Categories;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Categorie> Categories { get; private set; } = new List<Categorie>();

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories
            .OrderBy(c => c.Nom)
            .ToListAsync();
    }
}
