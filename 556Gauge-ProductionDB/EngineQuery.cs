using System;
using System.Collections.Generic;
using System.Text;

namespace _556Gauge_ProductionDB
{
    class EngineQuery
    {
        public string Server, User, Database, Query, Password;

        public EngineQuery(string server, string user, string database, string password)
        {
            this.Server = server; this.User = user; this.Database = database; this.Password = password;
        }

        ~EngineQuery() { }
    }
}
