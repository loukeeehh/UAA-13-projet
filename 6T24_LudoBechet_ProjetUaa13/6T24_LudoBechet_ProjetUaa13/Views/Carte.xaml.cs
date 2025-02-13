using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using _6T24_LudoBechet_ProjetUaa13;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    /// <summary>
    /// Logique d'interaction pour Carte.xaml
    /// </summary>
    public partial class Carte : Page
    {
        public Carte()
        {
            InitializeComponent();
            ChargerDonneesCarte(); // Charger les données à l'ouverture de la page
        }

        private void ChargerDonneesCarte()
        {
            try
            {
                // Connexion à la base de données
                bdd maBdd = new bdd();

                // Récupération des données depuis la BDD
                if (maBdd.ChercheCarte(out DataSet infos))
                {
                    // Nettoyer le conteneur avant d'ajouter les nouvelles cartes
                    CarteContainer.Children.Clear();

                    // Vérifier si la table "carte" contient des données
                    if (infos.Tables.Contains("carte") && infos.Tables["carte"].Rows.Count > 0)
                    {
                        foreach (DataRow row in infos.Tables["carte"].Rows)
                        {
                            // Récupération des données avec les bons noms
                            string nom = row["Nom_carte"].ToString();
                            string description = row["Description_carte"].ToString();
                            string attaque = row["Attaque_carte"].ToString();
                            string pv = row["Pv_carte"].ToString();
                            string prix = row["Prix_carte"].ToString();
                            string imagePath = row["Image"].ToString(); // Chemin de l'image

                            // Ajout d'une carte à l'interface
                            CarteContainer.Children.Add(CreerCarte(nom, description, attaque, pv, prix, imagePath));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Aucune carte trouvée dans la base de données.");
                    }
                }
                else
                {
                    MessageBox.Show("Erreur lors de la récupération des données.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des cartes : {ex.Message}");
            }
        }

        private Border CreerCarte(string nom, string description, string attaque, string pv, string prix, string imageFileName)
        {
            Border border = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                Width = 300,
                Background = Brushes.Black
            };

            StackPanel stackPanel = new StackPanel();

            // Nom de la carte
            TextBlock title = new TextBlock
            {
                Text = nom,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.PaleVioletRed,
                HorizontalAlignment = HorizontalAlignment.Center

                
            };

            // Image de la carte
            Image image = new Image
            {
                Width = 300,
                Height = 300,
                Margin = new Thickness(5),
                Stretch = Stretch.Uniform
            };

            // 🔹 Dossier des images
            string dossierImages = @"H:\UAA-13-projet\6T24_LudoBechet_ProjetUaa13\6T24_LudoBechet_ProjetUaa13\Asset\";

            // 🔹 Concaténer le chemin complet de l'image
            string imagePath = System.IO.Path.Combine(dossierImages, imageFileName);

            try
            {
                if (!string.IsNullOrEmpty(imageFileName) && System.IO.File.Exists(imagePath))
                {
                    image.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
                else
                {
                    // 🔹 Image par défaut si le fichier est manquant
                    image.Source = new BitmapImage(new Uri("Images/default.png", UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception)
            {
                image.Source = new BitmapImage(new Uri("Images/default.png", UriKind.RelativeOrAbsolute));
            }

            // Infos de la carte
            TextBlock stats = new TextBlock
            {
                Text = $"PV : {pv}\nPRIX : {prix}\nATTAQUE : {attaque}",
                Foreground = Brushes.White
            };

            // Description de la carte
            TextBlock desc = new TextBlock
            {
                Text = description,
                Foreground = Brushes.Yellow,
                TextWrapping = TextWrapping.Wrap
            };

            // Ajouter les éléments dans la carte
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(stats);
            stackPanel.Children.Add(desc);
            border.Child = stackPanel;

            return border;
        }


        // Gestion des clics sur les boutons de navigation
        //private void Carte1Boutton_Click(object sender, RoutedEventArgs e)
        // {
        //  Carte1.Content = new Carte();
        // }

        // private void Carte2Boutton_Click(object sender, RoutedEventArgs e)
        // {
        //     Carte1.Content = new Carte2();
        // }

        //  private void Carte3Boutton_Click(object sender, RoutedEventArgs e)
        //  {
        //      Carte1.Content = new Carte3();
        //   }

        // private void Carte4Boutton_Click(object sender, RoutedEventArgs e)
        // {
        //     Carte1.Content = new Carte4();
        // }

        // private void Carte5Boutton_Click(object sender, RoutedEventArgs e)
        // {
        //    Carte1.Content = new Carte5();
        //  }

        //  private void Carte6Boutton_Click(object sender, RoutedEventArgs e)
        //  {
        //      Carte1.Content = new Carte6();
        // }

        //  private void Carte7Boutton_Click(object sender, RoutedEventArgs e)
        //  {
        //      Carte1.Content = new Carte7();
        //  }

        //  private void Carte8Boutton_Click(object sender, RoutedEventArgs e)
        //  {
        //      Carte1.Content = new Carte8();
        //  }

        //  private void Carte9Boutton_Click(object sender, RoutedEventArgs e)
        // {
        //      Carte1.Content = new Carte9();
    }
}

