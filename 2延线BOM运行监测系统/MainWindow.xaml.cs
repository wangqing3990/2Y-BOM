﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;
using TextBox = System.Windows.Controls.TextBox;
// using Timer = System.Threading.Timer;
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
        // public static BackgroundWorker connectServerWorker;
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static ManualResetEvent mre = new ManualResetEvent(true);
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;
        private DispatcherTimer timerSendData;//发送数据定时器

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 在这里调用,this为MainWindow实例
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            RemoveMenu(GetSystemMenu(hwnd, false), SC_CLOSE, MF_BYCOMMAND);//禁用任务栏图标右键菜单的“关闭”选项
        }
        public MainWindow()
        {
            InitializeComponent();
            publicTextBox = this.tbShowLog;

            udpClient = new UdpClient();
            string serverIp = "172.22.50.3";
            int serverPort = 26730;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            timerSendData = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timerSendData.Tick += TimerSendMethod;
            timerSendData.Start();
        }

        

        private void TimerSendMethod(object sender, EventArgs e)
        {
            string version = $"V {Assembly.GetEntryAssembly()?.GetName().Version}";
            byte[] data = Encoding.UTF8.GetBytes(version);
            // MessageBox.Show(version);
            try
            {
                udpClient.Send(data, data.Length, remoteEndPoint);
            }
            catch (Exception)
            {
            }
        }

        //加载时
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            lbVersion.Content = $"版本号：V {Assembly.GetEntryAssembly()?.GetName().Version?.ToString()}";

            getCurrentDateTimeStationNameAndEqnumber();

            monitorWorker = new BackgroundWorker();
            monitorWorker.DoWork += monitorBOM;
            monitorWorker.RunWorkerAsync(cts.Token);

            resolutionWorker = new BackgroundWorker();
            resolutionWorker.DoWork += CheckScreenResolution;
            resolutionWorker.RunWorkerAsync(cts.Token);

            updateWorker = new BackgroundWorker();
            updateWorker.DoWork += update;
            updateWorker.RunWorkerAsync(cts.Token);

            // connectServerWorker = new BackgroundWorker();
            // connectServerWorker.DoWork += connectServer;
            // connectServerWorker.RunWorkerAsync(cts.Token);
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
            // timerSendData.Stop();
            // CancelBackgroundWorkers();
            Update.update(cts);
        }

        /*private void connectServer(object sender, DoWorkEventArgs e)
        {
            Client.connectServer(cts);
        }*/
        public static void CancelBackgroundWorkers()
        {
            monitorWorker.CancelAsync();
            resolutionWorker.CancelAsync();
            // updateWorker.CancelAsync();
        }

        public static void cancelThreads()
        {
            cts.Cancel();
        }

        //每秒获取一次当前日期时间、车站名和设备号
        private Timer timer;
        private void getCurrentDateTimeStationNameAndEqnumber()
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += (s1, e1) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    try
                    {
                        lbDate.Content = DateTime.Now.ToString("yyyy-MM-dd");
                        lbTime.Content = DateTime.Now.ToString("HH:mm:ss");
                        lbStationName.Content = GetStationName.getStationName();
                        if (lbStationName.Content.ToString() == "获取失败")
                        {
                            lbStationName.Foreground = Brushes.Red;
                        }
                        else
                        {
                            lbStationName.Foreground = Brushes.DodgerBlue;
                        }

                        lbEqNumber.Content = Environment.MachineName.Substring(Math.Max(0, (Environment.MachineName.Length - 6)), 6);
                    }
                    catch (Exception) { }
                }));
            };
            timer.Start();
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
                    try
                    {
                        ResetIE.resetIE();
                    }
                    catch (Exception ex)
                    {
                        sl.showLog($"重置IE失败：{ex.Message}");
                    }
                });
            }

            if (btn == diskBtn)//磁盘修复按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        if (Directory.Exists(@"D:\"))
                        {
                            Chkdsk.StartChkdsk("D");
                        }
                        else
                        {
                            Chkdsk.StartChkdsk("C");
                        }
                    }
                    catch (Exception ex)
                    {
                        sl.showLog($"修复磁盘失败：{ex.Message}");
                    }
                });
            }

            if (btn == restartBOM)//重启BOM按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (Process.GetProcessesByName("Suzhou.APP.BOM").Length == 0)
                    {
                        Monitor.startBOM();
                    }
                    else
                    {
                        MessageBox.Show("BOM程序正在运行中！");
                    }
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
                        try
                        {
                            if (Directory.Exists(@"D:\"))
                            {
                                Remote.remote(@"D:\");
                            }
                            else
                            {
                                Remote.remote(@"C:\");
                            }
                        }
                        catch (Exception ex)
                        {
                            sl.showLog($"重装BOM程序失败：{ex.Message}");
                        }
                    }
                });
            }

            if (btn == changeHostname)//修改主机名按钮
            {
                ChangeHostWindow changeHostWindow = null;
                foreach (var window in System.Windows.Application.Current.Windows)
                {
                    if (window is ChangeHostWindow)
                    {
                        changeHostWindow = (ChangeHostWindow)window;
                        break;
                    }
                }
                if (changeHostWindow == null)
                {
                    changeHostWindow = new ChangeHostWindow();
                    changeHostWindow.Show();
                }
                else
                {
                    changeHostWindow.Activate();
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

            if (btn == clearLog)//清理过期日志按钮
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    sl.showLog("开始清理40天前的日志文件和交易数据文件...");
                    try
                    {
                        if (Directory.Exists(@"D:\"))
                        {
                            DeleteOldFiles(40, @"D:\BOM\Log", @"D:\BOM\Datafile");
                        }
                        else
                        {
                            DeleteOldFiles(40, @"C:\BOM\Log", @"C:\BOM\Datafile");
                        }

                        sl.showLog("清理结束，如果有反复删除不掉的请执行[磁盘修复]按钮");
                    }
                    catch (Exception ex)
                    {
                        sl.showLog($"清理失败：{ex.Message}");
                    }
                });
            }
        }
        //删除过期日志文件
        private void DeleteOldFiles(int days, params string[] directoryPaths)
        {
            foreach (var directoryPath in directoryPaths)
            {
                try
                {
                    DirectoryInfo directory = new DirectoryInfo(directoryPath);
                    foreach (var file in directory.GetFiles())
                    {
                        if (file.LastWriteTime < DateTime.Now.AddDays(-days))
                        {
                            file.Delete();
                            sl.showLog($"{file.FullName}已被删除");
                        }
                    }
                    foreach (var subDirectory in directory.GetDirectories())
                    {
                        DeleteOldFiles(days, subDirectory.FullName);
                    }
                }
                catch (Exception ex)
                {
                    sl.showLog($"异常：{ex.Message}");
                }
            }
        }

        //TextBox的垂直滚动条自动下移
        private void TbShowLog_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.ScrollToEnd();
        }
    }
}
