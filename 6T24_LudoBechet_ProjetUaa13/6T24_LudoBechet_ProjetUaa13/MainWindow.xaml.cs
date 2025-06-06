﻿using _6T24_LudoBechet_ProjetUaa13.Views;
using System;
using System.Collections.Generic;
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




namespace _6T24_LudoBechet_ProjetUaa13
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MediaPlayer mediaPlayer;

        public MainWindow()   
        {
            InitializeComponent();

            // Initialisation du lecteur multimédia
            mediaPlayer = new MediaPlayer();

            // Charger le fichier audio
            mediaPlayer.Open(new Uri("H://UAA-13-projet\\6T24_LudoBechet_ProjetUaa13\\6T24_LudoBechet_ProjetUaa13\\Asset\\got.mp3", UriKind.RelativeOrAbsolute));

            // Lire la musique (pas utilisé)
            mediaPlayer.Play();
        }

        // Gestionnaire d'événements pour le bouton "Commencer"
        private void Commencer_Click(object sender, RoutedEventArgs e)
        {
            // Change le contenu principal de la fenêtre vers "Plateau"
            Main.Content = new Plateau();

            // Arrête la lecture du média en cours
            mediaPlayer.Stop();
        }

        // Gestionnaire d'événements pour le bouton "Carte"
        private void CarteButton_Click(object sender, RoutedEventArgs e)
        {
            // Change le contenu principal de la fenêtre vers "Carte"
            Main.Content = new Carte();
        }

        // Gestionnaire d'événements pour le bouton "Paramètre"
        private void ParametreButton_Click(object sender, RoutedEventArgs e)
        {
            // Change le contenu principal de la fenêtre vers "Paramètre"
            Main.Content = new Parametre();
        }

        // Gestionnaire d'événements pour le bouton "Règle"
        private void RegleButton_Click(object sender, RoutedEventArgs e)
        {
            // Change le contenu principal de la fenêtre vers "Règle"
            Main.Content = new Regle();
        }




    }
}