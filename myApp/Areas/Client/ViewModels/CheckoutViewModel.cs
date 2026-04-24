namespace myApp.Areas.Client.ViewModels;

public class CheckoutItemViewModel
{
    public string NomProduit { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal SousTotal => Quantite * PrixUnitaire;
}

public class CheckoutViewModel
{
    public List<CheckoutItemViewModel> Items { get; set; } = new List<CheckoutItemViewModel>();
    public decimal Total { get; set; }

    public string AdresseLivraison { get; set; } = string.Empty;
    public string VilleLivraison { get; set; } = string.Empty;
}