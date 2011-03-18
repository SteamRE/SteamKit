using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDRUpdater
{
    public class Config : XmlSerializable<Config>
    {
        public DatabaseInfo DatabaseInfo;

        public Config()
        {
            this.DatabaseInfo = new DatabaseInfo();
        }
    }

    public class DatabaseInfo
    {
        public string Host;
        public string Database;

        public string Username;
        public string Password;
    }
}
