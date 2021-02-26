using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace _556Gauge_ProductionDB
{
    class SQLServerEngine
    {
        string EngineConnectionString;

        Logger Logger;
        public SQLServerEngine(string server, string catalog, string user, string password, Logger logger)
        {
            this.Logger = logger;

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = server;

            builder.UserID = user;

            builder.Password = password;

            builder.InitialCatalog = catalog;

            builder.Encrypt = true;

            builder.ConnectTimeout = 10;

            Console.WriteLine(builder.ConnectionString);

            this.EngineConnectionString = builder.ConnectionString;
        }

        public List<List<string>> GetRowsSinceCutoff(string cutoff)
        {
            using (SqlConnection connection = new SqlConnection(this.EngineConnectionString))
            {
                string query = "SELECT [isPPR], [price], [rounds], [PPR], [prodTitle], [prodSource], [scrapeURL], [WriteDate], [ObservationID] FROM [dbo].[price_observations] WHERE [WriteDate] > '" + 
                    cutoff + "'";

                List<List<string>> ret = new List<List<string>>();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            List<string> add = new List<string>();

                            add.Add("" + BoolToNumString(reader.GetBoolean(0)));
                            add.Add( "" + reader.GetDouble(1));
                            add.Add("" + reader.GetInt32(2));
                            add.Add("" + reader.GetDouble(3));
                            add.Add("" + reader.GetString(4));
                            add.Add("" + reader.GetString(5));
                            add.Add("" + reader.GetString(6));
                            add.Add("" + reader.GetDateTime(7).ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            add.Add("" + reader.GetInt32(8));

                            ret.Add(add);
                        }
                    }

                    connection.Close();
                }

                return ret;
            }
        }

        private static string BoolToNumString(bool Bool)
        {
            if (Bool == true)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

    }
}
