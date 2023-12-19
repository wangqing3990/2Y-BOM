using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _2延线BOM运行监测系统
{
    class ResetIE
    {
        static ShowLog sl = new ShowLog();
        const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        const int INTERNET_OPTION_REFRESH = 37;

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

        static void ResetInternetExplorerSettings(object state)
        {
            
            bool result = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            result &= InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

            if (result)
                sl.showLog("Internet Explorer设置已被重置\n");
            else
                sl.showLog("Internet Explorer设置重置失败！\n");
        }

        static void ClearMyTracks()
        {
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 1"); //清除IE浏览历史
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 2"); //清除IE Cookie
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 8"); //清除IE缓存
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 16"); //清除IE临时文件
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 32"); //清除IE表单数据
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 64"); //清除IE密码
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 255");
            Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 4351");
        }

        public static void resetIE()
        {
            Task.Factory.StartNew(() =>
            {
                Process[] processes = Process.GetProcessesByName("iexplore");
                if (processes.Length == 0)
                {
                    try
                    {
                        ClearMyTracks();
                    }
                    catch (Exception ex) { }

                    ResetInternetExplorerSettings(null);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
    }
}
