using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace _2延线BOM运行监测系统
{
    class ShowLog
    {
        public void showLog(string text)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
           {
               MainWindow.publicTextBox.AppendText($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {text}{Environment.NewLine}");
           }));

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    if (Directory.Exists(@"D:\"))
                    {
                        File.AppendAllText(@"D:\LocalLog.txt", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {text}{Environment.NewLine}");
                    }
                    else
                    {
                        File.AppendAllText(@"C:\LocalLog.txt", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {text}{Environment.NewLine}");
                    }
                }
                catch (Exception)
                {
                    
                }
            });
        }
    }
}
