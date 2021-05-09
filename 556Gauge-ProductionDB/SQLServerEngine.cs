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

            this.EngineConnectionString = builder.ConnectionString;

            this.ClearLogs();
        }

        private void ClearLogs()
        {
            DateTime thirtydaysago = (DateTime.Now.AddDays(-30));

            using (SqlConnection connection = new SqlConnection(this.EngineConnectionString))
            {
                string query = "DELETE FROM [dbo].[logs] WHERE LogDate < '" + thirtydaysago.ToString("yyyy-MM-dd") + "'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                return;
            }
        }

        public List<BackupQueryRow> GetRowsSinceCutoff(MYSQLEngine mySqlEng)
        {
            using (SqlConnection connection = new SqlConnection(this.EngineConnectionString))
            {
                long HighestProdID = mySqlEng.ReadHighestProdReferenceID();

                mySqlEng.MySQLLog($"Began querying for new observations after row {HighestProdID}.");

                string query = $"SELECT TOP (1000) [isPPR], [price], [rounds], [PPR], [prodTitle], [prodSource], [scrapeURL], [WriteDate], [ObservationID] FROM [dbo].[price_observations] WHERE [ObservationID] > {HighestProdID} ORDER BY ObservationID asc;";

                List<BackupQueryRow> ret = new List<BackupQueryRow>();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            List<string> add = new List<string>();

                            add.Add("" + BoolToNumString(reader.GetBoolean(0)));
                            add.Add("" + reader.GetDouble(1));
                            add.Add("" + reader.GetInt32(2));
                            add.Add("" + reader.GetDouble(3));
                            add.Add("" + reader.GetString(4));
                            add.Add("" + reader.GetString(5));
                            add.Add("" + reader.GetString(6));
                            add.Add("" + reader.GetDateTime(7).ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            long BID = reader.GetInt32(8);
                            add.Add("" + BID);

                            BackupQueryRow finishedRow = new BackupQueryRow(add, BID);

                            ret.Add(finishedRow);
                        }
                    }

                    connection.Close();
                }

                mySqlEng.MySQLLog($"Found {ret.Count} rows after row {HighestProdID}.");

                return ret;
            }
        }

        public long ReadHighetBackupID()
        {
            long ret = -1;

            using (SqlConnection connection = new SqlConnection(this.EngineConnectionString))
            {
                string query = $"SELECT TOP 1 [ObservationID] FROM [dbo].[price_observations] ORDER BY [ObservationID] desc;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ret = reader.GetInt32(0);
                        }
                    }
                }
            }

            if(ret == -1)
            {
                throw new Exception("-1 for return value; generic backup ID read failure.");
            }
            else
            {
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

    public class BackupQueryRow
    {
        public List<string> Result;
        public long BackupID;

        public BackupQueryRow()
        {
            this.Result = new List<string>();
        }

        public BackupQueryRow(List<string> result, long backupID)
        {
            this.Result = result;
            this.BackupID = backupID;
        }
    }
}
