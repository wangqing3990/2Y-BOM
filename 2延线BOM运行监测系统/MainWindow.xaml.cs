using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Timer = System.Timers.Timer;

namespace _2延线BOM运行监测系统
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static TextBox publicTextBox;
        public static BackgroundWorker monitorWorker;
        public static BackgroundWorker resolutionWorker;
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();
            getCurrentDateTime();
            //一些注册表/菜单项等设置
            Settings.set();

            //删除启动文件夹下的启动程序快捷方式
            StartUpDelete.startUpDelete();
        }
        //加载时
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            publicTextBox = this.tbShowLog;
            lbVersion.Content = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            lbStationName.Content = GetStationName.getStationName();
            lbEqNumber.Content = Environment.MachineName.Substring(Math.Max(0, (Environment.MachineName.Length - 6)), 6);

            monitorWorker=new BackgroundWorker();
            monitorWorker.DoWork += monitorBOM;
            monitorWorker.RunWorkerAsync(cts.Token);

            resolutionWorker = new BackgroundWorker();
            resolutionWorker.DoWork+= CheckScreenResolution;
            resolutionWorker.RunWorkerAsync(cts.Token);
        }

        private void monitorBOM(object sender, DoWorkEventArgs e)
        {
            Monitor.MonitorBOM(cts);
        }

        private void CheckScreenResolution(object sender,DoWorkEventArgs e)
        {
            ScreenResolution.CheckScreen();
        }
        public static void CancelBackgroundWorkers()
        {
            monitorWorker.CancelAsync();
            resolutionWorker.CancelAsync();
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
                ResetIE.resetIE();
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
        }
        //鼠标移入事件
        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn == minBtn || btn == closeBtn)
            {
                btn.BorderBrush = Brushes.Gold;
                btn.BorderThickness = new Thickness(1);
            }
            else
            {
                btn.Background = Brushes.DodgerBlue;
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
                btn.Background = Brushes.Gold;
                btn.FontSize = 13;
                btn.Foreground = Brushes.Black;
            }
        }


    }
}
