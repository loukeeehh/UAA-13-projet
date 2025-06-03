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

        static string CheminBDD()
        {
            return "server=10.10.51.98; database=ludo; port=3306; UserId=ludo; password=root";
        }

        public bool ChercheCarte(out DataSet infos)
        {
            bool ok = false;
            infos = new DataSet();
            MySqlConnection maConnection = new MySqlConnection(CheminBDD());

            // Requête intégrant la jointure avec la table attitude pour récupérer id_attitude et attitude_type
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

                // Chemin complet des images (utilisez un chemin adapté à votre environnement)
                string cheminImages = "../Asset/";

                // Ajoute une colonne calculée au DataTable pour le chemin complet de l'image
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
                Debug.WriteLine("Erreur dans ChercheCarte: " + ex.Message);
                throw;
            }
            return ok;
        }

        public DataSet ObtenirCartes()
        {
            DataSet infos = new DataSet();
            // Requête modifiée pour inclure id_attitude et attitude_type
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

                    // Ajoute une colonne "CheminImage" pour stocker les chemins complets
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
                Debug.WriteLine("Erreur lors de la récupération des cartes : " + ex.Message);
            }

            return infos;
        }
    }
}
