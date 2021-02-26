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

            string BackupServer = "", BackupUser = "", BackupPassword = "";

            string BackupDatabase = "556prod";

            if (args.Length == 3)
            {
            }
            else if (args.Length == 0)
            {
                logger.log("Running as dev.");

                try
                {
                    BackupServer = ReadFromFile("bserver.txt");

                    logger.log("Found server name " + BackupServer + ".");

                    BackupUser = ReadFromFile("buser.txt");

                    logger.log("Found user name " + BackupUser + ".");

                    BackupPassword = ReadFromFile("bpw.txt");

                    logger.log("Found password " + BackupPassword + ".");

                    logger.log("Database name is set to " + BackupDatabase + ".");

                }
                catch (Exception ex)
                {
                    logger.log(ex.Message);
                }
            }
            else
            {
                logger.log("Utility will not run - you must enter 0 arguments (for dev) or 3 (production), you entered " + args.Length + ".");

                return;
            }

            try
            {
                MYSQLEngine mYSQLEngine = new MYSQLEngine(BackupServer, BackupUser, BackupDatabase, BackupPassword, logger);
            }
            catch (Exception ex)
            {
                logger.log(ex.Message);
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

        public void log(string log)
        {
            if (this.DisplayLog == true)
            {
                Console.WriteLine(Program.GetDateTimeString() + " -> " + log); ;
            }
        }
    }

    
}
