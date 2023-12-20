using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace _2延线BOM运行监测系统
{
    class Monitor
    {
        private static bool isShown = false;//只显示BOM正在运行一次
        public static ShowLog sl = new ShowLog();
        public static ManualResetEvent monitorMre;
        private static string ExtractFilePathFromErrorMessage(string errorMessage)
        {
            // 错误信息的格式为：文件或目录\BOM\Log\CCM\20230410.log已损坏且无法读取。请运行Chkdsk工具。
            string pattern = @"文件或目录\\(.+?)已损坏且无法读取(.+)";// 正则表达式
            Match match = Regex.Match(@errorMessage, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                string filePath = @"D:\" + match.Groups[1].Value;
                return filePath;
            }
            return null; // 提取失败
        }

        private const string BOM = "Suzhou.APP.BOM";
        public static void MonitorBOM(CancellationTokenSource cts, ManualResetEvent mre)
        {
            cts = MainWindow.cts;
            monitorMre = mre;
            Thread.Sleep(60 * 1000);//启动程序后等待1分钟再开始监测
            TimeSpan start = new TimeSpan(23, 30, 0);
            TimeSpan end = new TimeSpan(4, 0, 0);
            bool repeat = false;

            while (!cts.IsCancellationRequested)
            {
                monitorMre.WaitOne();

                var processes = Process.GetProcessesByName(BOM);
                if (processes.Length == 0)
                {
                    //23:00-4:00暂停监测
                    if (DateTime.Now.TimeOfDay >= start || DateTime.Now.TimeOfDay <= end)
                    {
                        if (!repeat)
                        {
                            sl.showLog($"夜间23：00-4：00暂停监测，如需启动BOM程序，点击[重启BOM]按钮{Environment.NewLine}");
                            repeat = true;
                        }
                        Thread.Sleep(1000);
                        continue;
                    }
                    //4:00-6:00和22:30-23:30自动重启BOM程序
                    else if ((DateTime.Now.TimeOfDay > end && DateTime.Now.TimeOfDay < new TimeSpan(6, 0, 0)) || (DateTime.Now.TimeOfDay >= new TimeSpan(22, 30, 0)) && DateTime.Now.TimeOfDay < start)
                    {
                        sl.showLog("BOM程序自动重启中,请等待...");
                        startBOM();
                    }
                    //其他时间段人工重启
                    else
                    {
                        DialogResult result = MessageBox.Show("监测到BOM程序退出，是否需要启动BOM程序？", "提示", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == DialogResult.Yes)
                        {
                            sl.showLog("BOM程序启动中,请等待...");
                            startBOM();
                        }
                        else if (result == DialogResult.No)
                        {
                            sl.showLog("暂停BOM运行监测5分钟");
                            Thread.Sleep(5 * 60 * 1000);
                            sl.showLog("恢复监测");
                        }
                    }
                }
                else
                {
                    if (!isShown)
                    {
                        sl.showLog("BOM程序正在运行中，进入监测状态\n");
                        isShown = true;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private static string BOMDPath = @"D:\BOM";
        private static string BOMCPath = @"C:\BOM";
        public static void startBOM()
        {
            if (Directory.Exists(@"D:\"))
            {
                startBOMUpdate(BOMDPath, BOMDPath + Path.Combine(@"\Suzhou.APP.Update.exe"), "D");
                delay1Min(BOMDPath, "D");
            }
            else
            {
                startBOMUpdate(BOMCPath, BOMCPath + Path.Combine(@"\Suzhou.APP.Update.exe"), "C");
                delay1Min(BOMCPath, "C");
            }
        }

        static void startBOMUpdate(string BOMPath, string BOMExePath, string disk)
        {
            try
            {
                Process.Start(BOMExePath);
            }
            catch (Win32Exception we)
            {
                sl.showLog($"BOM启动失败：{we.Message}");
                Remote.remote(Path.GetDirectoryName(BOMPath)); //远程拷贝BOM
                sl.showLog("BOM程序启动中,请等待...");
                Process.Start(BOMExePath);
            }
            catch (Exception) { }
        }

        static void messageShow()
        {
            MessageBox.Show("下面的弹窗请点击【是】", "提示", MessageBoxButtons.OK,
                                MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.DefaultDesktopOnly);
        }

        static void delay1Min(string path, string disk)
        {
            AutoResetEvent timerAre;
            int count = 0;//查找到的损坏文件个数
            timerAre = new AutoResetEvent(false);

            //程序启动后等待60秒
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = TimeSpan.FromSeconds(60).TotalMilliseconds;
            timer.AutoReset = false;
            timer.Elapsed += (s, e) =>
            {
                if (Process.GetProcessesByName(BOM).Length > 0)
                {
                    sl.showLog("BOM程序启动成功\n");
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        try
                        {
                            string directoryPath = @"\\172.22.100.13\2ydata\2yBOMLog\2yBOMMonitorLog";
                            if (!Directory.Exists(directoryPath))
                                Directory.CreateDirectory(directoryPath);

                            string eryanBOMMonitorLogPath = Path.Combine(directoryPath, $"{DateTime.Today.ToString("yyMMdd")}.txt");
                            if (!File.Exists(eryanBOMMonitorLogPath))
                                using (File.Create(eryanBOMMonitorLogPath)) { }
                            string stationName = GetStationName.getStationName();

                            File.AppendAllText(eryanBOMMonitorLogPath,
                                $"{DateTime.Now} {stationName}{Environment.MachineName}程序退出并成功重启" + Environment.NewLine);
                        }
                        catch (Exception) { }
                    });
                }
                else
                {
                    DialogResult result = MessageBox.Show("BOM程序未能成功启动，是否为人为退出？", "信息", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                    if (result == DialogResult.Yes)
                    {
                        sl.showLog("人为退出BOM程序，暂停运行监测5分钟\n");
                        Thread.Sleep(5 * 60 * 1000);
                        sl.showLog("恢复监测");
                    }
                    else if (result == DialogResult.No)
                    {
                        sl.showLog("BOM程序启动失败！开始扫描是否有损坏文件");

                        // 定义一个事件日志源
                        string eventLogSource = "Application"; // 这里使用了“应用程序”日志
                        // 创建一个事件日志实例
                        EventLog eventLog = new EventLog(eventLogSource);
                        // 遍历最新的事件条目
                        foreach (EventLogEntry entry in eventLog.Entries)
                        {
                            if (entry.EntryType == EventLogEntryType.Warning)
                            {
                                string filePath = ExtractFilePathFromErrorMessage(entry.Message);//匹配正则表达式
                                if (!string.IsNullOrEmpty(filePath))//如果提取到损坏文件路径
                                {
                                    if (File.Exists(filePath))//如果损坏文件还存在
                                    {
                                        count++;//损坏文件个数加1
                                        try
                                        {
                                            sl.showLog("找到并删除损坏文件:{filePath}");
                                            File.Delete(filePath);//尝试删除损坏文件
                                        }
                                        catch (Exception ex)
                                        {
                                            sl.showLog($"删除损坏文件:{filePath}失败：{ex.Message}。开始进行磁盘修复\n");
                                            Chkdsk.StartChkdsk(disk);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (count == 0)
                        {
                            sl.showLog("未找到损坏文件，是否重装BOM程序\n");
                            DialogResult dr = MessageBox.Show("未找到损坏文件，是否重装BOM程序", "提示",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.DefaultDesktopOnly);
                            if (dr == DialogResult.Yes)
                            {
                                try
                                {
                                    Directory.Delete(path, true);
                                    messageShow();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(DateTime.Now + $" 删除{path} 失败：{ex.Message}。开始进行磁盘修复\n");
                                    Chkdsk.StartChkdsk(disk);
                                    messageShow();
                                }
                            }
                            else
                            {
                                sl.showLog("即将再次尝试重启BOM程序");
                            }
                        }
                    }

                }
                timerAre.Set();
            };
            timer.Start();
            timerAre.WaitOne();
        }

    }
}
