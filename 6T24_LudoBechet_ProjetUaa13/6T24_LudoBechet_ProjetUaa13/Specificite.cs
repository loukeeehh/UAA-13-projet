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
                case 4:
                    return $"La carte {NomCarte} réduit le coût de la prochaine carte piochée de moitié.";
                case 2:
                    return $"La carte {NomCarte} (Archer) vous permet de choisir manuellement la cible d'attaque parmi les unités ennemies.";
                default:
                    return $"La carte {NomCarte} possède une capacité spéciale non définie (id_attitude = {IdAttitude}).";
            }
        }
    }
}
