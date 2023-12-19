using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace _2延线BOM运行监测系统
{
    class ShowLog
    {
        public void showLog(string text)
        {
            Application.Current.Dispatcher.Invoke((Action) (() =>
            {
                MainWindow.publicTextBox.AppendText($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {text}{Environment.NewLine}");
            }));
        }
    }
}
