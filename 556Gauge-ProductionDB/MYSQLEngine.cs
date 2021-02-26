using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace _556Gauge_ProductionDB
{
    public class NoRowsException : Exception
    {
        public NoRowsException()
        {
        }

        public NoRowsException(string message)
            : base(message)
        {
        }
    }

    class MYSQLEngine
    {
        public string Server, User, Database, Password, CutoffDate;

        private Logger Logger;

        public MYSQLEngine(string server, string user, string database, string password, Logger logger)
        {
            this.Server = server; this.User = user; this.Database = database; this.Password = password;

            this.Logger = logger;

            try
            {
                this.CutoffDate = this.ReadCutoffDate();
            }
            catch (NoRowsException NREx)
            {
                this.SQLLog(NREx.Message);

                this.CutoffDate = "1800-01-01 11:59:59:000";
            }
            catch (Exception Ex)
            {
                throw Ex;
            }

            this.SQLLog("Set Cutoff to " + this.CutoffDate);
        }

        private void SQLLog(string log)
        {
            this.Logger.log(log);
        }

        private string ReadCutoffDate()
        {
            MYSQLEngineQuery eq = new MYSQLEngineQuery(this.Server, this.User, this.Database, this.Password);

            eq.Query = "SELECT `WriteDate` FROM 556prod.price_observations ORDER BY `WriteDate` DESC LIMIT 1;";

            try
            {
                List<string> ret = Execute(eq)[0];

                return ret[0];
            }
            catch(NoRowsException NREx)
            {
                NREx = new NoRowsException(eq.Query + " returned no rows.");

                throw NREx;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public bool InsertPriceRows(List<List<string>> rows)
        {
            foreach(List<string> row in rows)
            {
                MYSQLEngineQuery eq = new MYSQLEngineQuery(this.Server, this.User, this.Database, this.Password);

                eq.Query = "INSERT INTO `556prod`.`price_observations` (`isPPR`, `price`, `rounds`, `PPR`, `prodTitle`, `prodSource`, `scrapeURL`, `WriteDate`, `ObservationID`) " + 
                    "VALUES(" + 
                    ""  + row[0] + ", " +
                    ""  + row[1] + ", " +
                    ""  + row[2] + ", " +
                    ""  + row[3] + ", " +
                    "'" + row[4] + "', " +
                    "'" + row[5] + "', " +
                    "'" + row[6] + "', " +
                    "'" + row[7] + "', " +
                    ""  + row[8] + ");";

                Execute(eq, false);
            }


            return false;
        }

        private static List<List<string>> Execute(MYSQLEngineQuery EQ)
        {
            return Execute(EQ, true);
        }

        private static List<List<string>> Execute(MYSQLEngineQuery EQ, bool read)
        {
            string connStr = "server="
                                + EQ.Server
                                + ";user="
                                + EQ.User
                                + ";database="
                                + EQ.Database
                                + ";port=3306;password="
                                + EQ.Password;

            MySqlConnection conn = new MySqlConnection(connStr);

            List<List<string>> ret = new List<List<string>>();

            try
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(EQ.Query, conn);

                MySqlDataReader rdr = cmd.ExecuteReader();

                if (read == true)
                {
                    int DateColNum = FindDateColNum(rdr);

                    while (rdr.Read())
                    {
                        List<string> tackon = new List<string>();

                        short ii = 0;

                        while (ii < rdr.FieldCount)
                        {
                            if (ii == DateColNum)
                            {
                                DateTime writedate = (DateTime)rdr[ii];

                                tackon.Add(writedate.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                            }
                            else
                            {
                                tackon.Add(rdr[ii].ToString());
                            }
                            ii++;
                        }

                        ret.Add(tackon);
                    }
                }

                rdr.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException pwex)
            {
                throw pwex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();

            if (ret.Count == 0 && read == true)
            {
                throw new NoRowsException();
            }
            else
            {
                return ret;
            }
        }

        static private int FindDateColNum(MySqlDataReader rdr)
        {
            for (int ii = 0; ii < rdr.FieldCount; ii++)
            {
                if (rdr.GetName(ii) == "WriteDate")
                {
                    return ii;
                }
            }

            throw new Exception("Did not find a 'WriteDate' column");
        }
    }
}
