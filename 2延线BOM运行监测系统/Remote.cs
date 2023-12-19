using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace _2延线BOM运行监测系统
{
    class Remote
    {
        static ShowLog sl = new ShowLog();
        public static void remote(string targetPath, string disk)
        {
            
            //string remoteIP = "172.22.50.11";
            string remoteIP = "172.22.100.13";

            string sourcePath = @"\\" + remoteIP + @"\2ydata\BOM";

            retry:
            if (new Ping().Send(remoteIP).Status == IPStatus.Success)
            {
                try
                {
                    sl.showLog(DateTime.Now + " 开始自动拷贝BOM程序...");
                    Copy.CopySth(sourcePath, targetPath, new string[] { "log", "datafile" });
                    sl.showLog(DateTime.Now + " BOM程序拷贝成功\n");
                }
                catch (IOException ioex)
                {
                    sl.showLog(DateTime.Now + " BOM程序拷贝失败：" + ioex.Message);
                    sl.showLog(DateTime.Now + " 开始进行磁盘修复\n");
                    Chkdsk.StartChkdsk(disk);
                    goto retry;
                }
                catch (Exception ex)
                {
                    sl.showLog(DateTime.Now + " BOM程序拷贝失败：" + ex.Message);
                    sl.showLog(DateTime.Now + " 按Enter键继续");
                    Console.ReadLine();
                }
            }
            else
            {
                sl.showLog(DateTime.Now + " 与服务器连接断开！拷贝BOM程序失败");
                sl.showLog(DateTime.Now + " 查找原因并恢复与服务器连接后按Enter键重试");
                Console.ReadLine();
                goto retry;
            }
        }
    }
}
