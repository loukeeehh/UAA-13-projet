﻿using System;
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
using System.Windows.Shapes;

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    /// <summary>
    /// Logique d'interaction pour test.xaml
    /// </summary>
    public partial class test : Window
    {
        public test()
        {
            InitializeComponent();
            Button btnB = new Button();
            StackPanel stkBloc1 = new StackPanel();
            TextBlock txtBMonTexte = new TextBlock();
            CheckBox chkBoite = new CheckBox();
            TextBox txtDonnees = new TextBox();
            ComboBox cboNoms = new ComboBox();

        }

    }
}
