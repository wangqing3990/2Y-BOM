using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace _2延线BOM运行监测系统
{
    class ResetTPU
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObjext);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("HHJTTPU.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint TPU_OPEN(byte[] lpbytSerialnumber);
        [DllImport("HHJTTPU.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint TPU_RST(byte Mod);
        [DllImport("HHJTTPU.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint TPU_CLOSE();

        private static ShowLog sl = Monitor.sl;
        public static void resetTPU()
        {
            IntPtr handle = CreateFile("COM3", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                // 关闭串口句柄
                CloseHandle(handle);
            }
            try
            {
                TPU_OPEN(Encoding.Default.GetBytes("COM3"));
                TPU_RST(0);
                TPU_CLOSE();
            }
            catch (Exception ex)
            {
                sl.showLog($"初始化发卡失败：{ex.Message}");
            }
            finally
            {
                CloseHandle(handle);
            }
        }
    }
}
