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
    /// Logique d'interaction pour Plateau.xaml
    /// </summary>
    public partial class Plateau : Page
    {
        Button[,] btn = new Button[7, 3];

        public Plateau()
        {
            InitializeComponent();
            Grill();
        }
        public void Grill ()
        {
            this.Width = 1000;
            this.Height = 1000;

            ColumnDefinition[] colDef = new ColumnDefinition[btn.GetLength(1)];
            RowDefinition[] rowDef = new RowDefinition[btn.GetLength(0)];

            for(int i = 0; i < btn.GetLength(1); i++)
            {
                colDef[i] = new ColumnDefinition();
            }
            for(int i = 0; i < btn.GetLength(i); i++)
            {
                grdMain.ColumnDefinitions.Add(colDef[i]);
            }
            for(int j = 0; j < btn.GetLength(0); j++)
            {
                rowDef[j] = new RowDefinition();    
            }
            for (int j = 0; j < btn.GetLength(0); j++)
            {
                grdMain.RowDefinitions.Add(rowDef[j]);
            }
            
        }
    }
}
