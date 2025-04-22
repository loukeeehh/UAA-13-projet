using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public Plateau()
        {
            InitializeComponent();
            ChargerPioche();
            MettreAJourAffichageOr();
            MettreAJourAffichageVies();
        }

        private void ChargerPioche()
        {
            try
            {
                // Cette classe doit implémenter la récupération des cartes depuis la BDD.
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
            // Mise à jour de l'affichage de l'or pour chaque joueur
            OrTextBlockJoueur1.Text = $"Joueur 1 : {orJoueur1}";
            OrTextBlockJoueur2.Text = $"Joueur 2 : {orJoueur2}";
        }

        private void MettreAJourAffichageVies()
        {
            // Actualisation de l'affichage des vies (assurez-vous que les TextBlock correspondants existent dans le XAML)
            VieTextBlockJoueur1.Text = $"Vies : {vieJoueur1}";
            VieTextBlockJoueur2.Text = $"Vies : {vieJoueur2}";
        }

        // Méthode de pioche commune pour les deux joueurs
        private void PiocherCarte(bool isJoueur1)
        {
            Panel bankContainer = isJoueur1 ? CarteContainerJoueur1 : CarteContainerJoueur2;

            if (!pioche.Tables.Contains("carte") || pioche.Tables["carte"].Rows.Count == 0)
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

            int index = random.Next(pioche.Tables["carte"].Rows.Count);
            DataRow row = pioche.Tables["carte"].Rows[index];

            string imagePath = row["CheminImage"].ToString();
            string nomCarte = row["Nom_carte"].ToString();
            int idCarte = Convert.ToInt32(row["id_type"]);
            int prixCarte = Convert.ToInt32(row["Prix_carte"]);
            int attaqueCarte = Convert.ToInt32(row["Attaque_carte"]);
            int pvCarte = Convert.ToInt32(row["PV_carte"]);

            if (isJoueur1)
            {
                if (orJoueur1 < prixCarte)
                {
                    MessageBox.Show("Joueur 1 n'a pas assez d'or, mon Seigneur !");
                    return;
                }
                orJoueur1 -= prixCarte;
            }
            else
            {
                if (orJoueur2 < prixCarte)
                {
                    MessageBox.Show("Joueur 2 n'a pas assez d'or, mon Seigneur !");
                    return;
                }
                orJoueur2 -= prixCarte;
            }

            MettreAJourAffichageOr();

            // Création des statistiques de la carte et assignation du propriétaire
            CardStats statsCard = new CardStats
            {
                Id = idCarte,
                Nom = nomCarte,
                Attaque = attaqueCarte,
                PointsDeVie = pvCarte,
                Prix = prixCarte,
                CheminImage = imagePath,
                Owner = isJoueur1 ? 1 : 2  // 1 pour Joueur 1, 2 pour Joueur 2
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
                Text = $"PV: {pvCarte} | ATQ: {attaqueCarte} | PRIX: {prixCarte}",
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

            pioche.Tables["carte"].Rows.RemoveAt(index);
        }

        // Boutons distincts pour chaque joueur
        private void PiocherCarteJoueur1_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(true);
        }

        private void PiocherCarteJoueur2_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(false);
        }

        // Sélection d'une carte dans le banc
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

        // Placement de la carte sélectionnée dans une zone donnée
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

            // Déduire le propriétaire de la zone à partir du tag ("Joueur1" ou "Joueur2")
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

            // Contrôle de type (exemple : attaque/défense) adapté selon vos règles
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

        // Gestion de la phase de combat en fin de manche
        private void FinDeManche_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les cartes d'attaque des deux joueurs
            List<Border> attackCardsJ1 = StackZoneJoueur1Attack.Children.OfType<Border>().ToList();
            List<Border> attackCardsJ2 = StackZoneJoueur2Attack.Children.OfType<Border>().ToList();

            int totalDamageJoueur1 = 0;
            int totalDamageJoueur2 = 0;

            int nbDuels = Math.Min(attackCardsJ1.Count, attackCardsJ2.Count);
            for (int i = 0; i < nbDuels; i++)
            {
                if (attackCardsJ1[i].Tag is CardStats statsJ1 && attackCardsJ2[i].Tag is CardStats statsJ2)
                {
                    // Combat réciproque : les cartes s'infligent mutuellement des dégâts
                    statsJ1.PointsDeVie -= statsJ2.Attaque;
                    statsJ2.PointsDeVie -= statsJ1.Attaque;

                    totalDamageJoueur1 += statsJ1.Attaque;
                    totalDamageJoueur2 += statsJ2.Attaque;

                    // Si une unité est éliminée, attribuer 2 pièces d'or à l'adversaire
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

            // Gestion des cartes en surplus (sans duel direct)
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

            // Détection de la défaite de manche pour chaque joueur
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

            // Remarque : l'appel à ResetGame() est commenté pour laisser le temps de voir le résultat des combats.
            // Vous pouvez le déclencher manuellement (par exemple via un bouton "Nouvelle Manche")
            // ResetGame();
        }

        // Suppression des cartes éliminées de la zone d'attaque
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

        // Boutons d'abandon pour chaque joueur : diminution d'une vie puis réinitialisation de la manche
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

        // Réinitialise la manche (les vies sont conservées d'une manche à l'autre)
        private void ResetGame()
        {
            // Réinitialiser l'or
            orJoueur1 = 20;
            orJoueur2 = 20;
            MettreAJourAffichageOr();

            // Effacer les zones de jeu et les bancs
            CarteContainerJoueur1.Children.Clear();
            CarteContainerJoueur2.Children.Clear();
            StackZoneJoueur1Attack.Children.Clear();
            StackZoneJoueur1Defense.Children.Clear();
            StackZoneJoueur2Attack.Children.Clear();
            StackZoneJoueur2Defense.Children.Clear();

            // Réinitialiser les listes de cartes mortes pour la manche
            cartesMortesJoueur1.Clear();
            cartesMortesJoueur2.Clear();

            MessageTextBlock.Text = "La manche est réinitialisée, mon Seigneur !";

            // (Optionnel) Recharger la pioche
            ChargerPioche();
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
        // 1 pour Joueur 1, 2 pour Joueur 2
        public int Owner { get; set; }
    }
}
