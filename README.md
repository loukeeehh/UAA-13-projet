# Game Of Cards
# Projet UAA13 - Jeu de Cartes Stratégique

Ce projet, nommé **Gma Of Cards**, est un jeu de cartes stratégique développé en C# avec WPF. Le jeu met en scène deux joueurs s'affrontant à l'aide d'unités aux capacités variées, gérant des phases comme la pioche, le placement sur le plateau, l'activation de capacités spéciales et la résolution des combats.

## Fonctionnalités

- **Gestion des Cartes :**
  - Pioche et affichage dynamique des cartes
  - Création de cartes avec statistiques (PV, Attaque, Prix) et capacités spéciales (pour les cartes defense)
- **Combat & Résolution :**
  - Attaque automatique à la fin d'un manche
  - Etat des cartes (mort ou vivant)
  - Activation de capacités spéciales pour les cartes défensive (soin, archer, ressuciter, ect)
  - Gestion de conditions de victoire/défaite basées sur le nombre de cartes mortes et l'or (système de point de vie pour les joueurs)
- **Navigation et Interface :**
  - Navigation entre différentes pages (Plateau, Carte, Paramètres, et règle)
- **Base de Données :**
  - Récupération des données des cartes à partir d'une base MySQL
  - Utilisation de DataSet pour charger dynamiquement la pioche et les informations des cartes
## Technologies Utilisées

- **Langage :** C#
- **Base de Données :** MySQL (via MySql.Data)
- **Environnement de Développement :** Visual Studio

## Installation

1. **Cloner le dépôt :**
   ```bash
   git clone https://github.com/votre-utilisateur/6T24_LudoBechet_ProjetUaa13.git

