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
        static ShowLog sl = Monitor.sl;

        public static void remote(string targetDisk)
        {
            string remoteIP = "172.22.50.175";
            //string remoteIP = "172.22.100.13";
            string BOMPath = Path.Combine(targetDisk, "BOM");
            string logdir = Path.Combine(targetDisk, @"BOM\Log");
            string logBak = Path.Combine(targetDisk, "LogBak");
            string datafileDir = Path.Combine(targetDisk, @"BOM\Datafile");
            string datafileBak = @"C:\DatafileBak";

            string sourcePath = @"\\" + remoteIP + @"\2ydata\BOM";

            sl.showLog($@"开始备份Log目录->{targetDisk}LogBak");
            try
            {
                Copy.CopySth(logdir, logBak);
            }
            catch (Exception ex)
            {
                sl.showLog($"备份Log目录失败：{ex.Message}");
            }

            sl.showLog(@"开始备份Datafile目录->C:\DatafileBak");
            try
            {
                Copy.CopySth(datafileDir, datafileBak);
            }
            catch (Exception ex)
            {
                sl.showLog($"备份Datafile目录失败：{ex.Message}");
            }

            //删除本地BOM程序目录
            sl.showLog("正在删除本地BOM程序目录");
            try
            {
                Directory.Delete(BOMPath, true);
            }
            catch (Exception ex)
            {
                sl.showLog($"删除失败：{ex.Message}");
                Chkdsk.StartChkdsk(targetDisk);
            }

            retry:
            if (new Ping().Send(remoteIP).Status == IPStatus.Success)
            {
                try
                {
                    sl.showLog("开始自动拷贝BOM程序...");
                    Copy.CopySth(sourcePath, Path.Combine(targetDisk, "BOM"), "log", "datafile");
                    sl.showLog("BOM程序拷贝成功");

                    sl.showLog("开始恢复Datafile目录");
                    if (Directory.Exists(datafileBak))
                    {
                        try
                        {
                            Copy.CopySth(datafileBak, datafileDir);
                        }
                        catch (Exception ex)
                        {
                            sl.showLog($"恢复Datafile目录失败：{ex.Message}");
                        }
                        Directory.Delete(datafileBak,true);
                    }
                    else
                    {
                        sl.showLog("未找到Datafile备份目录");
                    }
                    sl.showLog("BOM程序重装完成");
                }
                catch (IOException ioex)
                {
                    sl.showLog("BOM程序拷贝失败：" + ioex.Message);
                    sl.showLog("请点击[磁盘修复]按钮修复损坏文件后再次尝试\n");
                }
                catch (Exception ex)
                {
                    sl.showLog("BOM程序拷贝失败：" + ex.Message);
                }
            }
            else
            {
                sl.showLog("与服务器连接断开！拷贝BOM程序失败");
                sl.showLog("查找原因并恢复与服务器连接后重试");
            }
        }
    }
}
