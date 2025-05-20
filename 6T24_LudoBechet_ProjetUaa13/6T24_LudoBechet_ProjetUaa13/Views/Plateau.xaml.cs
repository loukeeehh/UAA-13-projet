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

        // Ce log accumule les événements (capacités activées) durant la manche
        private List<string> roundSummaryLog = new List<string>();

        private Border carteSelectionnee = null;
        private DataRow nextCardRow = null;

        // Flags pour les effets spéciaux (absorption, réduction de coût, etc.)
        private bool discountJoueur1 = false;
        private bool discountJoueur2 = false;
        private bool absorptionFlagJ1 = false;
        private bool absorptionFlagJ2 = false;

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

        #region Decrementer PV Defense (Optionnel)
        // Décrémente 1 PV pour chaque carte en défense et retire celles dont le PV tombe à 0.
        private void DecrementerPVDefense()
        {
            foreach (Border card in StackZoneJoueur1Defense.Children.OfType<Border>().ToList())
            {
                if (card.Tag is CardStats stats)
                {
                    stats.PointsDeVie -= 1;
                    if (stats.PointsDeVie <= 0)
                    {
                        cartesMortesJoueur1.Add(stats.Nom);
                        StackZoneJoueur1Defense.Children.Remove(card);
                    }
                }
            }
            foreach (Border card in StackZoneJoueur2Defense.Children.OfType<Border>().ToList())
            {
                if (card.Tag is CardStats stats)
                {
                    stats.PointsDeVie -= 1;
                    if (stats.PointsDeVie <= 0)
                    {
                        cartesMortesJoueur2.Add(stats.Nom);
                        StackZoneJoueur2Defense.Children.Remove(card);
                    }
                }
            }
        }
        #endregion

        #region Gestion des Cartes

        // Permet de piocher une carte. Le paramètre gratuit permet de piocher sans déduction d'or (pour Drakar par ex.)
        private void PiocherCarte(bool isJoueur, bool gratuit = false)
        {
            Panel bankContainer = isJoueur ? CarteContainerJoueur1 : CarteContainerJoueur2;

            if (nextCardRow == null)
            {
                MessageBox.Show("La pioche est vide, mon Seigneur !");
                return;
            }

            if (bankContainer.Children.Count >= 3)
            {
                MessageBox.Show(isJoueur
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

            // Pour les cartes d'archer, si l'attaque est nulle, on lui attribue une valeur par défaut (ex: 5)
            if ((idAttitude == 2 || idAttitude == 5 || idAttitude == 6 || idAttitude == 11) && attaqueCarte <= 0)
            {
                attaqueCarte = 5;
            }

            // MessageBox pour une carte spéciale (id_attitude != 1) sauf si c'est une carte de défense (idCarte == 2)
            if (idAttitude != 1 && idCarte != 2)
            {
                SpecificiteCarte specTemp = new SpecificiteCarte(idAttitude, nomCarte);
                MessageBox.Show($"Vous avez pioché une carte spéciale : {nomCarte}\nUtilité : {specTemp.ExpliquerCapacite()}",
                    "Carte Spéciale piochée");
            }

            SpecificiteCarte specCarte = new SpecificiteCarte(idAttitude, nomCarte);

            CardStats statsCard = new CardStats
            {
                Id = idCarte,
                Nom = nomCarte,
                Attaque = attaqueCarte,
                PointsDeVie = pvCarte,
                Prix = prixCarte,
                CheminImage = imagePath,
                Owner = isJoueur ? 1 : 2,
                IdAttitude = idAttitude,
                AttitudeType = attitudeType
            };

            if (idAttitude == 4)
            {
                if (isJoueur)
                    discountJoueur1 = true;
                else
                    discountJoueur2 = true;
            }

            int effectivePrixCarte = prixCarte;
            if (!gratuit)
            {
                if (isJoueur && discountJoueur1)
                {
                    effectivePrixCarte = prixCarte / 2;
                    discountJoueur1 = false;
                }
                else if (!isJoueur && discountJoueur2)
                {
                    effectivePrixCarte = prixCarte / 2;
                    discountJoueur2 = false;
                }
                if (isJoueur)
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
            }
            else
            {
                effectivePrixCarte = 0;
            }

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

            cardBorder.MouseDown += CartePiochee_Click;
            bankContainer.Children.Add(cardBorder);

            MessageTextBlock.Text = isJoueur
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

        private void CartePiochee_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card)
            {
                if (carteSelectionnee != null)
                    carteSelectionnee.BorderBrush = Brushes.Black;
                carteSelectionnee = card;
                carteSelectionnee.BorderBrush = Brushes.Red;
                MessageTextBlock.Text = "Unité sélectionnée, mon Seigneur !";
                e.Handled = true;
            }
        }

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

            if (!(zoneBorder.Child is StackPanel zoneStack) || zoneStack.Children.Count >= 3)
            {
                MessageBox.Show("Cette zone est déjà pleine ou mal définie !");
                return;
            }

            if (carteSelectionnee.Parent is Panel ancienPanel)
                ancienPanel.Children.Remove(carteSelectionnee);

            zoneStack.Children.Add(carteSelectionnee);

            if (zoneType.Contains("defense"))
            {
                if (stats.IdAttitude == 8)
                {
                    Panel alliedContainer = stats.Owner == 1 ? CarteContainerJoueur1 : CarteContainerJoueur2;
                    if (alliedContainer.Children.Count > 0)
                    {
                        Border cible = alliedContainer.Children.OfType<Border>().FirstOrDefault();
                        if (cible?.Tag is CardStats cibleStats)
                        {
                            cibleStats.PointsDeVie += 5;
                            MessageBox.Show($"Capacité activée : {stats.Nom} soigne {cibleStats.Nom} de 5 PV!", "Capacité de soin");
                            roundSummaryLog.Add($"{stats.Nom} a soigné {cibleStats.Nom} de 5 PV.");
                        }
                    }
                }
                else if (stats.IdAttitude != 1)
                {
                    roundSummaryLog.Add(new SpecificiteCarte(stats.IdAttitude, stats.Nom).ExpliquerCapacite());
                    AppliquerCapaciteDefense(stats.Owner == 1, stats);
                }
            }

            carteSelectionnee.BorderBrush = Brushes.Black;
            carteSelectionnee = null;
            MessageTextBlock.Text = "Unité placée, mon Seigneur !";
            e.Handled = true;
        }

        private void AfficherCartesMortes_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Cartes mortes Joueur 1 :");
            if (cartesMortesJoueur1.Count == 0)
                sb.AppendLine("Aucune carte morte.");
            else
                foreach (var nom in cartesMortesJoueur1)
                    sb.AppendLine($"- {nom}");
            sb.AppendLine("\nCartes mortes Joueur 2 :");
            if (cartesMortesJoueur2.Count == 0)
                sb.AppendLine("Aucune carte morte.");
            else
                foreach (var nom in cartesMortesJoueur2)
                    sb.AppendLine($"- {nom}");
            MessageBox.Show(sb.ToString(), "Cartes Mortes");
        }
        #endregion

        #region Nettoyage des Cartes Mortes

        private void NettoyerCartesMortesDansZone(Panel zone)
        {
            List<Border> elementsARetirer = zone.Children.OfType<Border>()
                .Where(b => (b.Tag as CardStats)?.PointsDeVie <= 0).ToList();

            foreach (var element in elementsARetirer)
            {
                zone.Children.Remove(element);
            }
        }

        private void NettoyerZones()
        {
            NettoyerCartesMortesDansZone(StackZoneJoueur1Attack);
            NettoyerCartesMortesDansZone(StackZoneJoueur1Defense);
            NettoyerCartesMortesDansZone(StackZoneJoueur2Attack);
            NettoyerCartesMortesDansZone(StackZoneJoueur2Defense);
        }
        #endregion

        #region Abandon et Réinitialisation

        private void AbandonnerJoueur1_Click(object sender, RoutedEventArgs e)
        {
            vieJoueur1--;
            if (vieJoueur1 > 0)
                MessageBox.Show($"Joueur 1 a abandonné, il lui reste {vieJoueur1} vie(s).\nJoueur 2 remporte la manche.");
            else
                MessageBox.Show("Joueur 1 n'a plus de vie, game over!");
            MettreAJourAffichageVies();
            ResetGame();
        }

        private void AbandonnerJoueur2_Click(object sender, RoutedEventArgs e)
        {
            vieJoueur2--;
            if (vieJoueur2 > 0)
                MessageBox.Show($"Joueur 2 a abandonné, il lui reste {vieJoueur2} vie(s).\nJoueur 1 remporte la manche.");
            else
                MessageBox.Show("Joueur 2 n'a plus de vie, game over!");
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
            roundSummaryLog.Clear();

            MessageTextBlock.Text = "La manche est réinitialisée, mon Seigneur !";
            ChargerPioche();
            GenererNextCardPreview();
        }
        #endregion

        #region Résolution des Combats et Synthèse de Manche

        private void FinDeManche_Click(object sender, RoutedEventArgs e)
        {
            // Optionnel: DecrementerPVDefense(); - décrémente 1 PV sur chaque carte de défense par manche.

            // Traitement pour les archers du Joueur 1
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
                    roundSummaryLog.Add($"{archerStats.Nom} (Archer) attaque {targetStats.Nom} pour {effectiveAttack} dégâts.");
                    if (targetStats.PointsDeVie <= 0)
                    {
                        if (StackZoneJoueur2Attack.Children.Contains(target))
                            StackZoneJoueur2Attack.Children.Remove(target);
                        else if (StackZoneJoueur2Defense.Children.Contains(target))
                            StackZoneJoueur2Defense.Children.Remove(target);
                        orJoueur1 += 2;
                        roundSummaryLog.Add($"{targetStats.Nom} est morte. +2 or pour Joueur 1.");
                        cartesMortesJoueur2.Add(targetStats.Nom);
                        enemyCardsForArchersJ1.RemoveAt(chosenIndex);
                    }
                }
            }

            // Traitement pour les archers du Joueur 2
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
                    roundSummaryLog.Add($"{archerStats.Nom} (Archer) attaque {targetStats.Nom} pour {effectiveAttack} dégâts.");
                    if (targetStats.PointsDeVie <= 0)
                    {
                        if (StackZoneJoueur1Attack.Children.Contains(target))
                            StackZoneJoueur1Attack.Children.Remove(target);
                        else if (StackZoneJoueur1Defense.Children.Contains(target))
                            StackZoneJoueur1Defense.Children.Remove(target);
                        orJoueur2 += 2;
                        roundSummaryLog.Add($"{targetStats.Nom} est morte. +2 or pour Joueur 2.");
                        cartesMortesJoueur1.Add(targetStats.Nom);
                        enemyCardsForArchersJ2.RemoveAt(chosenIndex);
                    }
                }
            }

            // Duels directs entre cartes restantes (non archers)
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
                int effectiveAttackJ1 = statsJ1.Attaque + ((statsJ1.IdAttitude == 1) ? 2 : 0);
                int effectiveAttackJ2 = statsJ2.Attaque + ((statsJ2.IdAttitude == 1) ? 2 : 0);

                statsJ1.PointsDeVie -= effectiveAttackJ2;
                statsJ2.PointsDeVie -= effectiveAttackJ1;
                totalDamageJ1 += effectiveAttackJ1;
                totalDamageJ2 += effectiveAttackJ2;

                roundSummaryLog.Add($"{statsJ1.Nom} inflige {effectiveAttackJ1} dégâts à {statsJ2.Nom} et réciproquement {effectiveAttackJ2} dégâts.");

                if (statsJ1.PointsDeVie <= 0)
                {
                    if (!cartesMortesJoueur1.Contains(statsJ1.Nom))
                        cartesMortesJoueur1.Add(statsJ1.Nom);
                    orJoueur2 += 2;
                    roundSummaryLog.Add($"{statsJ1.Nom} est morte. +2 or pour Joueur 2.");
                }
                if (statsJ2.PointsDeVie <= 0)
                {
                    if (!cartesMortesJoueur2.Contains(statsJ2.Nom))
                        cartesMortesJoueur2.Add(statsJ2.Nom);
                    orJoueur1 += 2;
                    roundSummaryLog.Add($"{statsJ2.Nom} est morte. +2 or pour Joueur 1.");
                }
            }

            // Attaques additionnelles pour les cartes restantes qui n'ont pas eu de duel direct.
            if (remainingAttackJ1.Count > nbDuels)
            {
                for (int i = nbDuels; i < remainingAttackJ1.Count; i++)
                {
                    CardStats statsJ1 = (CardStats)remainingAttackJ1[i].Tag;
                    totalDamageJ1 += statsJ1.Attaque;
                    roundSummaryLog.Add($"{statsJ1.Nom} attaque pour {statsJ1.Attaque} dégâts (aucun duel direct).");
                }
            }
            if (remainingAttackJ2.Count > nbDuels)
            {
                for (int i = nbDuels; i < remainingAttackJ2.Count; i++)
                {
                    CardStats statsJ2 = (CardStats)remainingAttackJ2[i].Tag;
                    totalDamageJ2 += statsJ2.Attaque;
                    roundSummaryLog.Add($"{statsJ2.Nom} attaque pour {statsJ2.Attaque} dégâts (aucun duel direct).");
                }
            }

            // Mise à jour de l'or affiché.
            MettreAJourAffichageOr();

            // Nettoyage: retirer les cartes dont les PV sont <= 0.
            NettoyerZones();

            // Création de la synthèse finale de la manche.
            StringBuilder summary = new StringBuilder();
            summary.AppendLine("Résultat de la Manche :");
            summary.AppendLine("-------------------------------------------------");
            summary.AppendLine("Cartes mortes Joueur 1 : " +
                (cartesMortesJoueur1.Count == 0 ? "Aucune" : string.Join(", ", cartesMortesJoueur1)));
            summary.AppendLine("Cartes mortes Joueur 2 : " +
                (cartesMortesJoueur2.Count == 0 ? "Aucune" : string.Join(", ", cartesMortesJoueur2)));
            summary.AppendLine();
            summary.AppendLine("Or final Joueur 1 : " + orJoueur1);
            summary.AppendLine("Or final Joueur 2 : " + orJoueur2);
            summary.AppendLine();
            summary.AppendLine("Capacités activées : " +
                (roundSummaryLog.Count == 0 ? "Aucune" : string.Join(" | ", roundSummaryLog)));

            bool resetManche = false;
            if ((cartesMortesJoueur1.Count >= 6 || orJoueur1 <= 0) ||
                (cartesMortesJoueur2.Count >= 6 || orJoueur2 <= 0))
            {
                if (cartesMortesJoueur1.Count >= 6 || orJoueur1 <= 0)
                {
                    vieJoueur1--;
                    summary.AppendLine("\nJoueur 1 perd une vie.");
                    resetManche = true;
                }
                if (cartesMortesJoueur2.Count >= 6 || orJoueur2 <= 0)
                {
                    vieJoueur2--;
                    summary.AppendLine("\nJoueur 2 perd une vie.");
                    resetManche = true;
                }
            }

            MessageBox.Show(summary.ToString(), "Synthèse de la Manche");

            if (resetManche)
                ResetGame();
            // Sinon, le plateau reste dans son état actuel pour continuer le jeu.
        }

        private int ChoisirCibleAttaque(List<Border> enemyCards)
        {
            if (enemyCards != null && enemyCards.Count > 0)
                return 0;
            else
                return 0;
        }

        #endregion

        #region Activation des Capacités Spéciales (Défense)

        private void AppliquerCapaciteDefense(bool isJoueur, CardStats stats)
        {
            switch (stats.IdAttitude)
            {
                case 2:
                case 5:
                case 6:
                case 11:
                    break;
                case 3:
                    if (isJoueur)
                        absorptionFlagJ1 = true;
                    else
                        absorptionFlagJ2 = true;
                    break;
                case 4:
                    break;
                case 7:
                    if (isJoueur)
                    {
                        foreach (var card in StackZoneJoueur2Attack.Children.OfType<Border>()
                            .Concat(StackZoneJoueur2Defense.Children.OfType<Border>()))
                        {
                            if (card.Tag is CardStats enemyStats)
                                enemyStats.PointsDeVie -= stats.Attaque;
                        }
                    }
                    else
                    {
                        foreach (var card in StackZoneJoueur1Attack.Children.OfType<Border>()
                            .Concat(StackZoneJoueur1Defense.Children.OfType<Border>()))
                        {
                            if (card.Tag is CardStats enemyStats)
                                enemyStats.PointsDeVie -= stats.Attaque;
                        }
                    }
                    break;
                case 8:
                    // L'effet de soin est déclenché lors du placement.
                    break;
                case 9:
                    if (isJoueur)
                    {
                        PiocherCarte(true, true);  // Gratuit
                        PiocherCarte(true, true);  // Gratuit
                    }
                    else
                    {
                        PiocherCarte(false, true);
                        PiocherCarte(false, true);
                    }
                    break;
                case 10:
                    if (isJoueur)
                    {
                        if (cartesMortesJoueur1.Count > 0)
                        {
                            string lastCard = cartesMortesJoueur1.Last();
                            cartesMortesJoueur1.RemoveAt(cartesMortesJoueur1.Count - 1);
                            roundSummaryLog.Add($"La carte {lastCard} a été ressuscitée pour Joueur 1.");
                        }
                    }
                    else
                    {
                        if (cartesMortesJoueur2.Count > 0)
                        {
                            string lastCard = cartesMortesJoueur2.Last();
                            cartesMortesJoueur2.RemoveAt(cartesMortesJoueur2.Count - 1);
                            roundSummaryLog.Add($"La carte {lastCard} a été ressuscitée pour Joueur 2.");
                        }
                    }
                    break;
                case 12:
                    if (isJoueur)
                    {
                        foreach (var card in CarteContainerJoueur1.Children.OfType<Border>())
                        {
                            if (card.Tag is CardStats cStats)
                                cStats.PointsDeVie += 2;
                        }
                    }
                    else
                    {
                        foreach (var card in CarteContainerJoueur2.Children.OfType<Border>())
                        {
                            if (card.Tag is CardStats cStats)
                                cStats.PointsDeVie += 2;
                        }
                    }
                    break;
                default:
                    break;
            }
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
