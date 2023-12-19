using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace _2延线BOM运行监测系统
{
    class Chkdsk
    {
        static ShowLog sl = new ShowLog();
        public static void StartChkdsk(string disk)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/C echo Y & echo Y & echo Y | chkdsk {disk}: /F /X /R";// 指定要执行的参数为 D: /F /X /R，表示修复磁盘 D 上的错误，尝试从坏扇区中恢复可读信息,并强制卸载该磁盘

            try
            {
                process.Start();

                process.WaitForExit();

                int exitCode = process.ExitCode;// 获取 Process 对象的退出代码
                sl.showLog(exitCode.ToString());//调试

                // 判断退出代码为0，表示 chkdsk 命令成功执行
                if (exitCode == 0)
                {
                    sl.showLog($"磁盘 {disk} 修复成功\n");
                }
                else if (exitCode == 50 || exitCode == 3) // 判断退出代码是否为 50或3，表示需要在下次启动时才能修复错误
                {
                    sl.showLog("磁盘 {disk} 需要在下次启动时才能修复错误,即将重启系统");
                    Process.Start("shutdown.exe", "/r /t 5 /f /d p:4:1 /c \"重启系统\"");
                    Thread.Sleep(10 * 1000);
                }
            }
            catch (Exception ex)
            {
                sl.showLog($"执行 chkdsk 命令时发生错误：{ex.Message}\n");
            }
            finally
            {
                process.Close();
            }
        }
    }
}
