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
        private static TcpClient tcpclnt;
        private static readonly TimeSpan reconnectInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan sendInterval = TimeSpan.FromSeconds(30);
        private static DateTime lastSendTime = DateTime.MinValue;

        public static void connectServer(CancellationTokenSource cts)
        {
            while (!cts.IsCancellationRequested)
            {
                if (tcpclnt == null)
                {
                    tcpclnt = new TcpClient();
                }

                sl.showLog(tcpclnt.Connected.ToString());

                if (!tcpclnt.Connected)
                {
                    ConnectToServer();
                    Thread.Sleep(reconnectInterval);
                }
                else
                {
                    if ((DateTime.Now - lastSendTime) > sendInterval)
                    {
                        try
                        {
                            SendVersionInformation();
                            lastSendTime = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            sl.showLog("Error: " + ex.Message);
                            tcpclnt.Close();
                            tcpclnt = null;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private static void ConnectToServer()
        {
            try
            {
                sl.showLog("连接服务器中...");
                tcpclnt.Connect("172.22.50.3", 8888);
                sl.showLog("连接服务器成功");
            }
            catch (Exception ex)
            {
                sl.showLog("连接服务器失败: " + ex.Message);
                tcpclnt.Close();
                tcpclnt = null;
            }
        }

        private static void SendVersionInformation()
        {
            string version = $"V {Assembly.GetEntryAssembly()?.GetName().Version}";

            tcpclnt.NoDelay = true;
            NetworkStream networkStream = tcpclnt.GetStream();
            sl.showLog("Sending version information...");

            byte[] versionBytes = Encoding.ASCII.GetBytes(version);
            networkStream.Write(versionBytes, 0, versionBytes.Length);
            networkStream.Flush();
        }
    }
}
