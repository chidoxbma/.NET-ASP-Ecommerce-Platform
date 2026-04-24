namespace myApp.Models
{
    public class LignePanier
    {
        public int Id { get; set; }
        public int PanierId { get; set; }
        public int ProduitId { get; set; }
        public int Quantite { get; set; }

        // Navigation properties
        public Panier Panier { get; set; }
        public Produit Produit { get; set; }
    }
}