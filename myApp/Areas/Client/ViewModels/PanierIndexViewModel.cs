using myApp.Models;

namespace myApp.Areas.Client.ViewModels;

public class PanierIndexViewModel
{
    public Panier? Panier { get; set; }
    public decimal Total { get; set; }
}