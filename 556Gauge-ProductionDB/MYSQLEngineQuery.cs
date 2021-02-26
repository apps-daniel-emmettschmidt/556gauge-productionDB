using System;
using System.Collections.Generic;
using System.Text;

namespace _556Gauge_ProductionDB
{
    class MYSQLEngineQuery
    {
        public string Server, User, Database, Query, Password;

        public MYSQLEngineQuery(string server, string user, string database, string password)
        {
            this.Server = server; this.User = user; this.Database = database; this.Password = password;
        }

        ~MYSQLEngineQuery() { }
    }
}
