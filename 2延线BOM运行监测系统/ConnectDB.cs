using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace _2延线BOM运行监测系统
{
    class ConnectDB
    {
        public static ShowLog sl = new ShowLog();
        public MySqlConnection getConn()
        {
            MySqlConnection conn;

            string server = "172.22.100.13";
            string database = "bom";
            string uid = "AFC";
            string password = "root";

            string sql = $"Server={server};Database={database};Uid={uid};Pwd={password};SslMode=None;Connection Timeout=5;";

            try
            {
                conn = new MySqlConnection(sql);
                conn.Open();
            }
            catch (MySqlException ex)
            {
                sl.showLog($"数据库连接失败：{ex.Message}");
                conn = null;
            }
            return conn;
        }
    }
}
