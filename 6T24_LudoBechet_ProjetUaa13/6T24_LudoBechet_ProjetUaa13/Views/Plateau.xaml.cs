using System;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Windows.Input;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    public partial class Plateau : Page
    {
        private DataSet pioche = new DataSet();
        private Random random = new Random();
        private int orJoueur = 99;
        private Image carteSelectionnee = null; // Carte actuellement sélectionnée

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

                if (!pioche.Tables.Contains("carte") || pioche.Tables["carte"].Rows.Count == 0)
                {
                    MessageBox.Show("Il n'y a pas de cartes dans la base de données.");
                }
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
                Width = 140,
                Background = Brushes.White
            };

            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical };

            // Image de la carte
            Image nouvelleCarte = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 130,
                Height = 160,
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

            nouvelleCarte.MouseDown += CartePiochée_Click;

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
            OrTextBlock.Text = orJoueur.ToString();
        }

        private void PiocherCarte_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte();
        }

        private void CartePiochée_Click(object sender, MouseButtonEventArgs e)
        {
            carteSelectionnee = sender as Image;
            if (carteSelectionnee != null)
            {
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

            Grid zone = sender as Grid;
            if (zone == null) return;

            // Vérifier s'il y a déjà 4 cartes dans la zone
            if (zone.Children.Count >= 4)
            {
                MessageBox.Show("Cette zone est déjà pleine !");
                return;
            }

            // Supprimer la carte de son ancien emplacement
            if (carteSelectionnee.Tag is Border ancienContainer)
            {
                CarteContainer.Children.Remove(ancienContainer);
            }

            if (carteSelectionnee.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(carteSelectionnee);
            }

            // Ajouter la carte à la zone (en ligne 0 si vide, sinon en ligne 1)
            int rowIndex = zone.Children.Count;
            Grid.SetRow(carteSelectionnee, rowIndex);
            zone.Children.Add(carteSelectionnee);

            // Réinitialiser la sélection
            carteSelectionnee = null;

            MessageTextBlock.Text = "Carte placée, mon Seigneur !";
        }


    }
}
