namespace myApp.Models
{
    public class LigneCommande
    {
        public int Id { get; set; }
        public int CommandeId { get; set; }
        public int ProduitId { get; set; }
        public int Quantite { get; set; }

        // Prix frozen at order time
        public decimal PrixUnitaire { get; set; }

        // Navigation properties
        public Commande Commande { get; set; }
        public Produit Produit { get; set; }
    }
}