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
        }
        private void Carte1Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte();

        }
        private void Carte2Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte2();
        }
        private void Carte3Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte3();
        }
        private void Carte4Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte4();
        }
        private void Carte5Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte5();
        }
        private void Carte6Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte6();
        }
        private void Carte7Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte7();
        }
        private void Carte8Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte8();
        }
        private void Carte9Boutton_Click(object sender, RoutedEventArgs e)
        {
            Carte1.Content = new Carte9();
        }
    }
}
