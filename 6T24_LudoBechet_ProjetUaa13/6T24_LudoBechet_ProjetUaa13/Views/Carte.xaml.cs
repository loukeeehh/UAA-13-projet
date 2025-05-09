﻿using System;
using System.Collections.Generic;
using System.Data; // Import unique
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
                bdd maBdd = new bdd();
                if (maBdd.ChercheCarte(out DataSet infos))
                {
                    // Assurez-vous que l'élément CarteContainer existe dans le XAML
                    CarteContainer.Children.Clear();

                    if (infos.Tables.Contains("carte") && infos.Tables["carte"].Rows.Count > 0)
                    {
                        foreach (DataRow row in infos.Tables["carte"].Rows)
                        {
                            string nom = row["Nom_carte"].ToString();
                            string description = row["Description_carte"].ToString();
                            string attaque = row["Attaque_carte"].ToString();
                            string pv = row["PV_carte"].ToString();
                            string prix = row["Prix_carte"].ToString();
                            string idAttitude = row["id_attitude"].ToString();
                            string typeAttitude = row["Attitude_type"].ToString();
                            string imagePath = row["Image"].ToString(); // Utilise la colonne "Image"

                            // Ajouter la carte à l'interface
                            CarteContainer.Children.Add(
                                CreerCarte(nom, description, attaque, pv, prix, imagePath, typeAttitude)
                            );
                        }
                    }
                    else
                    {
                        MessageBox.Show("Aucune carte trouvée.");
                    }
                }
                else
                {
                    MessageBox.Show("Erreur de récupération des données.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        private Border CreerCarte(string nom, string description, string attaque, string pv, string prix, string imageFileName, string typeAttitude)
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

            // Dossier contenant les images
            string dossierImages = @"H:\UAA-13-projet\6T24_LudoBechet_ProjetUaa13\6T24_LudoBechet_ProjetUaa13\Asset\";

            // Concaténation du chemin complet de l'image
            string cheminComplet = System.IO.Path.Combine(dossierImages, imageFileName);

            try
            {
                if (!string.IsNullOrEmpty(imageFileName) && System.IO.File.Exists(cheminComplet))
                {
                    image.Source = new BitmapImage(new Uri(cheminComplet, UriKind.Absolute));
                }
            }
            catch (Exception)
            {
                // Si une erreur survient, on affiche une image par défaut
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

            TextBlock attributs = new TextBlock
            {
                Text = $"★ Attribut : {typeAttitude}",
                Foreground = Brushes.LightGreen,
                TextWrapping = TextWrapping.Wrap,
            };

            // Assemblage des éléments de la carte
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
