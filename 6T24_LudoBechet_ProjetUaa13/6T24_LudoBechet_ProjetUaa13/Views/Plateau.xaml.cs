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
        private int orJoueur = 20;
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
            int prixCarte = Convert.ToInt32(carte["Prix_carte"]);

            if (orJoueur < prixCarte)
            {
                MessageBox.Show("Vous n'avez pas assez d'or, mon Seigneur !");
                return;
            }

            orJoueur -= prixCarte;
            MettreAJourAffichageOr();

            Image nouvelleCarte = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 130,
                Height = 160,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };

            nouvelleCarte.MouseDown += CartePiochée_Click;

            CarteContainer.Children.Add(nouvelleCarte);
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

        private void Rectangle_Click(object sender, RoutedEventArgs e)
        {
            if (carteSelectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une carte, mon Seigneur !");
                return;
            }

            Rectangle rectangle = sender as Rectangle;
            if (rectangle == null) return;

            Image imageExistante = null;
            foreach (UIElement element in grdMain.Children)
            {
                if (Grid.GetRow(element) == Grid.GetRow(rectangle) &&
                    Grid.GetColumn(element) == Grid.GetColumn(rectangle) &&
                    element is Image)
                {
                    imageExistante = element as Image;
                    break;
                }
            }

            if (imageExistante != null)
            {
                var result = MessageBox.Show("Voulez-vous remplacer cette carte ?", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                    return;

                grdMain.Children.Remove(imageExistante);
            }

            Grid.SetRow(carteSelectionnee, Grid.GetRow(rectangle));
            Grid.SetColumn(carteSelectionnee, Grid.GetColumn(rectangle));
            // Vérifier d'abord si la carte appartient encore à CarteContainer, puis la détacher
            if (carteSelectionnee.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(carteSelectionnee);
            }

            // Ajouter la carte au plateau
            grdMain.Children.Add(carteSelectionnee);

            carteSelectionnee = null;

            MessageTextBlock.Text = "Carte placée, mon Seigneur !";
        }
    }
}
