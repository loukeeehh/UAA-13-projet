using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic; // Pour Interaction.InputBox

namespace _6T24_LudoBechet_ProjetUaa13.Views
{
    public partial class Plateau : Page
    {
        // Gestion du jeu
        private DataSet pioche = new DataSet();
        private Random random = new Random();

        private int orJoueur1 = 20;
        private int orJoueur2 = 20;

        private int vieJoueur1 = 3;
        private int vieJoueur2 = 3;

        private List<string> cartesMortesJoueur1 = new List<string>();
        private List<string> cartesMortesJoueur2 = new List<string>();

        private Border carteSelectionnee = null;
        private DataRow nextCardRow = null;

        // Pour l'effet de la carte Bourgeois (réduction du prix de la prochaine carte)
        private bool discountJoueur1 = false;
        private bool discountJoueur2 = false;

        public Plateau()
        {
            InitializeComponent();
            ChargerPioche();
            MettreAJourAffichageOr();
            MettreAJourAffichageVies();
            GenererNextCardPreview();
        }

        #region Méthodes Générales


        private void GenererNextCardPreview()
        {
            if (!pioche.Tables.Contains("carte") || pioche.Tables["carte"].Rows.Count == 0)
            {
                nextCardRow = null;
                CartePiochée.Source = null;
                NextCardPriceTextBlock.Text = "Aucune carte disponible.";
                return;
            }

            int index = random.Next(pioche.Tables["carte"].Rows.Count);
            nextCardRow = pioche.Tables["carte"].Rows[index];

            int prixProchaineCarte = Convert.ToInt32(nextCardRow["Prix_carte"]);
            NextCardPriceTextBlock.Text = $"Prix de la prochaine carte : {prixProchaineCarte}";
        }

        private void ChargerPioche()
        {
            try
            {
                bdd maBdd = new bdd();
                pioche = maBdd.ObtenirCartes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la pioche : {ex.Message}");
            }
        }

        private void MettreAJourAffichageOr()
        {
            OrTextBlockJoueur1.Text = $"Joueur 1 : {orJoueur1}";
            OrTextBlockJoueur2.Text = $"Joueur 2 : {orJoueur2}";
        }

        private void MettreAJourAffichageVies()
        {
            VieTextBlockJoueur1.Text = $"Vies : {vieJoueur1}";
            VieTextBlockJoueur2.Text = $"Vies : {vieJoueur2}";
        }
        #endregion

        #region Gestion des Cartes

        private void PiocherCarte(bool isJoueur1)
        {
            Panel bankContainer = isJoueur1 ? CarteContainerJoueur1 : CarteContainerJoueur2;

            if (nextCardRow == null)
            {
                MessageBox.Show("La pioche est vide, mon Seigneur !");
                return;
            }

            if (bankContainer.Children.Count >= 3)
            {
                MessageBox.Show(isJoueur1
                    ? "Le banc de Joueur 1 est plein, mon Seigneur !"
                    : "Le banc de Joueur 2 est plein, mon Seigneur !");
                return;
            }

            DataRow row = nextCardRow;
            string imagePath = row["CheminImage"].ToString();
            string nomCarte = row["Nom_carte"].ToString();
            int idCarte = Convert.ToInt32(row["id_type"]); // 1: Attaque, 2: Défense
            int prixCarte = Convert.ToInt32(row["Prix_carte"]);
            int attaqueCarte = Convert.ToInt32(row["Attaque_carte"]);
            int pvCarte = Convert.ToInt32(row["PV_carte"]);
            int idAttitude = Convert.ToInt32(row["id_attitude"]);
            string attitudeType = row["attitude_type"].ToString();

            // Afficher la capacité spéciale via la classe SpecificiteCarte (classe séparée)
            if (idCarte == 2)
            {
                // Pour les cartes d'attaque spéciale (ex. Archer)
                SpecificiteCarte specCarte = new SpecificiteCarte(idAttitude, nomCarte);
                MessageBox.Show(specCarte.ExpliquerCapacite(), "Capacité de la carte spéciale");
            }
            if (idAttitude == 4)  // Carte Bourgeois qui réduit le coût de la prochaine carte
            {
                if (isJoueur1)
                    discountJoueur1 = true;
                else
                    discountJoueur2 = true;
            }

            int effectivePrixCarte = prixCarte;
            if (isJoueur1 && discountJoueur1)
            {
                effectivePrixCarte = prixCarte / 2;
                discountJoueur1 = false;
            }
            else if (!isJoueur1 && discountJoueur2)
            {
                effectivePrixCarte = prixCarte / 2;
                discountJoueur2 = false;
            }

            if (isJoueur1)
            {
                if (orJoueur1 < effectivePrixCarte)
                {
                    MessageBox.Show("Joueur 1 n'a pas assez d'or, mon Seigneur !");
                    return;
                }
                orJoueur1 -= effectivePrixCarte;
            }
            else
            {
                if (orJoueur2 < effectivePrixCarte)
                {
                    MessageBox.Show("Joueur 2 n'a pas assez d'or, mon Seigneur !");
                    return;
                }
                orJoueur2 -= effectivePrixCarte;
            }
            MettreAJourAffichageOr();

            CardStats statsCard = new CardStats
            {
                Id = idCarte,
                Nom = nomCarte,
                Attaque = attaqueCarte,
                PointsDeVie = pvCarte,
                Prix = effectivePrixCarte,
                CheminImage = imagePath,
                Owner = isJoueur1 ? 1 : 2,
                IdAttitude = idAttitude,
                AttitudeType = attitudeType
            };

            Border cardBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(2),
                Margin = new Thickness(2),
                Width = 145,
                Background = Brushes.White,
                Tag = statsCard
            };

            StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };

            Image image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 190,
                Height = 182,
                Stretch = Stretch.Uniform
            };

            TextBlock title = new TextBlock
            {
                Text = nomCarte,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock statsText = new TextBlock
            {
                Text = $"PV: {pvCarte} | ATQ: {attaqueCarte} | PRIX: {effectivePrixCarte}",
                FontSize = 12,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            sp.Children.Add(title);
            sp.Children.Add(image);
            sp.Children.Add(statsText);
            cardBorder.Child = sp;

            // Gestion de la sélection de la carte
            cardBorder.MouseDown += CartePiochee_Click;
            bankContainer.Children.Add(cardBorder);

            MessageTextBlock.Text = isJoueur1
                ? "Unité piochée pour Joueur 1, mon Seigneur !"
                : "Unité piochée pour Joueur 2, mon Seigneur !";

            pioche.Tables["carte"].Rows.Remove(row);
            GenererNextCardPreview();
        }

        private void PiocherCarteJoueur1_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(true);
        }

        private void PiocherCarteJoueur2_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(false);
        }
        #endregion

        #region Gestion des Interactions / Placements

        // Gère le clic sur une zone pour y déposer la carte sélectionnée.
        private void Zone_Click(object sender, MouseButtonEventArgs e)
        {
            if (carteSelectionnee == null)
            {
                MessageBox.Show("Sélectionnez d'abord une unité, mon Seigneur !");
                return;
            }

            if (!(sender is Border zoneBorder))
            {
                MessageBox.Show("Zone non reconnue !");
                return;
            }

            string zoneType = zoneBorder.Tag as string;
            if (string.IsNullOrEmpty(zoneType))
            {
                MessageBox.Show("La zone ne possède pas de type défini, vérifiez votre XAML !");
                return;
            }

            if (!(carteSelectionnee.Tag is CardStats stats))
            {
                MessageBox.Show("Impossible d'identifier l'unité sélectionnée !");
                return;
            }

            int zoneOwner = 0;
            if (zoneType.Contains("Joueur1"))
                zoneOwner = 1;
            else if (zoneType.Contains("Joueur2"))
                zoneOwner = 2;
            else
            {
                MessageBox.Show("Zone avec un propriétaire non défini !");
                return;
            }

            if (stats.Owner != zoneOwner)
            {
                MessageBox.Show("Vous ne pouvez pas placer une unité dans la zone adverse, mon Seigneur !");
                return;
            }

            // Vérifier que la carte va dans le bon type de zone.
            if (zoneType.Contains("attack") && stats.Id != 1)
            {
                MessageBox.Show("Une unité de défense ne peut être placée en zone d'attaque !");
                return;
            }
            if (zoneType.Contains("defense") && stats.Id != 2)
            {
                MessageBox.Show("Une unité d'attaque ne peut être placée en zone de défense !");
                return;
            }

            if (!(zoneBorder.Child is StackPanel zone) || zone.Children.Count >= 3)
            {
                MessageBox.Show("Cette zone est déjà pleine ou mal définie !");
                return;
            }

            if (carteSelectionnee.Parent is Panel ancienPanel)
                ancienPanel.Children.Remove(carteSelectionnee);

            carteSelectionnee.BorderBrush = Brushes.Black;
            zone.Children.Add(carteSelectionnee);
            carteSelectionnee = null;
            MessageTextBlock.Text = "Unité placée, mon Seigneur !";
            e.Handled = true;
        }

        // Gère la sélection d'une carte depuis le banc.
        private void CartePiochee_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card)
            {
                carteSelectionnee?.SetValue(Border.BorderBrushProperty, Brushes.Black);
                carteSelectionnee = card;
                carteSelectionnee.BorderBrush = Brushes.Red;
                MessageTextBlock.Text = "Unité sélectionnée, mon Seigneur !";
                e.Handled = true;
            }
        }

        // Affiche le récapitulatif des cartes mortes.
        private void AfficherCartesMortes_Click(object sender, RoutedEventArgs e)
        {
            string messageJ1 = (cartesMortesJoueur1.Count == 0)
                ? "Aucune carte morte pour Joueur 1."
                : string.Join("\n", cartesMortesJoueur1);
            string messageJ2 = (cartesMortesJoueur2.Count == 0)
                ? "Aucune carte morte pour Joueur 2."
                : string.Join("\n", cartesMortesJoueur2);
            MessageBox.Show($"Cartes mortes Joueur 1 :\n{messageJ1}\n\nCartes mortes Joueur 2 :\n{messageJ2}", "Unités éliminées");
        }

        private void AbandonnerJoueur1_Click(object sender, RoutedEventArgs e)
        {
            vieJoueur1--;
            if (vieJoueur1 > 0)
                MessageBox.Show($"Joueur 1 a abandonné, il lui reste {vieJoueur1} vie(s).\nJoueur 2 remporte la manche.");
            else
                MessageBox.Show("Joueur 1 a abandonné et n'a plus de vie, game over !");
            MettreAJourAffichageVies();
            ResetGame();
        }

        private void AbandonnerJoueur2_Click(object sender, RoutedEventArgs e)
        {
            vieJoueur2--;
            if (vieJoueur2 > 0)
                MessageBox.Show($"Joueur 2 a abandonné, il lui reste {vieJoueur2} vie(s).\nJoueur 1 remporte la manche.");
            else
                MessageBox.Show("Joueur 2 a abandonné et n'a plus de vie, game over !");
            MettreAJourAffichageVies();
            ResetGame();
        }

        private void ResetGame()
        {
            orJoueur1 = 20;
            orJoueur2 = 20;
            MettreAJourAffichageOr();

            CarteContainerJoueur1.Children.Clear();
            CarteContainerJoueur2.Children.Clear();
            StackZoneJoueur1Attack.Children.Clear();
            StackZoneJoueur1Defense.Children.Clear();
            StackZoneJoueur2Attack.Children.Clear();
            StackZoneJoueur2Defense.Children.Clear();

            cartesMortesJoueur1.Clear();
            cartesMortesJoueur2.Clear();

            MessageTextBlock.Text = "La manche est réinitialisée, mon Seigneur !";

            ChargerPioche();
            GenererNextCardPreview();
        }
        #endregion

        #region Résolution des Combats

        // Résout les combats lors de la fin de manche.
        private void FinDeManche_Click(object sender, RoutedEventArgs e)
        {
            // Traitement spécifique des archers (IdAttitude == 2)
            List<Border> attackArchersJ1 = StackZoneJoueur1Attack.Children.OfType<Border>()
                .Where(b => (b.Tag as CardStats)?.IdAttitude == 2).ToList();
            List<Border> enemyCardsForArchersJ1 = new List<Border>();
            enemyCardsForArchersJ1.AddRange(StackZoneJoueur2Attack.Children.OfType<Border>());
            enemyCardsForArchersJ1.AddRange(StackZoneJoueur2Defense.Children.OfType<Border>());
            foreach (var archer in attackArchersJ1)
            {
                if (enemyCardsForArchersJ1.Count > 0)
                {
                    int chosenIndex = ChoisirCibleAttaque(enemyCardsForArchersJ1);
                    Border target = enemyCardsForArchersJ1[chosenIndex];
                    CardStats archerStats = (CardStats)archer.Tag;
                    CardStats targetStats = (CardStats)target.Tag;
                    int effectiveAttack = archerStats.Attaque;
                    targetStats.PointsDeVie -= effectiveAttack;
                    MessageBox.Show($"{archerStats.Nom} (Archer) a ciblé {targetStats.Nom} pour {effectiveAttack} dégâts.", "Attaque Archer");
                    if (targetStats.PointsDeVie <= 0)
                    {
                        if (StackZoneJoueur2Attack.Children.Contains(target))
                            StackZoneJoueur2Attack.Children.Remove(target);
                        else if (StackZoneJoueur2Defense.Children.Contains(target))
                            StackZoneJoueur2Defense.Children.Remove(target);
                        orJoueur1 += 2;
                        enemyCardsForArchersJ1.RemoveAt(chosenIndex);
                    }
                }
            }

            // Traitement similaire pour les archers de Joueur 2
            List<Border> attackArchersJ2 = StackZoneJoueur2Attack.Children.OfType<Border>()
                .Where(b => (b.Tag as CardStats)?.IdAttitude == 2).ToList();
            List<Border> enemyCardsForArchersJ2 = new List<Border>();
            enemyCardsForArchersJ2.AddRange(StackZoneJoueur1Attack.Children.OfType<Border>());
            enemyCardsForArchersJ2.AddRange(StackZoneJoueur1Defense.Children.OfType<Border>());
            foreach (var archer in attackArchersJ2)
            {
                if (enemyCardsForArchersJ2.Count > 0)
                {
                    int chosenIndex = ChoisirCibleAttaque(enemyCardsForArchersJ2);
                    Border target = enemyCardsForArchersJ2[chosenIndex];
                    CardStats archerStats = (CardStats)archer.Tag;
                    CardStats targetStats = (CardStats)target.Tag;
                    int effectiveAttack = archerStats.Attaque;
                    targetStats.PointsDeVie -= effectiveAttack;
                    MessageBox.Show($"{archerStats.Nom} (Archer) a ciblé {targetStats.Nom} pour {effectiveAttack} dégâts.", "Attaque Archer");
                    if (targetStats.PointsDeVie <= 0)
                    {
                        if (StackZoneJoueur1Attack.Children.Contains(target))
                            StackZoneJoueur1Attack.Children.Remove(target);
                        else if (StackZoneJoueur1Defense.Children.Contains(target))
                            StackZoneJoueur1Defense.Children.Remove(target);
                        orJoueur2 += 2;
                        enemyCardsForArchersJ2.RemoveAt(chosenIndex);
                    }
                }
            }

            // Traitement des duels directs pour le reste des cartes (non archers)
            List<Border> remainingAttackJ1 = StackZoneJoueur1Attack.Children.OfType<Border>()
                .Where(b => (b.Tag as CardStats)?.IdAttitude != 2).ToList();
            List<Border> remainingAttackJ2 = StackZoneJoueur2Attack.Children.OfType<Border>()
                .Where(b => (b.Tag as CardStats)?.IdAttitude != 2).ToList();
            int nbDuels = Math.Min(remainingAttackJ1.Count, remainingAttackJ2.Count);
            int totalDamageJ1 = 0;
            int totalDamageJ2 = 0;
            for (int i = 0; i < nbDuels; i++)
            {
                CardStats statsJ1 = (CardStats)remainingAttackJ1[i].Tag;
                CardStats statsJ2 = (CardStats)remainingAttackJ2[i].Tag;
                int effectiveAttackJ1 = statsJ1.Attaque + (statsJ1.IdAttitude == 1 ? 2 : 0);
                int effectiveAttackJ2 = statsJ2.Attaque + (statsJ2.IdAttitude == 1 ? 2 : 0);

                statsJ1.PointsDeVie -= effectiveAttackJ2;
                statsJ2.PointsDeVie -= effectiveAttackJ1;
                totalDamageJ1 += effectiveAttackJ1;
                totalDamageJ2 += effectiveAttackJ2;

                if (statsJ1.PointsDeVie <= 0)
                {
                    if (!cartesMortesJoueur1.Contains(statsJ1.Nom))
                        cartesMortesJoueur1.Add(statsJ1.Nom);
                    orJoueur2 += 2;
                }
                if (statsJ2.PointsDeVie <= 0)
                {
                    if (!cartesMortesJoueur2.Contains(statsJ2.Nom))
                        cartesMortesJoueur2.Add(statsJ2.Nom);
                    orJoueur1 += 2;
                }
            }

            if (remainingAttackJ1.Count > nbDuels)
            {
                for (int i = nbDuels; i < remainingAttackJ1.Count; i++)
                {
                    CardStats statsJ1 = (CardStats)remainingAttackJ1[i].Tag;
                    totalDamageJ1 += statsJ1.Attaque;
                }
            }
            if (remainingAttackJ2.Count > nbDuels)
            {
                for (int i = nbDuels; i < remainingAttackJ2.Count; i++)
                {
                    CardStats statsJ2 = (CardStats)remainingAttackJ2[i].Tag;
                    totalDamageJ2 += statsJ2.Attaque;
                }
            }

            MessageTextBlock.Text = $"Joueur 1 a infligé {totalDamageJ1} dégâts.\n" +
                                    $"Joueur 2 a infligé {totalDamageJ2} dégâts.";

            // Retirer les cartes mortes des zones d'attaque
            RetirerCartesMortes(StackZoneJoueur1Attack.Children.OfType<Border>().ToList(), StackZoneJoueur1Attack);
            RetirerCartesMortes(StackZoneJoueur2Attack.Children.OfType<Border>().ToList(), StackZoneJoueur2Attack);

            if (cartesMortesJoueur1.Count >= 6 || orJoueur1 <= 0)
            {
                vieJoueur1--;
                if (vieJoueur1 > 0)
                    MessageBox.Show($"Joueur 1 a perdu la manche et perd une vie. Il lui reste {vieJoueur1} vie(s).");
                else
                    MessageBox.Show("Joueur 1 n'a plus de vie, game over !");
                orJoueur1 = 20;
                cartesMortesJoueur1.Clear();
            }
            if (cartesMortesJoueur2.Count >= 6 || orJoueur2 <= 0)
            {
                vieJoueur2--;
                if (vieJoueur2 > 0)
                    MessageBox.Show($"Joueur 2 a perdu la manche et perd une vie. Il lui reste {vieJoueur2} vie(s).");
                else
                    MessageBox.Show("Joueur 2 n'a plus de vie, game over !");
                orJoueur2 = 20;
                cartesMortesJoueur2.Clear();
            }

            MettreAJourAffichageOr();
            MettreAJourAffichageVies();
        }

        private void RetirerCartesMortes(List<Border> cards, Panel zone)
        {
            foreach (var card in cards.ToList())
            {
                if (card.Tag is CardStats stats && stats.PointsDeVie <= 0)
                    zone.Children.Remove(card);
            }
        }
        #endregion

        #region Méthode de Ciblage

        // Priorise automatiquement une cible tank (IdAttitude == 2), sinon propose une InputBox pour choisir.
        private int ChoisirCibleAttaque(List<Border> enemyCards)
        {
            for (int i = 0; i < enemyCards.Count; i++)
            {
                if (enemyCards[i].Tag is CardStats cs && cs.IdAttitude == 2)
                    return i;
            }

            StringBuilder sb = new StringBuilder("Choisissez la cible à attaquer :\n");
            for (int i = 0; i < enemyCards.Count; i++)
            {
                if (enemyCards[i].Tag is CardStats targetStats)
                {
                    sb.AppendLine($"{i}: {targetStats.Nom} (PV: {targetStats.PointsDeVie}, ATQ: {targetStats.Attaque})");
                }
            }
            string input = Interaction.InputBox(sb.ToString(), "Cible de l'attaque", "0");
            if (!int.TryParse(input, out int choix) || choix < 0 || choix >= enemyCards.Count)
            {
                MessageBox.Show("Indice invalide. La première cible sera sélectionnée par défaut.", "Indice invalide");
                choix = 0;
            }
            return choix;
        }
        #endregion
    }


    public class CardStats
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int Attaque { get; set; }
        public int PointsDeVie { get; set; }
        public int Prix { get; set; }
        public string CheminImage { get; set; }
        public int Owner { get; set; }
        public int IdAttitude { get; set; }
        public string AttitudeType { get; set; }
    }
}
