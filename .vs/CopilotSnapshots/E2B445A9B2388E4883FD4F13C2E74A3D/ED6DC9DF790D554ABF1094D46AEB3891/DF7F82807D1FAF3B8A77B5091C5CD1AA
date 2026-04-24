using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace myApp.Models
{
    public class Utilisateur : IdentityUser
    {
        public string NomComplet { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }

        // Navigation properties
        public Panier Panier { get; set; }
        public List<Commande> Commandes { get; set; }
    }
}

