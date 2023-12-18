using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _2延线BOM运行监测系统
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            getCurrentDateTime();
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            lbVersion.Content = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        }
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
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn=sender as Button;
            if (btn == minBtn || btn == closeBtn)
            {
                WindowState = WindowState.Minimized;
            }
            if (btn==ieBtn)
            {
                ResetIE.resetIE();
            }
            if (btn==diskBtn)
            {
                if (Directory.Exists(@"D:\"))
                    Chkdsk.StartChkdsk("D");
                else
                    Chkdsk.StartChkdsk("C");
            }
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            Button btn = (Button) sender;
            if (btn == minBtn|| btn == closeBtn)
            {
                btn.BorderBrush = Brushes.Gold;
                btn.BorderThickness=new Thickness(1);
            }
            else
            {
                btn.Background = Brushes.DodgerBlue;
                btn.FontSize = 15;
                btn.Foreground = Brushes.White;
            }
        }

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
                btn.Foreground=Brushes.Black;
            }
        }

        
    }
}
