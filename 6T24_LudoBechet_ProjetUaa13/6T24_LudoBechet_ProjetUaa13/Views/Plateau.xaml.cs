using System;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    public partial class Plateau : Page
    {
        private DataSet pioche = new DataSet();
        private Random random = new Random();

        public Plateau()
        {
            InitializeComponent();
            ChargerPioche(); // Charger les cartes au lancement de la page
        }

        private void ChargerPioche()
        {
            try
            {
                bdd maBdd = new bdd();
                pioche = maBdd.ObtenirCartes(); // Récupère les cartes depuis la BDD

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
                MessageBox.Show("La pioche est vide mon Seigneur !");
                return;
            }

            // Tirage au sort
            int index = random.Next(pioche.Tables["carte"].Rows.Count);
            string imagePath = pioche.Tables["carte"].Rows[index]["CheminImage"].ToString();

            // Afficher l'image dans l'interface
            CarteImage.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));

            MessageTextBlock.Text = "Unitée pioché mon seigneur !";

            // Supprimer la carte piochée du DataSet
            pioche.Tables["carte"].Rows.RemoveAt(index);
        }

        private void PiocherCarte_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte();
            
            
        }
    }
}
