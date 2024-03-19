using System;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace _2延线BOM运行监测系统
{
    class Client
    {
        static ShowLog sl = new ShowLog();
        public static void connectServer(CancellationTokenSource cts)
        {
            TcpClient tcpclnt = new TcpClient();
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    sl.showLog("Connecting to server...");
                    // 连接到服务端
                    tcpclnt.Connect("172.22.50.3", 8888);
                    sl.showLog("Connected to server");
                    // 获取程序版本号
                    string version = $"V {Assembly.GetEntryAssembly()?.GetName().Version}";

                    // 创建网络流
                    NetworkStream networkStream = tcpclnt.GetStream();

                    while (true)
                    {
                        // 接收来自服务端的心跳信息
                        byte[] heartbeatBuffer = new byte[1024];
                        int bytesRead = networkStream.Read(heartbeatBuffer, 0, heartbeatBuffer.Length);
                        string heartbeatMessage = Encoding.ASCII.GetString(heartbeatBuffer, 0, bytesRead);

                        // 如果收到心跳信息，向服务端发送版本号信息
                        if (heartbeatMessage == "Heartbeat")
                        {
                            sl.showLog("Received heartbeat from server. Sending version information...");
                            byte[] versionBytes = Encoding.ASCII.GetBytes(version);
                            networkStream.Write(versionBytes, 0, versionBytes.Length);
                            networkStream.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    sl.showLog("Error: " + ex.Message);
                }
                
                Thread.Sleep(1000);
            }
            tcpclnt.Close();
        }
    }
}
