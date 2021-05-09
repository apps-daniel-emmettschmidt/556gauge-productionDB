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
        public MYSQLEngineConnectionParameters ConnectionParameters;

        private Logger Logger;

        public MYSQLEngine(string server, string user, string database, string password, Logger logger)
        {
            Initialize(new MYSQLEngineConnectionParameters(server, user, database, password), logger);
        }

        public MYSQLEngine(MYSQLEngineConnectionParameters ConnectionParameters, Logger logger)
        {
            Initialize(ConnectionParameters, logger);
        }

        private void Initialize(MYSQLEngineConnectionParameters connectionParameters, Logger logger)
        {
            this.ConnectionParameters = connectionParameters;

            this.Logger = logger;

            try
            {
                long read = this.ReadHighestProdReferenceID();

                this.MySQLLog("Highest prod ID is " + read);

                this.ClearLogs();
            }
            catch (NoRowsException NREx)
            {
                this.MySQLLog(NREx.Message);
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public void MySQLLog(string log)
        {
            this.Logger.log(log);

            this.WriteLog(log);
        }

        private void WriteLog(string log)
        {
            MYSQLEngineQuery eq = new MYSQLEngineQuery(this.ConnectionParameters);

            log = Program.TrimLog(log, 200);

            eq.Query = "INSERT INTO `556prod`.`logs` (`LogDate`, `LogEntry`) VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + 
                            "', '" + log + "');";

            Execute(eq, false);
        }

        public string BuildConnectionString()
        {
            return this.ConnectionParameters.BuildConnectionString();
        }

        private void ClearLogs()
        {
            DateTime thirtydaysago = (DateTime.Now.AddDays(-30));

            string delete = "DELETE FROM `556prod`.`logs` WHERE `logdate` < '" + thirtydaysago.ToString("yyyy-MM-dd") + "'";

            string connStr = BuildConnectionString();

            MySqlConnection conn = new MySqlConnection(connStr);

            MySqlCommand command = new MySqlCommand(delete, conn);

            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
        }

        public long ReadHighestProdReferenceID()
        {
            MYSQLEngineQuery eq = new MYSQLEngineQuery(this.ConnectionParameters);

            eq.Query = "SELECT `Observations`.`BackupID` FROM `556prod`.`Observations` ORDER BY `Observations`.`BackupID` DESC LIMIT 1;";

            try
            {
                return Execute(eq, true, QueryReturnType.ReturnLongID).ReturnLongResult();
            }
            catch(NoRowsException NREx)
            {
                this.MySQLLog(eq.Query + " returned no rows, returning 0;");

                return 0;
            }
            catch (Exception Ex)
            {
                this.WriteLog(Ex.Message);

                throw Ex;
            }
        }

        public void InsertPriceRows(List<BackupQueryRow> rows)
        {
            this.MySQLLog("Began inserting rows.");

            foreach(BackupQueryRow row in rows)
            {
                MYSQLEngineQuery eq = new MYSQLEngineQuery(this.ConnectionParameters);

                eq.Query = "INSERT INTO `556prod`.`Observations` (`isPPR`, `Price`, `Rounds`, `PPR`, `ProductTitle`, `ProductSource`, `ScrapeUrl`, `WhenObserved`, `BackupID`)  " + 
                    "VALUES(" + 
                    ""  + row.Result[0] + ", " +
                    ""  + row.Result[1] + ", " +
                    ""  + row.Result[2] + ", " +
                    ""  + row.Result[3] + ", " +
                    "'" + row.Result[4] + "', " +
                    "'" + row.Result[5] + "', " +
                    "'" + row.Result[6] + "', " +
                    "'" + row.Result[7] + "', " +
                    ""  + row.Result[8] + ");";

                Execute(eq, false);
            }

            this.MySQLLog("Completed inserting rows.");

            return;
        }

        private static QueryResult Execute(MYSQLEngineQuery EQ)
        {
            try
            {
                return Execute(EQ, true);
            }
            catch(NoRowsException nrex)
            {
                throw nrex;
            }
            catch (MySql.Data.MySqlClient.MySqlException pwex)
            {
                throw pwex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private static QueryResult Execute(MYSQLEngineQuery EQ, bool read)
        {
            return Execute(EQ, read, QueryReturnType.ReturnStrings);
        }

        private static QueryResult Execute(MYSQLEngineQuery EQ, bool read, QueryReturnType returnType)
        {
            string connStr = EQ.ConnectionParameters.BuildConnectionString();

            MySqlConnection conn = new MySqlConnection(connStr);

            List<List<string>> ret = new List<List<string>>();

            try
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(EQ.Query, conn);

                MySqlDataReader rdr = cmd.ExecuteReader();

                if (read == true)
                {
                    int TargetColNum = -1;

                    if (returnType == QueryReturnType.ReturnDateTime)
                    {
                        TargetColNum = FindDateColNum(rdr);
                    }

                    if (returnType == QueryReturnType.ReturnLongID)
                    {
                        TargetColNum = FindBackupIDColNum(rdr);
                    }                        

                    while (rdr.Read())
                    {
                        List<string> tackon = new List<string>();

                        short ii = 0;

                        while (ii < rdr.FieldCount)
                        {
                            switch (returnType)
                            {
                                case QueryReturnType.ReturnLongID:
                                    if (ii == TargetColNum) { return EndExecutionWithLong(ref rdr, (long)rdr[ii]); }
                                    break;
                                case QueryReturnType.ReturnDateTime:
                                    if (ii == TargetColNum) { return EndExecutionWithDateTime(ref rdr, (DateTime)rdr[ii]); }
                                    break;
                                default:
                                    tackon.Add(rdr[ii].ToString());
                                    break;
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
                return new QueryResult(ret);
            }
        }

        private static QueryResult EndExecutionWithDateTime(ref MySqlDataReader rdr, DateTime dateTime)
        {
            rdr.Close();
            return new QueryResult(dateTime);
        }

        private static QueryResult EndExecutionWithLong(ref MySqlDataReader rdr, long longID)
        {
            rdr.Close();
            return new QueryResult(longID);
        }

        static private int FindBackupIDColNum(MySqlDataReader rdr)
        {
            try
            {
                return FindColNum(rdr, "BackupID");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static private int FindDateColNum(MySqlDataReader rdr)
        {
            try
            {
                return FindColNum(rdr, "WhenObserved");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static private int FindColNum(MySqlDataReader rdr, string target)
        {
            for (int ii = 0; ii < rdr.FieldCount; ii++)
            {
                if (rdr.GetName(ii) == target)
                {
                    return ii;
                }
            }

            throw new Exception($"Did not find a '{target}' column");
        }
    }

    public class QueryResult
    {
        private long? ResultAsLong;
        private DateTime ResultAsDateTime;
        private List<List<string>> ResultAsStrings;
        private byte? complete;

        public QueryResult(long resultAsLong)
        {
            this.ResultAsLong = resultAsLong;
            this.ResultAsStrings = null;

            this.complete = 1;
        }

        public QueryResult(DateTime resultAsDateTime)
        {
            this.ResultAsDateTime = resultAsDateTime;
            this.ResultAsStrings = null;

            this.complete = 1;
        }

        public QueryResult(List<List<string>> resultAsStrings)
        {
            this.ResultAsStrings = resultAsStrings;

            this.complete = 1;
        }

        public long ReturnLongResult()
        {
            if (this.complete == null || this.complete != 1) { throw new Exception("This result is incomplete!"); }
            if (ResultAsStrings != null) { throw new Exception("This result has strings!"); }

            return (long)this.ResultAsLong;
        }

        public DateTime ReturnDateTimeResult()
        {
            if (this.complete == null || this.complete != 1) { throw new Exception("This result is incomplete!"); }
            if (ResultAsStrings != null) { throw new Exception("This result has strings!"); }

            return this.ResultAsDateTime;
        }

        public List<List<string>> ReturnStringResults()
        {
            if (this.complete == null || this.complete != 1) { throw new Exception("This result is incomplete!"); }
            if (ResultAsStrings == null) { throw new Exception("This result doesn't have strings!"); }

            return this.ResultAsStrings;
        }
    }

    public enum QueryReturnType
    {
        ReturnLongID,
        ReturnDateTime,
        ReturnStrings
    }
}
