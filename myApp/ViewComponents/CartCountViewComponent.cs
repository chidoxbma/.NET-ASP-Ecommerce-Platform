using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myApp.Data;

namespace myApp.ViewComponents;

public class CartCountViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CartCountViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return View(0);
        }

        var utilisateurId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(utilisateurId))
        {
            return View(0);
        }

        var count = await _context.LignePaniers
            .Where(lp => lp.Panier.UtilisateurId == utilisateurId)
            .SumAsync(lp => (int?)lp.Quantite) ?? 0;

        return View(count);
    }
}