using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace _2延线BOM运行监测系统
{
    /// <summary>
    /// MonitorLogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorLogWindow : Window
    {
        static ShowLog sl = new ShowLog();
        static string tableName = $"2ybom{DateTime.Now.ToString("yyyy")}";
        static string allSql = $"SELECT * FROM {tableName}";

        public MonitorLogWindow()
        {
            InitializeComponent();
            addComboBoxItems();
        }

        private void addComboBoxItems()
        {
            string[] stations = { "骑河", "富翔路", "尹中路", "郭巷", "郭苑路", "尹山湖", "独墅湖南", "独墅湖邻里中心", "月亮湾", "松涛街", "金谷路", "金尚路", "桑田岛", };
            foreach (var station in stations)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = station;
                cbStation.Items.Add(item);
            }

            for (int i = 1; i < 5; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = $"BOM00{i}";
                cbEq.Items.Add(item);
            }

            string[] types = { "程序闪退", "分辨率跳变" };
            foreach (var type in types)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = type;
                cbType.Items.Add(item);
            }
        }
        private void MonitorLog_OnLoaded(object sender, RoutedEventArgs e)
        {
            DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            dpStart.SelectedDate = firstDayOfMonth;
            dpEnd.SelectedDate = lastDayOfMonth;

            try
            {
                dataGrid.ItemsSource = GetBomDataFromMysql(allSql);
                Dispatcher.Invoke((Action)(() =>
                {
                    lineNumber.Content = $"行数：{dataGrid.Items.Count}";
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn == closeBtn)
            {
                this.Close();
            }

            //查询所有按钮
            if (btn == allBtn)
            {
                try
                {
                    dataGrid.ItemsSource = GetBomDataFromMysql(allSql);
                    Dispatcher.Invoke((Action)(() =>
                    {
                        lineNumber.Content = $"行数：{dataGrid.Items.Count}";
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            //条件查询按钮
            if (btn == cdnBtn)
            {
                DateTime startDate = dpStart.SelectedDate.Value;
                DateTime endDate = dpEnd.SelectedDate.Value;
                if (startDate <= endDate)
                {
                    try
                    {
                        string sql = $"SELECT * " +
                                     $"FROM {tableName} " +
                                     $"WHERE 时间 BETWEEN '{startDate.ToString("yyyy-MM-dd")}' AND '{endDate.ToString("yyyy-MM-dd")}'";

                        if (cbStation.SelectedItem != null)
                        {
                            sql += $"AND 车站 ='{cbStation.SelectionBoxItem}'";
                        }

                        if (cbEq.SelectedItem != null)
                        {
                            sql += $"AND 设备号 ='{cbEq.SelectionBoxItem}'";
                        }

                        if (cbType.SelectedItem != null)
                        {
                            if (cbType.SelectionBoxItem.Equals("程序闪退"))
                            {
                                sql += $"AND 内容 LIKE '%重启%'";
                            }
                            else if (cbType.SelectionBoxItem.Equals("分辨率跳变"))
                            {
                                sql += $"AND 内容 LIKE '%分辨率%'";
                            }
                            else if (cbType.SelectionBoxItem.Equals("程序崩溃"))
                            {
                                sql += $"AND 内容 LIKE '%盘修复%'";
                            }
                        }

                        dataGrid.ItemsSource = GetBomDataFromMysql(sql);
                        Dispatcher.Invoke((Action)(() =>
                        {
                            lineNumber.Content = $"行数：{dataGrid.Items.Count}";
                        }));
                    }
                    catch (Exception) { }
                }
                else
                {
                    MessageBox.Show("开始时间应小于等于结束时间！");
                }
            }

            //删除选中行按钮
            if (btn == delBtn)
            {
                if (dataGrid.SelectedItems.Count == 0) return;

                var result = MessageBox.Show("确定删除所有选中的行？这会同时删除数据库里的数据，请谨慎操作！", "警告", MessageBoxButtons.OKCancel,
                     MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (MySqlConnection mc = new ConnectDB().getConn())
                    {
                        if (mc == null)
                        {
                            MessageBox.Show("数据库连接失败！");
                            return;
                        }

                        try
                        {
                            List<int> delIds = new List<int>();

                            foreach (bomData item in dataGrid.SelectedItems)
                            {
                                int id = item.序号;
                                delIds.Add(id);
                            }

                            var source = dataGrid.ItemsSource as List<bomData>;

                            foreach (var id in delIds)
                            {
                                bomData itemToRemove = source.FirstOrDefault(x => x.序号 == id);
                                if (itemToRemove != null)
                                {
                                    source.Remove(itemToRemove);
                                }
                            }
                            dataGrid.Items.Refresh();

                            string[] idsArray = delIds.Select(id => id.ToString()).ToArray();
                            string delSql = $"DELETE FROM {tableName} WHERE 序号 IN ({string.Join(",", idsArray)});";

                            if (delIds.Count > 0)
                            {
                                using (MySqlCommand cmd = new MySqlCommand(delSql, mc))
                                {
                                    cmd.CommandTimeout = 3;
                                    cmd.ExecuteNonQuery();
                                    sl.showLog("删除成功2");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"删除失败：{ex}");
                        }
                        finally
                        {
                            mc.Close();
                        }
                    }
                }
            }
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void comBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                comboBox.SelectedIndex = -1;
            }
        }

        public List<bomData> GetBomDataFromMysql(string sql)
        {
            List<bomData> data = new List<bomData>();

            using (MySqlConnection mc = new ConnectDB().getConn())
            {
                if (mc == null)
                {
                    MessageBox.Show("数据库连接失败！");
                    return null;
                }

                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, mc))
                    {
                        cmd.CommandTimeout = 3;
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            bomData bomData = new bomData();
                            bomData.序号 = (int)reader["序号"];
                            bomData.时间 = (DateTime)reader["时间"];
                            bomData.车站 = reader["车站"].ToString();
                            bomData.设备号 = reader["设备号"].ToString();
                            bomData.内容 = reader["内容"].ToString();
                            data.Add(bomData);
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"获取数据失败：{ex.Message}");
                }
                finally
                {
                    mc.Close();
                }
            }
            return data;
        }
    }

    public class bomData
    {
        public int 序号 { get; set; }
        public DateTime 时间 { get; set; }
        public string 车站 { get; set; }
        public string 设备号 { get; set; }
        public string 内容 { get; set; }
    }
}
