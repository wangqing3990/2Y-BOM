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
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        const int MF_BYCOMMAND = 0x00000000;
        const int SC_CLOSE = 0xF060;

        public static void set()
        {
            //控制台窗口宽高
            Console.SetWindowSize(80, 35);

            //注册表设置开机自启动
            RegistryKey registry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            registry.SetValue("2y_BOM运行监测", Application.ExecutablePath);
            registry.SetValue("BOM", @"D:\BOM\Suzhou.APP.Update.exe");
            registry.Close();

            //设置禁用任务管理器
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
            key.Close();

            //删除关闭菜单项
            IntPtr consoleHandle = GetConsoleWindow();
            IntPtr systemMenuHandle = GetSystemMenu(consoleHandle, false);
            DeleteMenu(systemMenuHandle, SC_CLOSE, MF_BYCOMMAND);
        }
    }
}
