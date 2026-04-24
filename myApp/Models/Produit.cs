namespace myApp.Models
{
    public class Produit
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public decimal Prix { get; set; }
        public int CategorieId { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation property
        public Categorie? Categorie { get; set; }
    }
}