using myApp.Models;

namespace myApp.Areas.Client.ViewModels;

public class ProduitIndexViewModel
{
    public List<Produit> Produits { get; set; } = [];
    public List<Categorie> Categories { get; set; } = [];
    public int? SelectedCategorieId { get; set; }
}
