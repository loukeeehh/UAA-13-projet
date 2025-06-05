using System;
using System.Collections.Generic;
using System.Data;
using System.IO; // Permet de gérer les fichiers et les chemins d'accès
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient; // Librairie pour se connecter à une base MySQL
using System.Diagnostics;
using _6T24_LudoBechet_ProjetUaa13;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    /// <summary>
    /// Logique d'interaction pour Carte.xaml
    /// </summary>
    public partial class Carte : Page
    {
        // Constructeur de la classe, initialisant la page et chargeant les données
        public Carte()
        {
            InitializeComponent();
            ChargerDonneesCarte(); // Chargement des données à l'ouverture de la page
        }

        // Fonction qui charge les données des cartes depuis la base de données
        private void ChargerDonneesCarte()
        {
            try
            {
                bdd maBdd = new bdd(); // Instance de l'objet base de données
                if (maBdd.ChercheCarte(out DataSet infos)) // Vérifie si des cartes existent
                {
                    // Nettoyer le conteneur des cartes dans l'interface graphique
                    CarteContainer.Children.Clear();

                    if (infos.Tables.Contains("carte") && infos.Tables["carte"].Rows.Count > 0)
                    {
                        foreach (DataRow row in infos.Tables["carte"].Rows)
                        {
                            // Récupération des informations de la carte depuis la base de données
                            string nom = row["Nom_carte"].ToString();
                            string description = row["Description_carte"].ToString();
                            string attaque = row["Attaque_carte"].ToString();
                            string pv = row["PV_carte"].ToString();
                            string prix = row["Prix_carte"].ToString();
                            string idAttitude = row["id_attitude"].ToString();
                            string typeAttitude = row["Attitude_type"].ToString();
                            string imagePath = row["Image"].ToString(); // Chemin de l'image

                            // Ajout de la carte à l'interface utilisateur
                            CarteContainer.Children.Add(
                                CreerCarte(nom, description, attaque, pv, prix, imagePath, typeAttitude)
                            );
                        }
                    }
                    else
                    {
                        MessageBox.Show("Aucune carte trouvée.");// message si aucune carte
                    }
                }
                else
                {
                    MessageBox.Show("Erreur de récupération des données.");//message si bug lors de la recup des donné 
                }
            }
            catch (Exception ex)
            {
                // Gérer et afficher les erreurs éventuelles
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        // Fonction qui crée un élément graphique représentant une carte
        private Border CreerCarte(string nom, string description, string attaque, string pv, string prix, string imageFileName, string typeAttitude)
        {
            Border border = new Border
            {
                BorderBrush = Brushes.White, // Bordure blanche
                BorderThickness = new Thickness(2), // Épaisseur de bordure
                Padding = new Thickness(10), // Espacement interne
                Margin = new Thickness(5), // Marges externes
                Width = 300, // Largeur fixe
                Background = Brushes.Black // Fond noir
            };

            StackPanel stackPanel = new StackPanel(); // Conteneur vertical pour les éléments

            // Affichage du nom de la carte
            TextBlock title = new TextBlock
            {
                Text = nom,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.PaleVioletRed, // Couleur du texte
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Image de la carte
            Image image = new Image
            {
                Width = 300,
                Height = 300,
                Margin = new Thickness(5),
                Stretch = Stretch.Uniform // Assure une mise à l'échelle uniforme
            };

            // Déterminer le chemin de l'image en fonction du dossier "Asset"
            string dossierImages = Path.Combine(Environment.CurrentDirectory, "Asset");
            string cheminComplet = Path.Combine(dossierImages, imageFileName);

            try
            {
                if (!string.IsNullOrEmpty(imageFileName) && File.Exists(cheminComplet))
                {
                    image.Source = new BitmapImage(new Uri(cheminComplet, UriKind.Absolute)); // Charger l'image
                }
                else
                {
                    // Chargement d'une image par défaut si aucune image n'est trouvée (ne fonctionne pas)
                    image.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/logo.png", UriKind.Absolute));
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, charger une image par défaut (ne fonctionne pas )
                image.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/logo.png", UriKind.Absolute));
            }

            // Affichage des statistiques de la carte
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
                TextWrapping = TextWrapping.Wrap // Permet de gérer les textes longs
            };

            // Attribut de la carte
            TextBlock attributs = new TextBlock
            {
                Text = $"★ Attribut : {typeAttitude}",
                Foreground = Brushes.LightGreen,
                TextWrapping = TextWrapping.Wrap
            };

            // Ajout des éléments à l'affichage de la carte
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(stats);
            stackPanel.Children.Add(desc);
            stackPanel.Children.Add(attributs);
            border.Child = stackPanel;

            return border;
        }
    }
}
