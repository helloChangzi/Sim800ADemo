using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSapplication
{
    public class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {

            string host = "206.81.9.80";
            int port = 3306;
            string database = "vuacotuong";
            string username = "root";
            string password = "asDqwE123!@#";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }
    }
}
