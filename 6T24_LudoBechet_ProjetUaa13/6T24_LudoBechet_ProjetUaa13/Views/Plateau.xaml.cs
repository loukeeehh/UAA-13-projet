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
    /// Logique d'interaction pour Plateau.xaml
    /// </summary>
    public partial class Plateau : Page
    {
        

        public Plateau()
        {
            InitializeComponent();
            ChargerDonneesCarte();


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
        private void PiocherCarte_Click(object sender, RoutedEventArgs e)
        {
            bdd maBdd = new bdd();
            if (maBdd.PiocherCarte(out DataRow carte))
            {
                // Récupérer le chemin d'image calculé dans la méthode ChercheCarte
                string imagePath = carte["Image"].ToString(); // Utiliser le chemin complet

                // Vérifier si le chemin de l'image est valide
                if (!string.IsNullOrEmpty(imagePath))
                {
                    try
                    {
                        // Si l'image existe à l'URL spécifiée, on la charge
                        Uri imageUri = new Uri(imagePath, UriKind.Absolute);

                        // Tenter de créer l'image à partir de l'URL
                        CartePiochée.Source = new BitmapImage(imageUri);
                    }
                    catch (Exception ex)
                    {
                        // En cas d'erreur lors du chargement de l'image
                        MessageBox.Show($"Erreur lors du chargement de l'image : {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Le chemin de l'image est vide.");
                }
            }
            else
            {
                MessageBox.Show("Erreur lors de la pioche.");
            }
        }







    }
}
