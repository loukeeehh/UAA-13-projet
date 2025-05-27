using System;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    public class SpecificiteCarte
    {
        public int IdAttitude { get; }
        public string NomCarte { get; }

        public SpecificiteCarte(int idAttitude, string nomCarte)
        {
            IdAttitude = idAttitude;
            NomCarte = nomCarte;
        }

        /// <summary>
        /// Renvoie une description de la capacité spéciale de la carte.
        /// </summary>
        public string ExpliquerCapacite()
        {
            switch (IdAttitude)
            {
                case 2:
                    return $"La carte {NomCarte} peut attaquer un ennemie de son choix.";
                case 3:
                    return $"La carte {NomCarte} prends touts les degats de la prochaine attaque (si il résiste).";
                case 4:
                    return $"La carte {NomCarte} réduit le coût de la prochaine carte piochée de moitié.";
                case 5:
                    return $"La carte {NomCarte} peut attaquer un ennemie de son choix.";
                case 6:
                    return $"La carte {NomCarte} peut attaquer un ennemie de son choix.";
                case 7:
                    return $"La carte {NomCarte} attaque toutes les cartes du terrain adverse.";
                case 8:
                    return $"La carte {NomCarte} soigne la carte de son choix.";
                case 9:
                    return $"La carte {NomCarte} piocher 2 carte a la fois ";
                case 10:
                    return $"La carte {NomCarte} permet de ressuciter la dernière carte morte (ennemie ou ami)";
                case 11:
                    return $"La carte {NomCarte} peut attaquer un ennemie de son choix.";
                case 12:
                    return $"La carte {NomCarte} Soigne toutes vos cartes sur le terrain de 2 point de vie ";
                default:
                    return $"La carte {NomCarte} possède une capacité spéciale non définie (id_attitude = {IdAttitude}).";
            }
        }
    }
}