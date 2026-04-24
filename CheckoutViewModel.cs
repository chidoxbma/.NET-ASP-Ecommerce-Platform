using System.ComponentModel.DataAnnotations;
using myApp.Data;

namespace myApp.Areas.Client.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        public string NomComplet { get; set; } = string.Empty;

        [Required]
        public string AdresseLivraison { get; set; } = string.Empty;

        [Required]
        public string VilleLivraison { get; set; } = string.Empty;

        // Make sure there isn't an extra } floating around in the middle of these properties

        public Panier? Panier { get; set; }
    }
} // Make sure the namespace closes here