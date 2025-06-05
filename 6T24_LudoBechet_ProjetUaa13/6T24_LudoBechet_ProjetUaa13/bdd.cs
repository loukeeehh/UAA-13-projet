using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace _6T24_LudoBechet_ProjetUaa13
{
    class bdd
    {
        
        public void CD()
        {
            MySqlConnection connection = null;
            MySqlDataAdapter adapter = null;
        }

        // Retourne la chaîne de connexion à la base de données.
        //  Stocker les identifiants en dur peut poser des risques de sécurité. 
        //    Une meilleure approche serait d'utiliser des variables d'environnement ou un fichier de configuration.
        static string CheminBDD()
        {
            return "server=10.10.51.98; database=ludo; port=3306; UserId=ludo; password=root";
        }

        // Cette méthode recherche une carte dans la base de données et stocke les résultats dans un DataSet.
        public bool ChercheCarte(out DataSet infos)
        {
            bool ok = false;
            infos = new DataSet();
            MySqlConnection maConnection = new MySqlConnection(CheminBDD());

            // Requête SQL intégrant une jointure avec la table "attitude"
            string query = @"
                SELECT 
                    carte.Nom_carte, 
                    carte.Description_carte, 
                    carte.Image, 
                    carte.PV_carte, 
                    carte.Prix_carte, 
                    carte.Attaque_carte,
                    carte.id_attitude,
                    attitude.attitude_type,
                    carte.id_type
                FROM 
                    carte
                JOIN 
                    attitude ON carte.id_attitude = attitude.id_attitude";

            try
            {
                maConnection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(query, maConnection);
                da.Fill(infos, "carte");
                maConnection.Close();

                // Définition du chemin des images
                string cheminImages = "../Asset/";

                // Ajout d'une colonne calculée pour stocker le chemin complet des images
                if (infos.Tables.Contains("carte"))
                {
                    infos.Tables["carte"].Columns.Add("CheminImage", typeof(string));
                    foreach (DataRow row in infos.Tables["carte"].Rows)
                    {
                        row["CheminImage"] = cheminImages + row["Image"].ToString();
                    }
                }

                ok = true;
            }
            catch (Exception ex)
            {
                // Debugging : Affichage des erreurs mais attention, la propagation de l'exception avec `throw` peut interrompre le programme.
                // Solution alternative : Loguer l'erreur et retourner `false` sans lever l'exception.
                Debug.WriteLine("Erreur dans ChercheCarte: " + ex.Message);
                throw;
            }

            return ok;
        }

        // Méthode qui retourne toutes les cartes disponibles.
        public DataSet ObtenirCartes()
        {
            DataSet infos = new DataSet();

            // Requête SQL similaire à celle de `ChercheCarte()`
            string query = @"
                SELECT 
                    carte.Nom_carte, 
                    carte.Description_carte, 
                    carte.Image, 
                    carte.PV_carte, 
                    carte.Prix_carte, 
                    carte.Attaque_carte,
                    carte.id_attitude,
                    attitude.attitude_type,
                    carte.id_type
                FROM carte
                JOIN attitude ON carte.id_attitude = attitude.id_attitude";

            string cheminImages = "../Asset/";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(CheminBDD()))
                {
                    connection.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, connection);
                    da.Fill(infos, "carte");

                    // Ajout de la colonne `CheminImage` pour stocker les chemins complets
                    if (infos.Tables.Contains("carte"))
                    {
                        infos.Tables["carte"].Columns.Add("CheminImage", typeof(string));

                        foreach (DataRow row in infos.Tables["carte"].Rows)
                        {
                            string imageFileName = row["Image"].ToString();
                            string fullPath = cheminImages + imageFileName;
                            row["CheminImage"] = fullPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 🔍 Debugging : Affichage des erreurs en cas de problème de connexion ou d'exécution de la requête.
                Debug.WriteLine("Erreur lors de la récupération des cartes : " + ex.Message);
            }

            return infos;
        }
    }
}
