using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using myApp.Models;

namespace myApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<Utilisateur>(options)
    {
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Panier> Paniers { get; set; }
        public DbSet<LignePanier> LignePaniers { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<LigneCommande> LigneCommandes { get; set; }
        public DbSet<Produit> Produits { get; set; }
        public DbSet<Categorie> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Utilisateur>()
                .HasOne(u => u.Panier)
                .WithOne(p => p.Utilisateur)
                .HasForeignKey<Panier>(p => p.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Utilisateur>()
                .HasMany(u => u.Commandes)
                .WithOne(c => c.Utilisateur)
                .HasForeignKey(c => c.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Panier>()
                .HasMany(p => p.LignePaniers)
                .WithOne(lp => lp.Panier)
                .HasForeignKey(lp => lp.PanierId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LignePanier>()
                .HasOne(lp => lp.Produit)
                .WithMany()
                .HasForeignKey(lp => lp.ProduitId);

            builder.Entity<Commande>()
                .HasMany(c => c.LigneCommandes)
                .WithOne(lc => lc.Commande)
                .HasForeignKey(lc => lc.CommandeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LigneCommande>()
                .HasOne(lc => lc.Produit)
                .WithMany()
                .HasForeignKey(lc => lc.ProduitId);

            builder.Entity<Produit>()
                .HasOne(p => p.Categorie)
                .WithMany()
                .HasForeignKey(p => p.CategorieId);
        }
    }
}
