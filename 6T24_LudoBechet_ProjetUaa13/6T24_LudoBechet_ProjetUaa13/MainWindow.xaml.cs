using _6T24_LudoBechet_ProjetUaa13.Views;
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
        public MainWindow()
        {
            InitializeComponent();
            



        }
        private void CarteButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new Carte();
        }
        private void ParametreButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new Parametre();
        }

        
    }
}
