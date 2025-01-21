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
            string query = "SELECT Nom_carte, Description_carte, Image, PV_carte, Prix_carte, Attaque_carte FROM carte";

            try
            {
                maConnection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(query, maConnection);
                da.Fill(infos, "carte");
                maConnection.Close();

                // Ajout du chemin complet des images
                string cheminImages = "file:///H:/UAA-13-projet/6T24_LudoBechet_ProjetUaa13/6T24_LudoBechet_ProjetUaa13/Asset/";

                // Ajouter une colonne calculée au DataTable
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
                Debug.WriteLine(ex.Message);
                throw;
            }
            return ok;
        }
        public bool AttitudeCarte(out DataSet infos)
        {
            bool ok = false;
            infos = new DataSet();
            MySqlConnection maConnection = new MySqlConnection(CheminBDD());
            string query = "SELECT Attiude_Type FROM attitude";
            try
            {
                maConnection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(query, maConnection);
                da.Fill(infos, "attitude");
                maConnection.Close();

                


                ok = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            return ok;


        }

    }
}
