using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace _2延线BOM运行监测系统
{
    class Update
    {
        static ShowLog sl = new ShowLog();
        private static readonly string updateServerPath = @"\\172.22.100.13\2ydata\BOMUpdate\";
        //private static readonly string updateServerPath = @"\\172.22.50.175\2ydata\BOMUpdate\";

        public static void update(CancellationTokenSource cts)
        {
            //cts = MainWindow.cts;
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(10 * 1000);
                //Thread.Sleep(10 * 60 * 1000);
                try
                {
                    string latestVersionPath = Path.Combine(updateServerPath, "version.txt");
                    string latestVersionStr = File.ReadAllText(latestVersionPath);

                    Version latestVersion = new Version(latestVersionStr);
                    Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                    if (latestVersion > currentVersion)
                    {
                        sl.showLog("检测到有新版本可用，即将退出本程序执行更新操作");
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            try
                            {
                                ProcessStartInfo psi = new ProcessStartInfo
                                {
                                    FileName = "BOMUpdate.exe",
                                    CreateNoWindow = true,
                                    UseShellExecute = false,
                                    RedirectStandardOutput = false,
                                    RedirectStandardError = false
                                };
                                Process updaterProcess = new Process { StartInfo = psi };
                                updaterProcess.Start();
                            }
                            catch (Exception) { }

                            MainWindow.cancelThreads();
                            Environment.Exit(0);
                        });
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
