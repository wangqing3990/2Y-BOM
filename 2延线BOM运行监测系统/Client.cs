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
        public static void connectServer(CancellationTokenSource cts)
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        if (tcpclnt == null || !tcpclnt.Connected) // 检查连接状态
                        {
                            if (tcpclnt != null)
                            {
                                tcpclnt.Close(); // 关闭旧连接
                            }
                            tcpclnt = new TcpClient();
                            sl.showLog("连接服务器中...");
                            tcpclnt.Connect("172.22.50.3", 8888);
                            sl.showLog("连接服务器成功");
                        }

                        // 获取程序版本号
                        string version = $"V {Assembly.GetEntryAssembly()?.GetName().Version}";

                        // 创建网络流
                        NetworkStream networkStream = tcpclnt.GetStream();
                        if (networkStream != null)
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
                        sl.showLog("Error1: " + ex.Message);
                    }
                    finally
                    {
                        Thread.Sleep(1000); // 延迟一秒再进行下一次循环
                    }
                }
            }
            catch (Exception ex)
            {
                sl.showLog("Error2: " + ex.Message);
            }
            finally
            {
                if (tcpclnt != null && tcpclnt.Connected)
                {
                    tcpclnt.Close(); // 关闭连接
                }
            }
        }
    }
}
