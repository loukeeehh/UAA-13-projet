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

        // Stockage complet des cartes mortes pour la résurrection.
        private List<CardStats> cartesMortesJoueur1 = new List<CardStats>();
        private List<CardStats> cartesMortesJoueur2 = new List<CardStats>();

        // Ce log accumule les événements (capacités activées) durant la manche.
        private List<string> roundSummaryLog = new List<string>();

        private Border carteSelectionnee = null;
        private DataRow nextCardRow = null;

        // Flags pour effets spéciaux
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

        #region Mise à jour et création des cartes

        // Mise à jour de l'affichage des stats (PV, ATQ, PRIX) d'une carte.
        private void UpdateCardDisplay(Border card)
        {
            if (card?.Child is StackPanel sp && sp.Children.Count >= 3 && sp.Children[2] is TextBlock statsText)
            {
                if (card.Tag is CardStats stats)
                {
                    statsText.Text = $"PV: {stats.PointsDeVie} | ATQ: {stats.Attaque} | PRIX: {stats.Prix}";
                }
            }
        }

        // Crée un Border à partir d'un objet CardStats.
        private Border CreateCardBorder(CardStats stats)
        {
            Border cardBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(2),
                Margin = new Thickness(2),
                Width = 145,
                Background = Brushes.White,
                Tag = stats
            };

            StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };

            Image image = new Image
            {
                Source = new BitmapImage(new Uri(stats.CheminImage, UriKind.RelativeOrAbsolute)),
                Width = 190,
                Height = 182,
                Stretch = Stretch.Uniform
            };

            TextBlock title = new TextBlock
            {
                Text = stats.Nom,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock statsText = new TextBlock
            {
                Text = $"PV: {stats.PointsDeVie} | ATQ: {stats.Attaque} | PRIX: {stats.Prix}",
                FontSize = 12,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            sp.Children.Add(title);
            sp.Children.Add(image);
            sp.Children.Add(statsText);
            cardBorder.Child = sp;
            cardBorder.MouseDown += CartePiochee_Click;

            return cardBorder;
        }

        // Méthode de pioche qui applique aussi le discount pour la carte spéciale (id_attitude == 4).
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
            int idCarte = Convert.ToInt32(row["id_type"]); // 1: Attaque, 2: Défense, 3: Tank, etc.
            int prixCarte = Convert.ToInt32(row["Prix_carte"]);
            int attaqueCarte = Convert.ToInt32(row["Attaque_carte"]);
            int pvCarte = Convert.ToInt32(row["PV_carte"]);
            int idAttitude = Convert.ToInt32(row["id_attitude"]);
            string attitudeType = row["attitude_type"].ToString();

            // Pour les cartes d'archer (ID 2, 11, 10 ou 6), si l'attaque est nulle, on attribue 5.
            if ((idAttitude == 2 || idAttitude == 11 || idAttitude == 10 || idAttitude == 6) && attaqueCarte <= 0)
                attaqueCarte = 5;

            // Pour une carte spéciale (hors défense), affichage d'un message.
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
                BasePointsDeVie = pvCarte, // Pour conserver les PV initiaux en cas de résurrection.
                Prix = prixCarte,
                CheminImage = imagePath,
                Owner = isJoueur ? 1 : 2,
                IdAttitude = idAttitude,
                AttitudeType = attitudeType
            };

            // Gestion de la réduction pour la carte spéciale (id_attitude == 4)
            if (statsCard.IdAttitude == 4)
            {
                if (isJoueur)
                {
                    discountJoueur1 = true;
                    roundSummaryLog.Add($"{statsCard.Nom} active une réduction : la prochaine carte sera à moitié prix pour Joueur 1.");
                }
                else
                {
                    discountJoueur2 = true;
                    roundSummaryLog.Add($"{statsCard.Nom} active une réduction : la prochaine carte sera à moitié prix pour Joueur 2.");
                }
            }

            int effectivePrixCarte = prixCarte;
            if (!gratuit)
            {
                // Application du discount si actif.
                if (isJoueur && discountJoueur1)
                {
                    effectivePrixCarte = prixCarte / 2;
                    discountJoueur1 = false; // Réinitialisation après usage.
                }
                else if (!isJoueur && discountJoueur2)
                {
                    effectivePrixCarte = prixCarte / 2;
                    discountJoueur2 = false; // Réinitialisation après usage.
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

            statsCard.Prix = effectivePrixCarte;

            Border cardBorder = CreateCardBorder(statsCard);
            bankContainer.Children.Add(cardBorder);
            MessageTextBlock.Text = isJoueur
                ? "Unité piochée pour Joueur 1, mon Seigneur !"
                : "Unité piochée pour Joueur 2, mon Seigneur !";
            pioche.Tables["carte"].Rows.Remove(row);
            GenererNextCardPreview();
        }

        #endregion

        #region Gestion des interactions / placements

        private void PiocherCarteJoueur1_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(true);
        }

        private void PiocherCarteJoueur2_Click(object sender, RoutedEventArgs e)
        {
            PiocherCarte(false);
        }

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

        #endregion

        #region Zones de placement et affichage des cartes mortes

        private void AfficherCartesMortes_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Cartes mortes Joueur 1 :");
            if (cartesMortesJoueur1.Count == 0)
                sb.AppendLine("Aucune carte morte.");
            else
            {
                foreach (var card in cartesMortesJoueur1)
                    sb.AppendLine($"- {card.Nom}");
            }
            sb.AppendLine("\nCartes mortes Joueur 2 :");
            if (cartesMortesJoueur2.Count == 0)
                sb.AppendLine("Aucune carte morte.");
            else
            {
                foreach (var card in cartesMortesJoueur2)
                    sb.AppendLine($"- {card.Nom}");
            }
            MessageBox.Show(sb.ToString(), "Cartes Mortes");
        }

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

        #region Abandon et réinitialisation

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

        #region Phase de Soin

        // Exécute la phase de soin en début de manche.
        private void ExecuteHealingPhase()
        {
            // Pour Joueur 1
            var player1Cards = StackZoneJoueur1Attack.Children.OfType<Border>()
                                 .Concat(StackZoneJoueur1Defense.Children.OfType<Border>());
            foreach (var card in player1Cards)
            {
                if (card.Tag is CardStats stats)
                {
                    if (stats.IdAttitude == 8)
                    {
                        var firstAttack = StackZoneJoueur1Attack.Children.OfType<Border>().FirstOrDefault();
                        if (firstAttack != null && firstAttack.Tag is CardStats targetStats)
                        {
                            targetStats.PointsDeVie += 3;
                            UpdateCardDisplay(firstAttack);
                            roundSummaryLog.Add($"{stats.Nom} (Soin) soigne {targetStats.Nom} de 3 PV (phase de soin).");
                        }
                        else
                        {
                            roundSummaryLog.Add($"{stats.Nom} (Soin) n'a trouvé aucune carte en attaque à soigner pour Joueur 1.");
                        }
                    }
                    else if (stats.IdAttitude == 12)
                    {
                        foreach (var target in player1Cards)
                        {
                            if (target.Tag is CardStats targetStats)
                            {
                                targetStats.PointsDeVie += 2;
                                UpdateCardDisplay(target);
                            }
                        }
                        roundSummaryLog.Add($"{stats.Nom} (Bénédiction) soigne toutes les cartes de Joueur 1 de 2 PV (phase de soin).");
                    }
                }
            }
            // Pour Joueur 2
            var player2Cards = StackZoneJoueur2Attack.Children.OfType<Border>()
                                 .Concat(StackZoneJoueur2Defense.Children.OfType<Border>());
            foreach (var card in player2Cards)
            {
                if (card.Tag is CardStats stats)
                {
                    if (stats.IdAttitude == 8)
                    {
                        var firstAttack = StackZoneJoueur2Attack.Children.OfType<Border>().FirstOrDefault();
                        if (firstAttack != null && firstAttack.Tag is CardStats targetStats)
                        {
                            targetStats.PointsDeVie += 3;
                            UpdateCardDisplay(firstAttack);
                            roundSummaryLog.Add($"{stats.Nom} (Soin) soigne {targetStats.Nom} de 3 PV (phase de soin).");
                        }
                        else
                        {
                            roundSummaryLog.Add($"{stats.Nom} (Soin) n'a trouvé aucune carte en attaque à soigner pour Joueur 2.");
                        }
                    }
                    else if (stats.IdAttitude == 12)
                    {
                        foreach (var target in player2Cards)
                        {
                            if (target.Tag is CardStats targetStats)
                            {
                                targetStats.PointsDeVie += 2;
                                UpdateCardDisplay(target);
                            }
                        }
                        roundSummaryLog.Add($"{stats.Nom} (Bénédiction) soigne toutes les cartes de Joueur 2 de 2 PV (phase de soin).");
                    }
                }
            }
        }

        #endregion

        #region Process des attaques des Archers et Résolution des duels directs

        // Renvoie true si la carte est de type archer.
        private bool IsArcher(CardStats stats)
        {
            return stats.IdAttitude == 2 || stats.IdAttitude == 11 ||
                   stats.IdAttitude == 10 || stats.IdAttitude == 6;
        }

        // Gère les attaques des archers pour Joueur 1 et 2.
        private void ProcessArcherAttacks()
        {
            // Archers du Joueur 1 attaquant Joueur 2.
            List<Border> archersJ1 = new List<Border>();
            archersJ1.AddRange(StackZoneJoueur1Attack.Children.OfType<Border>().Where(b => b.Tag is CardStats cs && IsArcher(cs)));
            archersJ1.AddRange(StackZoneJoueur1Defense.Children.OfType<Border>().Where(b => b.Tag is CardStats cs && IsArcher(cs)));

            foreach (var archer in archersJ1)
            {
                Panel enemyDefense = StackZoneJoueur2Defense;
                Border target = enemyDefense.Children.OfType<Border>().FirstOrDefault(b =>
                {
                    CardStats cs = b.Tag as CardStats;
                    return cs != null && cs.Id == 3;
                });
                if (target == null)
                {
                    var defenseCards = enemyDefense.Children.OfType<Border>().ToList();
                    if (defenseCards.Any())
                        target = defenseCards.First();
                    else
                    {
                        Panel enemyAttack = StackZoneJoueur2Attack;
                        var attackCards = enemyAttack.Children.OfType<Border>().ToList();
                        if (attackCards.Any())
                            target = attackCards.First();
                    }
                }
                if (target != null)
                {
                    CardStats archerStats = (CardStats)archer.Tag;
                    CardStats targetStats = (CardStats)target.Tag;
                    int effectiveAttack = archerStats.Attaque;
                    targetStats.PointsDeVie -= effectiveAttack;
                    UpdateCardDisplay(target);
                    roundSummaryLog.Add($"{archerStats.Nom} (Archer) attaque {targetStats.Nom} pour {effectiveAttack} dégâts.");
                    if (targetStats.PointsDeVie <= 0)
                    {
                        if (StackZoneJoueur2Defense.Children.Contains(target))
                            StackZoneJoueur2Defense.Children.Remove(target);
                        else if (StackZoneJoueur2Attack.Children.Contains(target))
                            StackZoneJoueur2Attack.Children.Remove(target);
                        orJoueur1 += 2;
                        roundSummaryLog.Add($"{targetStats.Nom} est morte. +2 or pour Joueur 1.");
                        cartesMortesJoueur2.Add(targetStats);
                    }
                }
            }

            // Archers du Joueur 2 attaquant Joueur 1.
            List<Border> archersJ2 = new List<Border>();
            archersJ2.AddRange(StackZoneJoueur2Attack.Children.OfType<Border>().Where(b => b.Tag is CardStats cs && IsArcher(cs)));
            archersJ2.AddRange(StackZoneJoueur2Defense.Children.OfType<Border>().Where(b => b.Tag is CardStats cs && IsArcher(cs)));

            foreach (var archer in archersJ2)
            {
                Panel enemyDefense = StackZoneJoueur1Defense;
                Border target = enemyDefense.Children.OfType<Border>().FirstOrDefault(b =>
                {
                    CardStats cs = b.Tag as CardStats;
                    return cs != null && cs.Id == 3;
                });
                if (target == null)
                {
                    var defenseCards = enemyDefense.Children.OfType<Border>().ToList();
                    if (defenseCards.Any())
                        target = defenseCards.First();
                    else
                    {
                        Panel enemyAttack = StackZoneJoueur1Attack;
                        var attackCards = enemyAttack.Children.OfType<Border>().ToList();
                        if (attackCards.Any())
                            target = attackCards.First();
                    }
                }
                if (target != null)
                {
                    CardStats archerStats = (CardStats)archer.Tag;
                    CardStats targetStats = (CardStats)target.Tag;
                    int effectiveAttack = archerStats.Attaque;
                    targetStats.PointsDeVie -= effectiveAttack;
                    UpdateCardDisplay(target);
                    roundSummaryLog.Add($"{archerStats.Nom} (Archer) attaque {targetStats.Nom} pour {effectiveAttack} dégâts.");
                    if (targetStats.PointsDeVie <= 0)
                    {
                        if (StackZoneJoueur1Defense.Children.Contains(target))
                            StackZoneJoueur1Defense.Children.Remove(target);
                        else if (StackZoneJoueur1Attack.Children.Contains(target))
                            StackZoneJoueur1Attack.Children.Remove(target);
                        orJoueur2 += 2;
                        roundSummaryLog.Add($"{targetStats.Nom} est morte. +2 or pour Joueur 2.");
                        cartesMortesJoueur1.Add(targetStats);
                    }
                }
            }
        }
        #region Décrémente PV en défense
        // Décrémente 1 PV pour chaque carte en zone de défense dont l'IdAttitude est entre 2 et 12.
        private void DecrementerPVDefense()
        {
            // Pour Joueur 1
            foreach (Border card in StackZoneJoueur1Defense.Children.OfType<Border>().ToList())
            {
                if (card.Tag is CardStats stats)
                {
                    if (stats.IdAttitude >= 2 && stats.IdAttitude <= 12)
                    {
                        stats.PointsDeVie -= 1;
                        roundSummaryLog.Add($"{stats.Nom} perd 1 PV (fin de manche).");
                        UpdateCardDisplay(card);
                        if (stats.PointsDeVie <= 0)
                        {
                            cartesMortesJoueur1.Add(stats);
                            StackZoneJoueur1Defense.Children.Remove(card);
                        }
                    }
                }
            }
            // Pour Joueur 2
            foreach (Border card in StackZoneJoueur2Defense.Children.OfType<Border>().ToList())
            {
                if (card.Tag is CardStats stats)
                {
                    if (stats.IdAttitude >= 2 && stats.IdAttitude <= 12)
                    {
                        stats.PointsDeVie -= 1;
                        roundSummaryLog.Add($"{stats.Nom} perd 1 PV (fin de manche).");
                        UpdateCardDisplay(card);
                        if (stats.PointsDeVie <= 0)
                        {
                            cartesMortesJoueur2.Add(stats);
                            StackZoneJoueur2Defense.Children.Remove(card);
                        }
                    }
                }
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
            if (zoneType.Contains("defense") && stats.Id != 2 && stats.Id != 3)
            {
                MessageBox.Show("Seules les unités de défense ou le Tank peuvent être placées en zone de défense !");
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
                MessageBox.Show("Unité placée en défense. La capacité sera activée à la fin de la manche.", "Information");
            }
            carteSelectionnee.BorderBrush = Brushes.Black;
            carteSelectionnee = null;
            MessageTextBlock.Text = "Unité placée, mon Seigneur !";
            e.Handled = true;
        }

        #endregion
        #region Activation des capacités de défense
        // Parcoure les zones d'attaque et de défense pour activer les capacités (ex. Explosion) sur le terrain.
        private void ActiverCapacitesDefense()
        {
            // Pour Joueur 1 en défense.
            foreach (Border card in StackZoneJoueur1Defense.Children.OfType<Border>())
            {
                if (card.Tag is CardStats stats && stats.IdAttitude >= 2 && stats.IdAttitude <= 12)
                {
                    SpecificiteCarte spec = new SpecificiteCarte(stats.IdAttitude, stats.Nom);
                    roundSummaryLog.Add(spec.ExpliquerCapacite());
                    AppliquerCapaciteDefense(true, stats);
                }
            }
            // Pour Joueur 2 en défense.
            foreach (Border card in StackZoneJoueur2Defense.Children.OfType<Border>())
            {
                if (card.Tag is CardStats stats && stats.IdAttitude >= 2 && stats.IdAttitude <= 12)
                {
                    SpecificiteCarte spec = new SpecificiteCarte(stats.IdAttitude, stats.Nom);
                    roundSummaryLog.Add(spec.ExpliquerCapacite());
                    AppliquerCapaciteDefense(false, stats);
                }
            }
            // Pour Explosion (ID 7) en attaque.
            foreach (Border card in StackZoneJoueur1Attack.Children.OfType<Border>())
            {
                if (card.Tag is CardStats stats && stats.IdAttitude == 7)
                {
                    SpecificiteCarte spec = new SpecificiteCarte(stats.IdAttitude, stats.Nom);
                    roundSummaryLog.Add(spec.ExpliquerCapacite());
                    AppliquerCapaciteDefense(true, stats);
                }
            }
            foreach (Border card in StackZoneJoueur2Attack.Children.OfType<Border>())
            {
                if (card.Tag is CardStats stats && stats.IdAttitude == 7)
                {
                    SpecificiteCarte spec = new SpecificiteCarte(stats.IdAttitude, stats.Nom);
                    roundSummaryLog.Add(spec.ExpliquerCapacite());
                    AppliquerCapaciteDefense(false, stats);
                }
            }
        }
        #endregion



        #endregion
        #region Résolution des combats et synthèse de manche

        private void FinDeManche_Click(object sender, RoutedEventArgs e)
        {
            // 1. Phase de soin.
            ExecuteHealingPhase();

            // 2. Activation des capacités sur le terrain.
            ActiverCapacitesDefense();

            // 3. Décrémente les PV en défense.
            DecrementerPVDefense();

            // 4. Traitement des attaques des archers.
            ProcessArcherAttacks();

            // 5. Traitement des duels directs entre cartes restantes (non-archers).
            List<Border> remainingAttackJ1 = StackZoneJoueur1Attack.Children.OfType<Border>()
                .Where(b => !(b.Tag is CardStats cs && IsArcher(cs))).ToList();
            List<Border> remainingAttackJ2 = StackZoneJoueur2Attack.Children.OfType<Border>()
                .Where(b => !(b.Tag is CardStats cs && IsArcher(cs))).ToList();

            int nbDuels = Math.Min(remainingAttackJ1.Count, remainingAttackJ2.Count);
            int totalDamageJ1 = 0, totalDamageJ2 = 0;
            for (int i = 0; i < nbDuels; i++)
            {
                CardStats statsJ1 = (CardStats)remainingAttackJ1[i].Tag;
                CardStats statsJ2 = (CardStats)remainingAttackJ2[i].Tag;
                int effectiveAttackJ1 = statsJ1.Attaque + ((statsJ1.IdAttitude == 1) ? 2 : 0);
                int effectiveAttackJ2 = statsJ2.Attaque + ((statsJ2.IdAttitude == 1) ? 2 : 0);
                statsJ1.PointsDeVie -= effectiveAttackJ2;
                statsJ2.PointsDeVie -= effectiveAttackJ1;
                UpdateCardDisplay(remainingAttackJ1[i]);
                UpdateCardDisplay(remainingAttackJ2[i]);
                totalDamageJ1 += effectiveAttackJ1;
                totalDamageJ2 += effectiveAttackJ2;
                roundSummaryLog.Add($"{statsJ1.Nom} inflige {effectiveAttackJ1} dégâts à {statsJ2.Nom} et réciproquement {effectiveAttackJ2} dégâts.");
                if (statsJ1.PointsDeVie <= 0)
                {
                    if (!cartesMortesJoueur1.Contains(statsJ1))
                        cartesMortesJoueur1.Add(statsJ1);
                    orJoueur2 += 2;
                    roundSummaryLog.Add($"{statsJ1.Nom} est morte. +2 or pour Joueur 2.");
                }
                if (statsJ2.PointsDeVie <= 0)
                {
                    if (!cartesMortesJoueur2.Contains(statsJ2))
                        cartesMortesJoueur2.Add(statsJ2);
                    orJoueur1 += 2;
                    roundSummaryLog.Add($"{statsJ2.Nom} est morte. +2 or pour Joueur 1.");
                }
            }
            // 6. Attaques additionnelles pour les cartes restantes sans duel direct.
            if (remainingAttackJ1.Count > nbDuels)
            {
                for (int i = nbDuels; i < remainingAttackJ1.Count; i++)
                {
                    CardStats statsJ1 = (CardStats)remainingAttackJ1[i].Tag;
                    totalDamageJ1 += statsJ1.Attaque;
                    roundSummaryLog.Add($"{statsJ1.Nom} attaque pour {statsJ1.Attaque} dégâts (aucun duel direct).");
                    UpdateCardDisplay(remainingAttackJ1[i]);
                }
            }
            if (remainingAttackJ2.Count > nbDuels)
            {
                for (int i = nbDuels; i < remainingAttackJ2.Count; i++)
                {
                    CardStats statsJ2 = (CardStats)remainingAttackJ2[i].Tag;
                    totalDamageJ2 += statsJ2.Attaque;
                    roundSummaryLog.Add($"{statsJ2.Nom} attaque pour {statsJ2.Attaque} dégâts (aucun duel direct).");
                    UpdateCardDisplay(remainingAttackJ2[i]);
                }
            }
            MettreAJourAffichageOr();
            NettoyerZones();

            StringBuilder summary = new StringBuilder();
            summary.AppendLine("Résultat de la Manche :");
            summary.AppendLine("-------------------------------------------------");
            summary.AppendLine("Cartes mortes Joueur 1 : " +
                (cartesMortesJoueur1.Count == 0 ? "Aucune" : string.Join(", ", cartesMortesJoueur1.Select(cs => cs.Nom))));
            summary.AppendLine("Cartes mortes Joueur 2 : " +
                (cartesMortesJoueur2.Count == 0 ? "Aucune" : string.Join(", ", cartesMortesJoueur2.Select(cs => cs.Nom))));
            summary.AppendLine();
            summary.AppendLine("Or final Joueur 1 : " + orJoueur1);
            summary.AppendLine("Or final Joueur 2 : " + orJoueur2);
            summary.AppendLine();
            summary.AppendLine("Capacités activées : " +
                (roundSummaryLog.Count == 0 ? "Aucune" : string.Join(" | ", roundSummaryLog)));

            

            bool resetManche = false;
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
            if (vieJoueur1 <= 0 || vieJoueur2 <= 0)
            {
                if (vieJoueur1 <= 0)
                {
                    // Le Joueur 2 gagne : navigation vers la page de victoire "vicj2"
                    NavigationService.Navigate(new vicJ2());
                }
                else if (vieJoueur2 <= 0)
                {
                    // Le Joueur 1 gagne : navigation vers la page de victoire "vicj1"
                    NavigationService.Navigate(new vicJ1());
                }
                return; // Arrête le traitement de la manche
            }

            // Mise à jour immédiate de l'affichage des vies.
            MettreAJourAffichageVies();

            MessageBox.Show(summary.ToString(), "Synthèse de la Manche");

            // Vérification des conditions de victoire : un joueur qui n'a plus de vies perd la partie.
           

            // Affichage des messages de réduction si applicable.
            if (discountJoueur1)
                MessageBox.Show("Joueur 1, la prochaine carte piochée sera à moitié prix !", "Réduction activée");
            if (discountJoueur2)
                MessageBox.Show("Joueur 2, la prochaine carte piochée sera à moitié prix !", "Réduction activée");

            if (resetManche)
                ResetGame();
        }


        #endregion

        #region Activation des capacités spéciales (Défense, Explosion, Soin)
        private void AppliquerCapaciteDefense(bool isJoueur, CardStats stats)
        {
            string nomJoueur = isJoueur ? "Joueur 1" : "Joueur 2";
            switch (stats.IdAttitude)
            {
                case 2:
                case 5:
                case 6:
                case 11:
                    roundSummaryLog.Add($"{stats.Nom} ne possède pas de capacité de défense activable.");
                    break;
                case 3: // Tank : capacité gérée ailleurs.
                    roundSummaryLog.Add($"{stats.Nom} (Tank) est en position pour absorber les dégâts.");
                    break;
                case 4:
                    roundSummaryLog.Add($"{stats.Nom} a activé la réduction, la prochaine carte sera à moitié prix.");
                    break;
                case 7: // Explosion
                    var ennemis = isJoueur
                        ? StackZoneJoueur2Attack.Children.OfType<Border>().Concat(StackZoneJoueur2Defense.Children.OfType<Border>())
                        : StackZoneJoueur1Attack.Children.OfType<Border>().Concat(StackZoneJoueur1Defense.Children.OfType<Border>());
                    foreach (var card in ennemis)
                    {
                        if (card.Tag is CardStats enemyStats)
                        {
                            enemyStats.PointsDeVie -= stats.Attaque;
                            UpdateCardDisplay(card);
                        }
                    }
                    roundSummaryLog.Add($"{stats.Nom} (Explosion) inflige {stats.Attaque} dégâts à toutes les cartes adverses.");
                    break;
                case 8:
                    roundSummaryLog.Add($"{stats.Nom} (Soin) a effectué son action en phase de soin.");
                    break;
                case 9:
                    if (isJoueur)
                    {
                        PiocherCarte(true, true);
                        PiocherCarte(true, true);
                    }
                    else
                    {
                        PiocherCarte(false, true);
                        PiocherCarte(false, true);
                    }
                    roundSummaryLog.Add($"{stats.Nom} (Inspiration) fait piocher 2 cartes gratuitement à {nomJoueur}.");
                    break;
                case 10:
                    {
                        var graveyard = isJoueur ? cartesMortesJoueur1 : cartesMortesJoueur2;
                        if (graveyard.Count > 0)
                        {
                            CardStats resurrectedStats = graveyard.Last();
                            graveyard.RemoveAt(graveyard.Count - 1);
                            resurrectedStats.PointsDeVie = resurrectedStats.BasePointsDeVie;
                            resurrectedStats.Prix = 0;
                            Border resurrectedCard = CreateCardBorder(resurrectedStats);
                            Panel benchPanel = isJoueur ? CarteContainerJoueur1 : CarteContainerJoueur2;
                            benchPanel.Children.Add(resurrectedCard);
                            roundSummaryLog.Add($"{stats.Nom} (Résurrection) ramène {resurrectedStats.Nom} du cimetière pour {nomJoueur} gratuitement.");
                        }
                        else
                        {
                            roundSummaryLog.Add($"{stats.Nom} (Résurrection) n’a trouvé aucune carte à ressusciter.");
                        }
                    }
                    break;
                case 12:
                    roundSummaryLog.Add($"{stats.Nom} (Bénédiction) a effectué son action en phase de soin.");
                    break;
                default:
                    roundSummaryLog.Add($"{stats.Nom} a une capacité inconnue (ID {stats.IdAttitude}).");
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
        public int BasePointsDeVie { get; set; } // Points de vie initiaux.
        public int Prix { get; set; }
        public string CheminImage { get; set; }
        public int Owner { get; set; }
        public int IdAttitude { get; set; }
        public string AttitudeType { get; set; }
    }
}
