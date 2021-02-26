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







        // STATIC BASIC FUNCTIONS

        //public static string timenow()
        //{
        //    return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        //}

        

        //public static void WriteManifest(Parser parser)
        //{

        //    // write old table to archive




        //    // drop old table

        //    parser.eq.query = "DELETE FROM `stockplanner`.`manifest`;";

        //    Execute(parser.eq);

        //    // write new table

        //    foreach (ManifestValue mv in parser.manifestvalues.values)
        //    {
        //        parser.eq.query = "INSERT INTO `stockplanner`.`manifest` (`stock`, `target_percentage`, `write_date`) VALUES ('" +
        //                    mv.stock +
        //                    "'," +
        //                    mv.targetpercentage +
        //                    ",'" +
        //                    timenow() +
        //                    "');";

        //        Execute(parser.eq);
        //    }

        //    parser.eq.query = "";
        //}

        //public static void WriteYCA(EngineQuery eq, string amnt)
        //{
        //    eq.query = "INSERT INTO `stockplanner`.`yearly_contribution` (`write_date`, `yearly_contribution_amount`) VALUES(" +
        //                "'" + timenow() + "'," +
        //                amnt +
        //                ");"
        //                ;

        //    Execute(eq);

        //    eq.query = "";

        //}

        //public static bool CheckPW(EngineQuery eq)
        //{
        //    try
        //    {
        //        eq.query = "SELECT * FROM stockplanner.view_portfolio;";

        //        Execute(eq);
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //public static void WriteSource(Parser parser)
        //{
        //    if (parser.has_csv_file == true)
        //    {
        //        // write old table to archive

        //        ArchiveSource(parser.readvalues, parser.eq);
        //    }

        //    // drop old table

        //    parser.eq.query = "DELETE FROM `stockplanner`.`source`;";

        //    Execute(parser.eq);


        //    // write new table

        //    foreach (CSVValue csvv in parser.readvalues.values)
        //    {
        //        parser.eq.query = "INSERT INTO `stockplanner`.`source` (`stock`, `current_value`, `quantity`, `write_date`) VALUES ('" +
        //                    csvv.stock +
        //                    "'," +
        //                    csvv.current_value +
        //                    "," +
        //                    csvv.quantity +
        //                    ",'" +
        //                    csvv.write_date +
        //                    "');";

        //        Execute(parser.eq);
        //    }

        //    parser.eq.query = "";
        //}

        //public static CSVValues ReadSource(in EngineQuery eq)
        //{
        //    eq.query = "SELECT * FROM stockplanner.source;";
        //    try
        //    {
        //        CSVValues ret = new CSVValues(Execute(eq));
        //        eq.query = "";

        //        return ret;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}



        //public static void ArchiveSource(in EngineQuery eq)
        //{
        //    ArchiveSource(new CSVValues(), eq);
        //}

        //public static void ArchiveSource(CSVValues csvvs, in EngineQuery eq)
        //{
        //    if (csvvs.initialized == false)
        //    {
        //        csvvs = ReadSource(eq);
        //    }

        //    foreach (CSVValue csvv in csvvs.values)
        //    {
        //        eq.query = "INSERT INTO `stockplanner`.`source_archive` (`stock`, `write_date`, `current_value`, `quantity`) VALUES (" +
        //                    "'" + csvv.stock + "', " +
        //                    "'" + csvv.write_date + "', " +
        //                    csvv.current_value + ", " +
        //                    csvv.quantity +
        //                    "); ";

        //        Execute(eq);
        //    }
        //}

    }
}
