using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Timers.Timer;

namespace _2延线BOM运行监测系统
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static ShowLog sl = new ShowLog();
        public static TextBox publicTextBox;
        public static BackgroundWorker monitorWorker;
        public static BackgroundWorker resolutionWorker;
        public static BackgroundWorker updateWorker;
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static ManualResetEvent mre = new ManualResetEvent(true);
        public MainWindow()
        {
            InitializeComponent();
            publicTextBox = this.tbShowLog;
        }

        //加载时
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            lbVersion.Content = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            lbStationName.Content = GetStationName.getStationName();
            lbEqNumber.Content = Environment.MachineName.Substring(Math.Max(0, (Environment.MachineName.Length - 6)), 6);

            getCurrentDateTime();

            monitorWorker = new BackgroundWorker();
            monitorWorker.DoWork += monitorBOM;
            monitorWorker.RunWorkerAsync(cts.Token);

            resolutionWorker = new BackgroundWorker();
            resolutionWorker.DoWork += CheckScreenResolution;
            resolutionWorker.RunWorkerAsync(cts.Token);

            updateWorker=new BackgroundWorker();
            updateWorker.DoWork += update;
            updateWorker.RunWorkerAsync(cts.Token);
        }

        private void monitorBOM(object sender, DoWorkEventArgs e)
        {
            Monitor.MonitorBOM(cts, mre);
        }

        private void CheckScreenResolution(object sender, DoWorkEventArgs e)
        {
            ScreenResolution.CheckScreen(cts);
        }

        private void update(object sender, DoWorkEventArgs e)
        {
            Update.update(cts);
        }
        public static void CancelBackgroundWorkers()
        {
            monitorWorker.CancelAsync();
            resolutionWorker.CancelAsync();
            updateWorker.CancelAsync();
        }

        public static void cancelThreads()
        {
            cts.Cancel();
        }
        //显示当前日期时间
        private void getCurrentDateTime()
        {
            Timer t = new Timer();
            t.Interval = 1000;
            t.Elapsed += (s1, e1) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    lbDate.Content = DateTime.Now.ToString("yyyy-MM-dd");
                    lbTime.Content = DateTime.Now.ToString("HH:mm:ss");
                }));
            };
            t.Start();
        }
        //顶部鼠标按下移动窗口
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        //按钮点击事件
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn == minBtn || btn == closeBtn)//右上角最小化和关闭按钮
            {
                WindowState = WindowState.Minimized;
            }
            if (btn == ieBtn)//重置IE按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    ResetIE.resetIE();
                });
            }
            if (btn == diskBtn)//磁盘修复按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (Directory.Exists(@"D:\"))
                        Chkdsk.StartChkdsk("D");
                    else
                        Chkdsk.StartChkdsk("C");
                });
            }
            if (btn == restartBOM)//重启BOM按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (Process.GetProcessesByName("Suzhou.APP.BOM").Length == 0)
                        Monitor.startBOM();
                    else
                        MessageBox.Show("BOM程序正在运行中！");
                });
            }
            if (btn == reinstallBOM)//重装BOM按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    DialogResult result = MessageBox.Show("即将重装BOM程序！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        if (Directory.Exists(@"D:\"))
                            Remote.remote(@"D:\");
                        else
                            Remote.remote(@"C:\");
                    }
                });
            }
            if (btn == stopMonitor)//暂停监测按钮
            {
                if (btn.Content.Equals("暂停监测"))
                {
                    btn.Content = "恢复监测";
                    Monitor.sl.showLog("暂停监测BOM进程");
                    Monitor.monitorMre.Reset();
                }
                else if (btn.Content.Equals("恢复监测"))
                {
                    Monitor.sl.showLog("恢复监测BOM进程");
                    Monitor.monitorMre.Set();
                    btn.Content = "暂停监测";
                }
            }
            if (btn == initTPU)//初始化发卡按钮
            {
                sl.showLog("开始执行初始化发卡模块");
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        ResetTPU.resetTPU();
                        sl.showLog("初始化完成");
                    }
                    catch (Exception ex)
                    {
                        sl.showLog($"初始化发卡模块失败：{ex.Message}");
                    }
                });
            }
            if (btn == clearLog)//清理过期日志按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    sl.showLog("开始清理30天前的日志文件和交易数据文件...");
                    if (Directory.Exists(@"D:\"))
                        DeleteFilesOlderThanOneMonth(@"D:\BOM\Log", @"D:\BOM\Datafile");
                    else
                        DeleteFilesOlderThanOneMonth(@"C:\BOM\Log", @"C:\BOM\Datafile");
                    sl.showLog("清理结束，如果有反复删除不掉的请执行[磁盘修复]按钮");
                });
            }
            if (btn == monitorLogBtn)//监测日志按钮
            {

                MonitorLogWindow monitorLogWindow = null;
                foreach (var window in System.Windows.Application.Current.Windows)
                {
                    if (window is MonitorLogWindow)
                    {
                        monitorLogWindow = (MonitorLogWindow)window;
                        break;
                    }
                }
                if (monitorLogWindow == null)
                {
                    monitorLogWindow = new MonitorLogWindow();
                    monitorLogWindow.Show();
                }
                else
                {
                    monitorLogWindow.Activate();
                }
            }
        }
        //删除过期日志文件
        private void DeleteFilesOlderThanOneMonth(params string[] directoryPaths)
        {
            foreach (var directoryPath in directoryPaths)
            {
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(directoryPath);
                    foreach (var file in directory.GetFiles())
                    {
                        if (file.CreationTime < DateTime.Now.AddMonths(-1))
                        {
                            file.Delete();
                            sl.showLog($"{file.FullName}已被删除");
                        }
                    }
                    foreach (var subDirectory in directory.GetDirectories())
                    {
                        DeleteFilesOlderThanOneMonth(subDirectory.FullName);
                    }
                }
                catch (Exception ex)
                {
                    sl.showLog($"异常：{ex.Message}");
                }
            }
        }
        //鼠标移入事件
        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn == minBtn || btn == closeBtn)
            {
                btn.BorderBrush = Brushes.Goldenrod;
                btn.BorderThickness = new Thickness(1);
            }
            else
            {
                btn.Background = Brushes.DodgerBlue;
                if (btn == clearLog)
                    btn.FontSize = 12;
                else
                    btn.FontSize = 15;
                btn.Foreground = Brushes.White;
            }
        }
        //鼠标移出事件
        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn == minBtn || btn == closeBtn)
            {
                btn.BorderBrush = null;
            }
            else
            {
                btn.Background = Brushes.Goldenrod;
                if (btn == clearLog)
                    btn.FontSize = 11;
                else
                    btn.FontSize = 13;
                btn.Foreground = Brushes.Black;
            }
        }

    }
}
