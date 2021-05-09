using System;
using System.Collections.Generic;
using System.Text;

namespace _556Gauge_ProductionDB
{
    class MYSQLEngineQuery
    {
        public MYSQLEngineConnectionParameters ConnectionParameters;

        public string Query;

        public MYSQLEngineQuery(MYSQLEngineConnectionParameters connectionParameters)
        {
            Initialize(connectionParameters, "");
        }

        public MYSQLEngineQuery(MYSQLEngineConnectionParameters connectionParameters, string query)
        {
            Initialize(connectionParameters, query);
        }

        public MYSQLEngineQuery(string server, string user, string database, string password)
        {
            Initialize(new MYSQLEngineConnectionParameters(server, user, database, password), "");
        }

        public MYSQLEngineQuery(string server, string user, string database, string password, string query)
        {
            Initialize(new MYSQLEngineConnectionParameters(server, user, database, password), query);
        }

        private void Initialize(MYSQLEngineConnectionParameters connectionParameters, string query)
        {
            this.ConnectionParameters = connectionParameters;
        }

        ~MYSQLEngineQuery() { }
    }
    class MYSQLEngineConnectionParameters
    {
        public string Server, User, Database, Query, Password;

        public MYSQLEngineConnectionParameters(string server, string user, string database, string password)
        {
            this.Server = server; this.User = user; this.Database = database; this.Password = password;
        }

        public string BuildConnectionString()
        {
            return "server="
                    + this.Server
                    + ";user="
                    + this.User
                    + ";database="
                    + this.Database
                    + ";port=3306;password="
                    + this.Password;
        }
    }

}
