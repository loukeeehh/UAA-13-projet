using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    public partial class Plateau : Page
    {
        private DataSet pioche = new DataSet();
        private Random random = new Random();
        private int orJoueur = 20;
        private Border carteSelectionnee = null;
        private List<string> cartesMortes = new List<string>();


        // PV et ATQ de l'ennemi fictif utilisés pour le test
        private int enemyPV = 50;
        private int enemyATQ = 1;

        public Plateau()
        {
            InitializeComponent();
            ChargerPioche();
            MettreAJourAffichageOr();

            // Mise à jour initiale des PV de l'ennemi
            if (EnemyHealthTextBlock != null)
                EnemyHealthTextBlock.Text = $"PV Ennemi : {enemyPV}";
        }

        // Classe contenant les statistiques d'une carte.
        public class CardStats
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public int Attaque { get; set; }
            public int PointsDeVie { get; set; }
            public int Prix { get; set; }
            public string CheminImage { get; set; }
        }

        // Chargement des cartes depuis la base de données
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

        // Mise à jour de l'affichage de l'or et du message associé
        private void MettreAJourAffichageOr()
        {
            OrTextBlock.Text = orJoueur.ToString();
            if (orJoueur >= 10)
                OrMessageTextBlock.Text = "Nous avons un trésor conséquent, mon Seigneur !";
            else if (orJoueur >= 5)
                OrMessageTextBlock.Text = "Nos ressources s'amenuisent, mon Seigneur.";
            else if (orJoueur >= 2)
                OrMessageTextBlock.Text = "Nous sommes presque à court d'or, mon Seigneur !";
            else
                OrMessageTextBlock.Text = "Les caisses sont vides, mon Seigneur !";
        }

        // Méthode pour piocher une carte
        private void PiocherCarte()
        {
            if (!pioche.Tables.Contains("carte") || pioche.Tables["carte"].Rows.Count == 0)
            {
                MessageBox.Show("La pioche est vide, mon Seigneur !");
                return;
            }
            if (CarteContainer.Children.Count >= 3)
            {
                MessageBox.Show("Le banc est plein, mon Seigneur !");
                return;
            }

            int index = random.Next(pioche.Tables["carte"].Rows.Count);
            DataRow row = pioche.Tables["carte"].Rows[index];

            // Récupération des informations de la carte
            string imagePath = row["CheminImage"].ToString();
            string nomCarte = row["Nom_carte"].ToString();
            int idCarte = Convert.ToInt32(row["id_type"]);
            int prixCarte = Convert.ToInt32(row["Prix_carte"]);
            int attaqueCarte = Convert.ToInt32(row["Attaque_carte"]);
            System.Diagnostics.Debug.WriteLine($"Attaque récupérée : {attaqueCarte}");
            int pvCarte = Convert.ToInt32(row["PV_carte"]);

            if (orJoueur < prixCarte)
            {
                MessageBox.Show("Vous n'avez pas assez d'or, mon Seigneur !");
                return;
            }

            orJoueur -= prixCarte;
            MettreAJourAffichageOr();

            // Création d'un objet CardStats pour la carte piochée
            CardStats statsCard = new CardStats
            {
                Id = idCarte,
                Nom = nomCarte,
                Attaque = attaqueCarte,
                PointsDeVie = pvCarte,
                Prix = prixCarte,
                CheminImage = imagePath
            };

            // Création du Border qui contiendra la carte
            Border cardBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(2),
                Margin = new Thickness(2),
                Width = 145,
                Background = Brushes.White,
                Tag = statsCard // Stocke l'objet complet dans le Tag
            };

            StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };

            // Création de l'image de la carte
            Image image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 190,
                Height = 182,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0)
            };

            // Titre de la carte
            TextBlock title = new TextBlock
            {
                Text = nomCarte,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Affichage des statistiques initiales de la carte
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

            // Gestion de la sélection de la carte au clic
            cardBorder.MouseDown += CartePiochée_Click;
            CarteContainer.Children.Add(cardBorder);

            MessageTextBlock.Text = "Unité piochée, mon Seigneur !";
            pioche.Tables["carte"].Rows.RemoveAt(index);
        }

        // Méthode pour simuler le combat d'une carte sélectionnée (en cas d'attaque individuelle)
        private void Combattre_Click(object sender, RoutedEventArgs e)
        {
            if (carteSelectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une unitée pour combattre, mon Seigneur !");
                return;
            }

            // Récupération de l'objet CardStats stocké dans le Tag de la carte sélectionnée
            CardStats statsCard = carteSelectionnee.Tag as CardStats;
            if (statsCard == null)
            {
                MessageBox.Show("Impossible de récupérer les informations de la carte !");
                return;
            }

            // La carte inflige ses dégâts à l'ennemi
            enemyPV -= statsCard.Attaque;
            if (enemyPV < 0)
                enemyPV = 0;
            EnemyHealthTextBlock.Text = $"PV Ennemi : {enemyPV}";

            // L'ennemi contre-attaque la carte
            statsCard.PointsDeVie -= enemyATQ;
            if (statsCard.PointsDeVie < 0)
                statsCard.PointsDeVie = 0;

            // Mise à jour de l'affichage des statistiques de la carte
            if (carteSelectionnee.Child is StackPanel sp && sp.Children.Count >= 3 && sp.Children[2] is TextBlock statsTb)
            {
                statsTb.Text = $"PV: {statsCard.PointsDeVie} | ATQ: {statsCard.Attaque} | PRIX: {statsCard.Prix}";
            }

            MessageTextBlock.Text = $"Vous avez infligé {statsCard.Attaque} dégâts à l'ennemi et encaissé {enemyATQ} en retour ! " +
                                      $"(Carte : {statsCard.PointsDeVie} PV restants ; Ennemi : {enemyPV} PV)";

            // Si l'ennemi est vaincu, réinitialisation pour futurs combats
            if (enemyPV == 0)
            {
                MessageBox.Show("L'ennemi est vaincu, mon Seigneur !");
                enemyPV = 100;
                EnemyHealthTextBlock.Text = $"PV Ennemi : {enemyPV}";
            }

            // Si la carte est détruite, la retirer du plateau
            if (statsCard.PointsDeVie == 0)
            {
                cartesMortes.Add(statsCard.Nom);
                MessageBox.Show($"Votre unité {statsCard.Nom} est morte, mon Seigneur !");
                if (carteSelectionnee.Parent is Panel parentPanel)
                    parentPanel.Children.Remove(carteSelectionnee);
                carteSelectionnee = null;
            }

        }

        // Méthode gérant la sélection d'une carte sur le banc
        private void CartePiochée_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card)
            {
                // Réinitialiser la bordure de l'ancienne carte sélectionnée (si elle existe)
                if (carteSelectionnee != null)
                    carteSelectionnee.BorderBrush = Brushes.Black;
                carteSelectionnee = card;
                carteSelectionnee.BorderBrush = Brushes.Red;
                MessageTextBlock.Text = "Unitée sélectionnée, mon Seigneur !";

                e.Handled = true; // Empêche la propagation de l'événement vers le parent
            }
        }


        // Bouton de piocher une carte
        private void PiocherCarte_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte();
        }

        // Déplacement manuel d'une carte sélectionnée vers une zone (lors d'un clic sur la zone)
        private void Zone_Click(object sender, MouseButtonEventArgs e)
        {
            // Vérifier qu'une unité est sélectionnée
            if (carteSelectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une unité, mon Seigneur !");
                return;
            }

            // Le sender doit être le Border de la zone
            if (!(sender is Border zoneBorder))
            {
                MessageBox.Show("Zone non reconnue !");
                return;
            }

            // Récupérer le tag de la zone (doit être "attack" ou "defense")
            string zoneType = zoneBorder.Tag as string;
            if (string.IsNullOrEmpty(zoneType))
            {
                MessageBox.Show("La zone ne possède pas de type défini, vérifiez votre XAML !");
                return;
            }

            // L'unité doit être contenue dans un objet CardStats
            if (!(carteSelectionnee.Tag is CardStats stats))
            {
                MessageBox.Show("Impossible d'identifier l'unité sélectionnée !");
                return;
            }

            // Vérifier que l'unité correspond au type de la zone
            if (zoneType == "attack" && stats.Id != 1)
            {
                MessageBox.Show("Une unité de défense ne peut être placée en zone d'attaque, mon Seigneur !");
                return;
            }
            if (zoneType == "defense" && stats.Id != 2)
            {
                MessageBox.Show("Une unité d'attaque ne peut être placée en zone de défense, mon Seigneur !");
                return;
            }

            // Puisque la zone est trouvée et qu'elle n'est pas pleine
            if (!(zoneBorder.Child is StackPanel zone))
            {
                MessageBox.Show("Zone introuvable, vérifiez votre XAML !");
                return;
            }

            if (zone.Children.Count >= 3)
            {
                MessageBox.Show("Cette zone est déjà pleine, mon Seigneur !");
                return;
            }

            // Retirer l'unité de son conteneur actuel
            if (carteSelectionnee.Parent is Panel ancienPanel)
            {
                ancienPanel.Children.Remove(carteSelectionnee);
            }

            // Remise à l'état non sélectionné
            carteSelectionnee.BorderBrush = Brushes.Black;

            // Ajouter l'unité dans la zone cliquée
            zone.Children.Add(carteSelectionnee);
            carteSelectionnee = null;

            MessageTextBlock.Text = "Unité placée, mon Seigneur !";
            e.Handled = true;
        }
        //affiche les cartes morte dans un messagebox
        private void AfficherCartesMortes_Click(object sender, RoutedEventArgs e)
        {
            if (cartesMortes.Count == 0)
            {
                MessageBox.Show("Aucune carte n'est morte pour l'instant, mon Seigneur !");
                return;
            }

            string message = "C'est unitée ne sont plus jouable mon seigneur  :\n" + string.Join("\n", cartesMortes);
            MessageBox.Show(message, "Unitée Eliminées");
        }



        // Fin de Manche : toutes les cartes du banc attaquent l'ennemi dans l'ordre d'insertion
        private void FinDeManche_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les unités présentes dans la zone d'attaque
            var cardsInAttackZone = StackZone2.Children.Cast<Border>().ToList();
            int totalDamageDealt = 0;
            List<Border> cardsToRemove = new List<Border>();

            foreach (Border card in cardsInAttackZone)
            {
                if (card.Tag is CardStats stats)
                {
                    // Si c'est une unité d'attaque (on suppose que stats.Id == 1 signifie "attaque")
                    if (stats.Id == 1)
                    {
                        enemyPV -= stats.Attaque;
                        totalDamageDealt += stats.Attaque;
                        if (enemyPV < 0)
                            enemyPV = 0;
                    }

                    // L'ennemi contre-attaque : chaque unité perd enemyATQ PV
                    stats.PointsDeVie -= enemyATQ;
                    if (stats.PointsDeVie < 0)
                        stats.PointsDeVie = 0;

                    // Mise à jour de l'affichage des statistiques de l'unité
                    if (card.Child is StackPanel sp && sp.Children.Count >= 3 && sp.Children[2] is TextBlock tb)
                    {
                        tb.Text = $"PV: {stats.PointsDeVie} | ATQ: {stats.Attaque} | PRIX: {stats.Prix}";
                    }

                    // Si l'unité est détruite, la planquer
                    if (stats.PointsDeVie == 0)
                    {
                        cartesMortes.Add(stats.Nom);
                        cardsToRemove.Add(card);
                    }

                }
            }

            // Supprimer les unités détruites
            foreach (var card in cardsToRemove)
            {
                StackZone2.Children.Remove(card);
            }

            // Mise à jour de l'affichage de l'ennemi et des dégâts
            EnemyHealthTextBlock.Text = $"PV Ennemi : {enemyPV}";
            MessageTextBlock.Text = $"Vos unités ont infligé un total de {totalDamageDealt} dégâts à l'ennemi. PV Ennemi : {enemyPV}";

            if (enemyPV == 0)
            {
                MessageBox.Show("L'ennemi est vaincu, mon Seigneur !");
                enemyPV = 100;
                EnemyHealthTextBlock.Text = $"PV Ennemi : {enemyPV}";
            }
        }






    }
}
