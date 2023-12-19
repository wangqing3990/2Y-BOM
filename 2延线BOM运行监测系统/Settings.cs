using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace _2延线BOM运行监测系统
{
    class Settings
    {
        public static void set()
        {
            //注册表设置开机自启动
            RegistryKey registry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            registry.SetValue("2y_BOM运行监测", Application.ExecutablePath);
            registry.SetValue("BOM", @"D:\BOM\Suzhou.APP.Update.exe");
            registry.Close();

            //设置禁用任务管理器
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
            key.Close();

        }
    }
}
