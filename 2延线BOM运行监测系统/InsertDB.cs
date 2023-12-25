using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace _2延线BOM运行监测系统
{
    class InsertDB
    {
        static ShowLog sl = ConnectDB.sl;
        public static void insertDB(string str)
        {
            string tableName = $"2ybom{DateTime.Now.ToString("yyyy")}";

            using (MySqlConnection mc = new ConnectDB().getConn())
            {
                try
                {
                    if (!tableExists(mc, tableName))
                        createNewTable(mc, tableName);
                }
                catch (Exception) { }

                string insertQuery = $"INSERT INTO {tableName} (时间,车站,设备号,内容) VALUES(@时间,@车站,@设备号,@内容)";

                using (MySqlCommand cmd = new MySqlCommand(insertQuery, mc))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@车站", GetStationName.getStationName());
                        cmd.Parameters.AddWithValue("@设备号", Environment.MachineName.Substring(Math.Max(0, Environment.MachineName.Length - 6), 6));
                        cmd.Parameters.AddWithValue("@内容", str);

                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        sl.showLog($"写入日志失败：{ex.Message}");
                    }
                }
            }
        }
        static bool tableExists(MySqlConnection mc, string tableName)
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                cmd.Connection = mc;
                cmd.CommandText = " SHOW TABLES LIKE @tableName";
                cmd.Parameters.AddWithValue("@tableName", tableName);

                object result = cmd.ExecuteScalar();
                return result != null;
            }
        }

        static void createNewTable(MySqlConnection mc, string tableName)
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                cmd.Connection = mc;
                cmd.CommandText = $"CREATE TABLE {tableName} (" +
                                      "序号 INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                                      "时间 DATETIME NULL DEFAULT NULL," +
                                      "车站 VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci'," +
                                      "设备号 VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci'," +
                                      "内容 TEXT NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci'" +
                                      ")"; ;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
