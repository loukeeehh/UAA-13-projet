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

        // Cartes mortes (défaite à 5)
        private List<string> cartesMortesJoueur1 = new List<string>();
        private List<string> cartesMortesJoueur2 = new List<string>();

        // Carte sélectionnée pour placement
        private Border carteSelectionnee = null;

        public Plateau()
        {
            InitializeComponent();
            ChargerPioche();
            MettreAJourAffichageOr();
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
            // Mise à jour du texte affiché pour chaque joueur
            OrTextBlockJoueur1.Text = $"Joueur 1 : {orJoueur1}";
            OrTextBlockJoueur2.Text = $"Joueur 2 : {orJoueur2}";

            
        }

        // Méthode de pioche commune
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

            CardStats statsCard = new CardStats
            {
                Id = idCarte,
                Nom = nomCarte,
                Attaque = attaqueCarte,
                PointsDeVie = pvCarte,
                Prix = prixCarte,
                CheminImage = imagePath
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

        // Gestion de la sélection d'une carte dans le banc
        private void CartePiochee_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card)
            {
                // Réinitialiser la bordure de l'ancienne carte sélectionnée
                carteSelectionnee?.SetValue(Border.BorderBrushProperty, Brushes.Black);
                carteSelectionnee = card;
                carteSelectionnee.BorderBrush = Brushes.Red;
                MessageTextBlock.Text = "Unité sélectionnée, mon Seigneur !";
                e.Handled = true;
            }
        }

        // Placement de la carte sélectionnée dans une zone
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

            // Exemple de contrôle de type (adapter selon vos règles)
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

        // Phase de combat à la fin de la manche
        private void FinDeManche_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les cartes d'attaque de chaque joueur
            List<Border> attackCardsJ1 = StackZoneJoueur1Attack.Children.OfType<Border>().ToList();
            List<Border> attackCardsJ2 = StackZoneJoueur2Attack.Children.OfType<Border>().ToList();

            int totalDamageJoueur1 = 0;
            int totalDamageJoueur2 = 0;

            int nbDuels = Math.Min(attackCardsJ1.Count, attackCardsJ2.Count);
            for (int i = 0; i < nbDuels; i++)
            {
                if (attackCardsJ1[i].Tag is CardStats statsJ1 && attackCardsJ2[i].Tag is CardStats statsJ2)
                {
                    // Danger réciproque : échange de dégâts
                    statsJ1.PointsDeVie -= statsJ2.Attaque;
                    statsJ2.PointsDeVie -= statsJ1.Attaque;

                    totalDamageJoueur1 += statsJ1.Attaque;
                    totalDamageJoueur2 += statsJ2.Attaque;

                    // Attribution de 2 pièces d'or pour chaque carte éliminée
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

            // Gérer les cartes en surplus (sans duel direct)
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
                MessageBox.Show("Joueur 1 a perdu, mon Seigneur !");
                orJoueur1 = 20;
                cartesMortesJoueur1.Clear();
            }
            if (cartesMortesJoueur2.Count >= 6 || orJoueur2 <= 0)
            {
                MessageBox.Show("Joueur 2 a perdu, mon Seigneur !");
                
                orJoueur2 = 20;
                cartesMortesJoueur2.Clear();
            }

            MettreAJourAffichageOr();
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
    }

    public class CardStats
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int Attaque { get; set; }
        public int PointsDeVie { get; set; }
        public int Prix { get; set; }
        public string CheminImage { get; set; }
    }
}
