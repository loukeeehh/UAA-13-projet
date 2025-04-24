using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    public partial class Plateau : Page
    {
        // Pioche et générateur aléatoire
        private DataSet pioche = new DataSet();
        private Random random = new Random();

        // Ressources des joueurs
        private int orJoueur1 = 20;
        private int orJoueur2 = 20;

        // Nombre de vies (manches) qui commencent à 3 pour chaque joueur
        private int vieJoueur1 = 3;
        private int vieJoueur2 = 3;

        // Cartes mortes (défaite à 6 par manche)
        private List<string> cartesMortesJoueur1 = new List<string>();
        private List<string> cartesMortesJoueur2 = new List<string>();

        // Carte sélectionnée pour placement
        private Border carteSelectionnee = null;

        // Variable pour la prévisualisation de la prochaine carte à piocher
        private DataRow nextCardRow = null;

        // Flags indiquant que le joueur bénéficie d'un discount sur le prochain coût de pioche
        private bool discountJoueur1 = false;
        private bool discountJoueur2 = false;

        public Plateau()
        {
            InitializeComponent();
            ChargerPioche();
            MettreAJourAffichageOr();
            MettreAJourAffichageVies();
            // Générer la première prévisualisation
            GenererNextCardPreview();
        }

        /// <summary>
        /// Génère la prévisualisation de la prochaine carte et affiche son prix dans le TextBlock
        /// NextCardPriceTextBlock.
        /// </summary>
        private void GenererNextCardPreview()
        {
            if (!pioche.Tables.Contains("carte") || pioche.Tables["carte"].Rows.Count == 0)
            {
                nextCardRow = null;
                CartePiochée.Source = null;
                NextCardPriceTextBlock.Text = "";
                return;
            }
            int index = random.Next(pioche.Tables["carte"].Rows.Count);
            nextCardRow = pioche.Tables["carte"].Rows[index];
            string previewImagePath = nextCardRow["CheminImage"].ToString();
           

            // Affiche initialement le prix normal (la réduction sera appliquée lors de la pioche)
            int prixProchaineCarte = Convert.ToInt32(nextCardRow["Prix_carte"]);
            NextCardPriceTextBlock.Text = $"Prix de la prochaine carte : {prixProchaineCarte}";
        }

        private void ChargerPioche()
        {
            try
            {
                bdd maBdd = new bdd();
                pioche = maBdd.ObtenirCartes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la pioche : {ex.Message}");
            }
        }

        private void MettreAJourAffichageOr()
        {
            OrTextBlockJoueur1.Text = $"Joueur 1 : {orJoueur1}";
            OrTextBlockJoueur2.Text = $"Joueur 2 : {orJoueur2}";
        }

        private void MettreAJourAffichageVies()
        {
            VieTextBlockJoueur1.Text = $"Vies : {vieJoueur1}";
            VieTextBlockJoueur2.Text = $"Vies : {vieJoueur2}";
        }

        // Méthode de pioche commune pour les deux joueurs utilisant la carte prévisualisée
        private void PiocherCarte(bool isJoueur1)
        {
            Panel bankContainer = isJoueur1 ? CarteContainerJoueur1 : CarteContainerJoueur2;

            if (nextCardRow == null)
            {
                MessageBox.Show("La pioche est vide, mon Seigneur !");
                return;
            }

            if (bankContainer.Children.Count >= 3)
            {
                MessageBox.Show(isJoueur1
                    ? "Le banc de Joueur 1 est plein, mon Seigneur !"
                    : "Le banc de Joueur 2 est plein, mon Seigneur !");
                return;
            }

            // Utilise la carte prévisualisée comme carte piochée
            DataRow row = nextCardRow;
            string imagePath = row["CheminImage"].ToString();
            string nomCarte = row["Nom_carte"].ToString();
            int idCarte = Convert.ToInt32(row["id_type"]);
            int prixCarte = Convert.ToInt32(row["Prix_carte"]);
            int attaqueCarte = Convert.ToInt32(row["Attaque_carte"]);
            int pvCarte = Convert.ToInt32(row["PV_carte"]);

            // Lecture des compétences spéciales de la carte
            int idAttitude = Convert.ToInt32(row["id_attitude"]);
            string attitudeType = row["attitude_type"].ToString();

            // Application d'un discount si le joueur bénéficie d'une réduction
            int effectivePrixCarte = prixCarte;
            if (isJoueur1 && discountJoueur1)
            {
                effectivePrixCarte = prixCarte / 2;
                discountJoueur1 = false; // Le bonus s'applique pour une seule pioche
            }
            else if (!isJoueur1 && discountJoueur2)
            {
                effectivePrixCarte = prixCarte / 2;
                discountJoueur2 = false;
            }

            // Vérification des ressources du joueur
            if (isJoueur1)
            {
                if (orJoueur1 < effectivePrixCarte)
                {
                    MessageBox.Show("Joueur 1 n'a pas assez d'or, mon Seigneur !");
                    return;
                }
                orJoueur1 -= effectivePrixCarte;
            }
            else
            {
                if (orJoueur2 < effectivePrixCarte)
                {
                    MessageBox.Show("Joueur 2 n'a pas assez d'or, mon Seigneur !");
                    return;
                }
                orJoueur2 -= effectivePrixCarte;
            }

            MettreAJourAffichageOr();

            // Création des statistiques de la carte en incluant la compétence spéciale
            CardStats statsCard = new CardStats
            {
                Id = idCarte,
                Nom = nomCarte,
                Attaque = attaqueCarte,
                PointsDeVie = pvCarte,
                Prix = effectivePrixCarte,
                CheminImage = imagePath,
                Owner = isJoueur1 ? 1 : 2,
                IdAttitude = idAttitude,
                AttitudeType = attitudeType
            };

            Border cardBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(2),
                Margin = new Thickness(2),
                Width = 145,
                Background = Brushes.White,
                Tag = statsCard
            };

            StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };

            Image image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 190,
                Height = 182,
                Stretch = Stretch.Uniform
            };

            TextBlock title = new TextBlock
            {
                Text = nomCarte,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock statsText = new TextBlock
            {
                Text = $"PV: {pvCarte} | ATQ: {attaqueCarte} | PRIX: {effectivePrixCarte}",
                FontSize = 12,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            sp.Children.Add(title);
            sp.Children.Add(image);
            sp.Children.Add(statsText);
            cardBorder.Child = sp;

            // Gestion de la sélection de la carte
            cardBorder.MouseDown += CartePiochee_Click;
            bankContainer.Children.Add(cardBorder);

            MessageTextBlock.Text = isJoueur1
                ? "Unité piochée pour Joueur 1, mon Seigneur !"
                : "Unité piochée pour Joueur 2, mon Seigneur !";

            // Si la carte piochée a la compétence spéciale id_attitude == 4,
            // le joueur bénéficiera d'une réduction sur le coût de la prochaine pioche.
            if (idAttitude == 4)
            {
                if (isJoueur1)
                    discountJoueur1 = true;
                else
                    discountJoueur2 = true;
            }

            // Retirer la carte piochée de la pioche
            pioche.Tables["carte"].Rows.Remove(row);

            // Régénérer la prévisualisation pour la prochaine carte
            GenererNextCardPreview();
        }

        private void PiocherCarteJoueur1_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(true);
        }

        private void PiocherCarteJoueur2_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(false);
        }

        private void CartePiochee_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card)
            {
                // Réinitialiser l'ancienne sélection
                carteSelectionnee?.SetValue(Border.BorderBrushProperty, Brushes.Black);
                carteSelectionnee = card;
                carteSelectionnee.BorderBrush = Brushes.Red;
                MessageTextBlock.Text = "Unité sélectionnée, mon Seigneur !";
                e.Handled = true;
            }
        }

        private void Zone_Click(object sender, MouseButtonEventArgs e)
        {
            if (carteSelectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une unité, mon Seigneur !");
                return;
            }

            if (!(sender is Border zoneBorder))
            {
                MessageBox.Show("Zone non reconnue !");
                return;
            }

            string zoneType = zoneBorder.Tag as string;
            if (string.IsNullOrEmpty(zoneType))
            {
                MessageBox.Show("La zone ne possède pas de type défini, vérifiez votre XAML !");
                return;
            }

            if (!(carteSelectionnee.Tag is CardStats stats))
            {
                MessageBox.Show("Impossible d'identifier l'unité sélectionnée !");
                return;
            }

            int zoneOwner = 0;
            if (zoneType.Contains("Joueur1"))
                zoneOwner = 1;
            else if (zoneType.Contains("Joueur2"))
                zoneOwner = 2;
            else
            {
                MessageBox.Show("Zone avec un propriétaire non défini !");
                return;
            }

            if (stats.Owner != zoneOwner)
            {
                MessageBox.Show("Vous ne pouvez pas placer une unité dans la zone adverse, mon Seigneur !");
                return;
            }

            if (zoneType.Contains("attack") && stats.Id != 1)
            {
                MessageBox.Show("Une unité de défense ne peut être placée en zone d'attaque !");
                return;
            }
            if (zoneType.Contains("defense") && stats.Id != 2)
            {
                MessageBox.Show("Une unité d'attaque ne peut être placée en zone de défense !");
                return;
            }

            if (!(zoneBorder.Child is StackPanel zone) || zone.Children.Count >= 3)
            {
                MessageBox.Show("Cette zone est déjà pleine ou mal définie !");
                return;
            }

            if (carteSelectionnee.Parent is Panel ancienPanel)
                ancienPanel.Children.Remove(carteSelectionnee);

            carteSelectionnee.BorderBrush = Brushes.Black;
            zone.Children.Add(carteSelectionnee);

            

            


            carteSelectionnee = null;
            MessageTextBlock.Text = "Unité placée, mon Seigneur !";
            e.Handled = true;
        }

        

        private void FinDeManche_Click(object sender, RoutedEventArgs e)
        {
            List<Border> attackCardsJ1 = StackZoneJoueur1Attack.Children.OfType<Border>().ToList();
            List<Border> attackCardsJ2 = StackZoneJoueur2Attack.Children.OfType<Border>().ToList();

            int totalDamageJoueur1 = 0;
            int totalDamageJoueur2 = 0;

            int nbDuels = Math.Min(attackCardsJ1.Count, attackCardsJ2.Count);
            for (int i = 0; i < nbDuels; i++)
            {
                if (attackCardsJ1[i].Tag is CardStats statsJ1 && attackCardsJ2[i].Tag is CardStats statsJ2)
                {
                    // Calcul des attaques effectives en tenant compte de certaines compétences
                    int effectiveAttackJ1 = statsJ1.Attaque;
                    int effectiveAttackJ2 = statsJ2.Attaque;

                    // Par exemple : bonus d'attaque de +2 si la compétence "Aggressive" est activée (id_attitude == 1)
                    if (statsJ1.IdAttitude == 1)
                        effectiveAttackJ1 += 2;
                    if (statsJ2.IdAttitude == 1)
                        effectiveAttackJ2 += 2;

                    // Par exemple : bonus de PV de +3 si la compétence "Défensive" est activée (id_attitude == 2)
                    if (statsJ1.IdAttitude == 2)
                        statsJ1.PointsDeVie += 3;
                    if (statsJ2.IdAttitude == 2)
                        statsJ2.PointsDeVie += 3;

                    statsJ1.PointsDeVie -= effectiveAttackJ2;
                    statsJ2.PointsDeVie -= effectiveAttackJ1;

                    totalDamageJoueur1 += effectiveAttackJ1;
                    totalDamageJoueur2 += effectiveAttackJ2;

                    if (statsJ1.PointsDeVie <= 0)
                    {
                        if (!cartesMortesJoueur1.Contains(statsJ1.Nom))
                            cartesMortesJoueur1.Add(statsJ1.Nom);
                        orJoueur2 += 2;
                    }
                    if (statsJ2.PointsDeVie <= 0)
                    {
                        if (!cartesMortesJoueur2.Contains(statsJ2.Nom))
                            cartesMortesJoueur2.Add(statsJ2.Nom);
                        orJoueur1 += 2;
                    }
                }
            }

            if (attackCardsJ1.Count > nbDuels)
            {
                for (int i = nbDuels; i < attackCardsJ1.Count; i++)
                {
                    if (attackCardsJ1[i].Tag is CardStats statsJ1)
                        totalDamageJoueur1 += statsJ1.Attaque;
                }
            }
            if (attackCardsJ2.Count > nbDuels)
            {
                for (int i = nbDuels; i < attackCardsJ2.Count; i++)
                {
                    if (attackCardsJ2[i].Tag is CardStats statsJ2)
                        totalDamageJoueur2 += statsJ2.Attaque;
                }
            }

            MessageTextBlock.Text = $"Joueur 1 a infligé {totalDamageJoueur1} dégâts.\n" +
                                    $"Joueur 2 a infligé {totalDamageJoueur2} dégâts.";

            RetirerCartesMortes(attackCardsJ1, StackZoneJoueur1Attack);
            RetirerCartesMortes(attackCardsJ2, StackZoneJoueur2Attack);

            if (cartesMortesJoueur1.Count >= 6 || orJoueur1 <= 0)
            {
                vieJoueur1--;
                if (vieJoueur1 > 0)
                    MessageBox.Show($"Joueur 1 a perdu la manche et perd une vie. Il lui reste {vieJoueur1} vie(s).");
                else
                    MessageBox.Show("Joueur 1 n'a plus de vie, game over !");
                orJoueur1 = 20;
                cartesMortesJoueur1.Clear();
            }
            if (cartesMortesJoueur2.Count >= 6 || orJoueur2 <= 0)
            {
                vieJoueur2--;
                if (vieJoueur2 > 0)
                    MessageBox.Show($"Joueur 2 a perdu la manche et perd une vie. Il lui reste {vieJoueur2} vie(s).");
                else
                    MessageBox.Show("Joueur 2 n'a plus de vie, game over !");
                orJoueur2 = 20;
                cartesMortesJoueur2.Clear();
            }

            MettreAJourAffichageOr();
            MettreAJourAffichageVies();
            // ResetGame() est laissé en commentaire pour laisser le temps de visualiser le résultat des duels.
            // ResetGame();
        }

        private void RetirerCartesMortes(List<Border> cards, Panel zone)
        {
            foreach (var card in cards.ToList())
            {
                if (card.Tag is CardStats stats && stats.PointsDeVie <= 0)
                    zone.Children.Remove(card);
            }
        }

        private void AfficherCartesMortes_Click(object sender, RoutedEventArgs e)
        {
            string messageJ1 = cartesMortesJoueur1.Count == 0
                ? "Aucune carte morte pour Joueur 1."
                : string.Join("\n", cartesMortesJoueur1);
            string messageJ2 = cartesMortesJoueur2.Count == 0
                ? "Aucune carte morte pour Joueur 2."
                : string.Join("\n", cartesMortesJoueur2);

            MessageBox.Show($"Cartes mortes Joueur 1 :\n{messageJ1}\n\nCartes mortes Joueur 2 :\n{messageJ2}", "Unités éliminées");
        }

        private void AbandonnerJoueur1_Click(object sender, RoutedEventArgs e)
        {
            vieJoueur1--;
            if (vieJoueur1 > 0)
                MessageBox.Show($"Joueur 1 a abandonné, il lui reste {vieJoueur1} vie(s).\nJoueur 2 remporte la manche.");
            else
                MessageBox.Show("Joueur 1 a abandonné et n'a plus de vie, game over !");
            MettreAJourAffichageVies();
            ResetGame();
        }

        private void AbandonnerJoueur2_Click(object sender, RoutedEventArgs e)
        {
            vieJoueur2--;
            if (vieJoueur2 > 0)
                MessageBox.Show($"Joueur 2 a abandonné, il lui reste {vieJoueur2} vie(s).\nJoueur 1 remporte la manche.");
            else
                MessageBox.Show("Joueur 2 a abandonné et n'a plus de vie, game over !");
            MettreAJourAffichageVies();
            ResetGame();
        }

        private void ResetGame()
        {
            orJoueur1 = 20;
            orJoueur2 = 20;
            MettreAJourAffichageOr();

            CarteContainerJoueur1.Children.Clear();
            CarteContainerJoueur2.Children.Clear();
            StackZoneJoueur1Attack.Children.Clear();
            StackZoneJoueur1Defense.Children.Clear();
            StackZoneJoueur2Attack.Children.Clear();
            StackZoneJoueur2Defense.Children.Clear();

            cartesMortesJoueur1.Clear();
            cartesMortesJoueur2.Clear();

            MessageTextBlock.Text = "La manche est réinitialisée, mon Seigneur !";

            ChargerPioche();
            GenererNextCardPreview();
        }
    }

    public class CardStats
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int Attaque { get; set; }
        public int PointsDeVie { get; set; }
        public int Prix { get; set; }
        public string CheminImage { get; set; }
        public int Owner { get; set; }

        // Nouvelles propriétés pour la compétence spéciale
        public int IdAttitude { get; set; }
        public string AttitudeType { get; set; }
    }
}
