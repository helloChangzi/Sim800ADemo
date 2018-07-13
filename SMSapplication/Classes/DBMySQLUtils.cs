using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSapplication
{
    public class DBMySQLUtils
    {
        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            // Connection String.
            String connString = "SERVER=" + host + ";DATABASE=" + database
                + ";PORT=" + port + ";UID=" + username + ";PASSWORD=" + password + ";";

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }
    }
}
