using System;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    // Classe représentant les spécificités d'une carte en fonction de son attitude
    public class SpecificiteCarte
    {
        // Propriétés : Identifiant de l'attitude et nom de la carte
        public int IdAttitude { get; }
        public string NomCarte { get; }

        // Constructeur de la classe qui initialise l'identifiant d'attitude et le nom de la carte
        public SpecificiteCarte(int idAttitude, string nomCarte)
        {
            IdAttitude = idAttitude;
            NomCarte = nomCarte;
        }

        /// <summary>
        /// Renvoie une description de la capacité spéciale de la carte en fonction de son attitude.
        /// </summary>
        public string ExpliquerCapacite()
        {
            // Utilisation d'un switch pour définir les capacités spéciales selon l'identifiant d'attitude
            switch (IdAttitude)
            {
                case 2:
                    return $"La carte {NomCarte} peut attaquer un ennemi de son choix.";
                case 3:
                    return $"La carte {NomCarte} prend tous les dégâts de la prochaine attaque (si elle résiste).";
                case 4:
                    return $"La carte {NomCarte} réduit le coût de la prochaine carte piochée de moitié.";
                case 5:
                    return $"La carte {NomCarte} peut attaquer un ennemi de son choix.";
                case 6:
                    return $"La carte {NomCarte} peut attaquer un ennemi de son choix.";
                case 7:
                    return $"La carte {NomCarte} attaque toutes les cartes du terrain adverse.";
                case 8:
                    return $"La carte {NomCarte} soigne la carte de son choix.";
                case 9:
                    return $"La carte {NomCarte} permet de piocher 2 cartes à la fois.";
                case 10:
                    return $"La carte {NomCarte} permet de ressusciter la dernière carte morte (ennemie ou alliée).";
                case 11:
                    return $"La carte {NomCarte} peut attaquer un ennemi de son choix.";
                case 12:
                    return $"La carte {NomCarte} soigne toutes vos cartes sur le terrain de 2 points de vie.";
                default:
                    // Cas par défaut : si l'identifiant n'est pas reconnu, une description générique est retournée
                    return $"La carte {NomCarte} possède une capacité spéciale non définie (id_attitude = {IdAttitude}).";
            }
        }
    }
}
