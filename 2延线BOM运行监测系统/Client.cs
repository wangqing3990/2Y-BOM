using System;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace _2延线BOM运行监测系统
{
    class Client
    {
        static ShowLog sl = new ShowLog();
        private static TcpClient tcpclnt = null;
        private static readonly TimeSpan sendInterval = TimeSpan.FromSeconds(10);
        private static DateTime lastSendTime = DateTime.MinValue;

        public static void connectServer(CancellationTokenSource cts)
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    if (tcpclnt == null || !tcpclnt.Connected)
                    {
                        ConnectToServer();
                    }

                    sl.showLog(tcpclnt.Connected.ToString());

                    if ((DateTime.Now - lastSendTime) > sendInterval && tcpclnt.Connected)
                    {
                        SendVersionInformation();
                        lastSendTime = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    sl.showLog("Error1: " + ex.Message);
                }
                Thread.Sleep(1000);
            }
        }

        private static void ConnectToServer()
        {
            try
            {
                sl.showLog("连接服务器中...");
                tcpclnt = new TcpClient();
                tcpclnt.Connect("172.22.50.3", 8888);
                sl.showLog("连接服务器成功");
                return;
            }
            catch (Exception ex)
            {
                sl.showLog("连接服务器失败: " + ex.Message);
                if (tcpclnt != null)
                {
                    tcpclnt.Close();
                    tcpclnt = null;
                }
            }
        }

        private static void SendVersionInformation()
        {
            try
            {
                string version = $"V {Assembly.GetEntryAssembly()?.GetName().Version}\n";

                tcpclnt.NoDelay = true;

                /*using (NetworkStream networkStream = tcpclnt.GetStream())
                {*/
                    NetworkStream networkStream = tcpclnt.GetStream();

                    sl.showLog("Sending version information...");
                    byte[] versionBytes = Encoding.ASCII.GetBytes(version);
                    networkStream.Write(versionBytes, 0, versionBytes.Length);
                    networkStream.Flush();
                // }
            }
            catch (Exception ex)
            {
                sl.showLog("Error2: " + ex.Message);
                if (tcpclnt != null)
                {
                    tcpclnt.Close();
                    tcpclnt = null;
                }
            }
            
        }
    }
}
