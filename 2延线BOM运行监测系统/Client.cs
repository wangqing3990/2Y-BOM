using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace _2延线BOM运行监测系统
{
    class Client
    {
        static ShowLog sl = new ShowLog();

        public static void connectServer(CancellationTokenSource cts)
        {
            string version = $"V {Assembly.GetEntryAssembly()?.GetName().Version}\n";
            while (!cts.IsCancellationRequested)
            {

                TcpClient clientSocket = new TcpClient();
                try
                {
                    // sl.showLog("连接服务器中...");
                    clientSocket.Connect("172.22.50.3", 49200); // 连接到服务器
                    // sl.showLog("连接服务器成功");

                    using (NetworkStream networkStream = clientSocket.GetStream())
                    using (StreamWriter writer = new StreamWriter(networkStream))
                    {
                        while (true)
                        {
                            // sl.showLog("对服务端发送版本消息");
                            string message =version;
                            writer.Write(message);
                            writer.Flush(); // 确保消息被发送
                            // networkStream.Flush();
                            // 每隔10秒发送一次消息
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // sl.showLog("连接服务器失败: " + ex.Message);
                }
                finally
                {
                    clientSocket.Close(); // 关闭连接
                    // sl.showLog("已关闭连接");
                }
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
}
