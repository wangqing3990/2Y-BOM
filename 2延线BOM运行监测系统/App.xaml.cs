using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;

namespace _2延线BOM运行监测系统
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = new Mutex(true, "{300B8DD8-2188-471F-8979-7E00F1AEA9AA}");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!_mutex.WaitOne(TimeSpan.Zero,true))
            {
                MessageBox.Show("程序已经在运行了！");
                Current.Shutdown();
            }

            //一些注册表/菜单项等设置
            Settings.set();
            //删除启动文件夹下的启动程序快捷方式
            StartUpDelete.startUpDelete();

            
        }

    }
}
