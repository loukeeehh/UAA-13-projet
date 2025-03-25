using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using System.Security.Policy;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    public partial class Plateau : Page
    {
        private DataSet pioche = new DataSet();
        private Random random = new Random();
        private int orJoueur = 20;
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
                bdd maBdd = new bdd();
                pioche = maBdd.ObtenirCartes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la pioche : {ex.Message}");
            }
        }

        private void PiocherCarte()
        {
            if (!pioche.Tables.Contains("carte") || pioche.Tables["carte"].Rows.Count == 0)
            {
                MessageBox.Show("La pioche est vide, mon Seigneur !");
                return;
            }
            if (CarteContainer.Children.Count >= 3) // Vérification du nombre de cartes sur le banc
            {
                MessageBox.Show("Le banc est plein, mon Seigneur !");
                return;
            }

            int index = random.Next(pioche.Tables["carte"].Rows.Count);
            DataRow carte = pioche.Tables["carte"].Rows[index];

            string imagePath = carte["CheminImage"].ToString();
            string nomCarte = carte["Nom_carte"].ToString();
            int prixCarte = Convert.ToInt32(carte["Prix_carte"]);
            int attaqueCarte = Convert.ToInt32(carte["Attaque_carte"]);
            int pvCarte = Convert.ToInt32(carte["PV_carte"]);

            if (orJoueur < prixCarte)
            {
                MessageBox.Show("Vous n'avez pas assez d'or, mon Seigneur !");
                return;
            }
            

            orJoueur -= prixCarte;
            MettreAJourAffichageOr();

            // Conteneur principal
            Border carteContainer = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(2),
                Margin = new Thickness(2),
                Width = 145,
                Background = Brushes.White,
                Tag = nomCarte // Stocker le nom pour l'identification
            };

            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical };

            // Image de la carte
            Image nouvelleCarte = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 140,
                Height = 172,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };

            // Ajout du texte
            TextBlock title = new TextBlock
            {
                Text = nomCarte,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock stats = new TextBlock
            {
                Text = $"PV: {pvCarte} | ATQ: {attaqueCarte} | PRIX: {prixCarte}",
                FontSize = 12,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Attacher l'événement MouseDown au Border (au lieu de l'image seule)
            carteContainer.MouseDown += CartePiochée_Click;

            // Ajout des éléments dans le StackPanel
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(nouvelleCarte);
            stackPanel.Children.Add(stats);

            // Encapsuler dans le Border
            carteContainer.Child = stackPanel;

            // Ajouter la carte au conteneur de cartes piochées
            CarteContainer.Children.Add(carteContainer);

            MessageTextBlock.Text = "Unité piochée, mon Seigneur !";

            pioche.Tables["carte"].Rows.RemoveAt(index);
        }


        private void MettreAJourAffichageOr()
        {
            OrTextBlock.Text = orJoueur.ToString(); // Met à jour l'affichage de l'or

            if (orJoueur >= 10)
            {
                OrMessageTextBlock.Text = "Nous avons un trésor conséquent, mon Seigneur !";
            }
            else if (orJoueur >= 5)
            {
                OrMessageTextBlock.Text = "Nos ressources s'amenuisent, mon Seigneur.";
            }
            else if (orJoueur >= 2)
            {
                OrMessageTextBlock.Text = "Nous sommes à court d'or, mon Seigneur !";
            }
            else
            {
                OrMessageTextBlock.Text = "Les caisses sont vides, mon Seigneur !";
            }
        }



        private void PiocherCarte_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte();
        }

        private void CartePiochée_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border carte)
            {
                // Réinitialiser la couleur de la carte précédemment sélectionnée
                if (carteSelectionnee != null)
                {
                    carteSelectionnee.BorderBrush = Brushes.Black;
                }

                // Sélectionner la nouvelle carte
                carteSelectionnee = carte;
                carteSelectionnee.BorderBrush = Brushes.Red;

                MessageTextBlock.Text = "Carte sélectionnée, mon Seigneur !";
            }
        }





        private void Zone_Click(object sender, MouseButtonEventArgs e)
        {
            if (carteSelectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une carte, mon Seigneur !");
                return;
            }

            // Vérifier si l'expéditeur est un Border
            Border zoneBorder = sender as Border;
            if (zoneBorder == null)
            {
                MessageBox.Show("Clic non reconnu !");
                return;
            }

            // Trouver le bon StackPanel en fonction du Border cliqué
            StackPanel zone = null;
            if (zoneBorder == StackZone1.Parent) // Vérifier si le parent est le bon
            {
                zone = StackZone1;
            }
            else if (zoneBorder == StackZone2.Parent)
            {
                zone = StackZone2;
            }

            if (zone == null)
            {
                MessageBox.Show("Zone introuvable !");
                return;
            }

            // Vérifier le nombre maximum de cartes (limite à 3)
            if (zone.Children.Count >= 3)
            {
                MessageBox.Show("Cette zone est déjà pleine !");
                return;
            }

            // Retirer la carte de son ancien emplacement
            if (carteSelectionnee.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(carteSelectionnee);
            }

            // Réinitialiser la bordure de la carte avant de la placer
            carteSelectionnee.BorderBrush = Brushes.Black;

            // Ajouter la carte au bon StackPanel
            zone.Children.Add(carteSelectionnee);

            // Réinitialiser la sélection
            carteSelectionnee = null;

            MessageTextBlock.Text = "Carte placée, mon Seigneur !";
        }





    }
}
