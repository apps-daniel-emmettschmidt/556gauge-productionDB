using System;
using System.Collections.Generic;

using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace _556Gauge_ProductionDB
{
    class Program
    {        

        static void Main(string[] args)
        {
            Logger logger = new Logger(true);

            logger.log("Logger constructed.");

            string ProductionServer = "", ProductionUser = "", ProductionPassword = "";

            string ProductionDatabase = "556prod";

            string BackupServer = "", BackupUser = "", BackupPassword = "";

            string BackupDatabase = "prices";

            if (args.Length == 6)
            {
                logger.log("Running as production.");

                ProductionServer = args[0];

                logger.log("Found production server name " + ProductionServer + ".");

                ProductionUser = args[1];

                logger.log("Found production user name " + ProductionUser + ".");

                ProductionPassword = args[2];

                logger.log("Found production password " + ProductionPassword + ".");

                logger.log("Production database name is set to " + ProductionDatabase + ".");

                BackupServer = args[3];

                logger.log("Found backup server name " + BackupServer + ".");

                BackupUser = args[4];

                logger.log("Found backup user name " + BackupUser + ".");

                BackupPassword = args[5];

                logger.log("Found backup password " + BackupPassword + ".");

                logger.log("Backup database name is set to " + BackupDatabase + ".");
            }
            else if (args.Length == 0)
            {
                logger.log("Running as dev.");

                try
                {
                    ProductionServer = ReadFromFile("pserver.txt");

                    logger.log("Found production server name " + ProductionServer + ".");

                    ProductionUser = ReadFromFile("puser.txt");

                    logger.log("Found production user name " + ProductionUser + ".");

                    ProductionPassword = ReadFromFile("ppw.txt");

                    logger.log("Found production password " + ProductionPassword + ".");

                    logger.log("Production database name is set to " + ProductionDatabase + ".");

                    BackupServer = ReadFromFile("bserver.txt");

                    logger.log("Found backup server name " + BackupServer + ".");

                    BackupUser = ReadFromFile("buser.txt");

                    logger.log("Found backup user name " + BackupUser + ".");

                    BackupPassword = ReadFromFile("bpw.txt");

                    logger.log("Found backup password " + BackupPassword + ".");

                    logger.log("Backup database name is set to " + BackupDatabase + ".");

                }
                catch (Exception ex)
                {
                    logger.log(ex.Message);
                }
            }
            else
            {
                logger.log("Utility will not run - you must enter 0 arguments (for dev) or 6 (production), you entered " + args.Length + ".");

                return;
            }
                       

            try
            {
                MYSQLEngine mySQLEngine = new MYSQLEngine(ProductionServer, ProductionUser, ProductionDatabase, ProductionPassword, logger);

                SQLServerEngine sqlServerEngine = new SQLServerEngine(BackupServer, BackupDatabase, BackupUser, BackupPassword, logger);

                mySQLEngine.InsertPriceRows(sqlServerEngine.GetRowsSinceCutoff(mySQLEngine.CutoffDate));
            }
            catch (Exception ex)
            {
                logger.log(ex.Message);

                return;
            }
        }

        static string ReadFromFile(string filename)
        {
            string FileName = ImprovedGetAssembly() + "\\" + filename;

            return System.IO.File.ReadAllText(FileName);
        }

        public static string GetDateTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
        }

        static string ImprovedGetAssembly()
        {
            string start = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).CodeBase);

            if (start.Contains("file:\\file:") == true)
            {
                start = start.Replace("file:\\file:", "");
            }

            if (start.Contains("file:\\") == true)
            {
                start = start.Replace("file:\\", "");
            }

            return start;
        }

    }

    

    class Logger
    {
        bool DisplayLog;
        
        public Logger(bool display)
        {
            this.DisplayLog = display;
        }

        public string log(string log)
        {
            string ret = Program.GetDateTimeString() + " -> " + log;

            if (this.DisplayLog == true)
            {
                Console.WriteLine(ret); ;
            }

            return ret;
        }
    }

    
}
