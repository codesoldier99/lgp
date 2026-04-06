using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Data.OleDb;

namespace IndustryDemo.Controllerui
{
    class MySqlOperate
    {
        protected static String dbServer = "127.0.0.1";
        protected static String dbUser = "root";
        protected static String dbPwd = "12345";
        protected static String dbName = "filterdetectiondatabase";


        private MySqlConnection getConn()
        { return null; }

    }
}
