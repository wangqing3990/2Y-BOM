using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace _2延线BOM运行监测系统
{
    class GetStationName
    {
        private class NetworkInfo
        {
            public string IPAddress { get; set; } = "0.0.0.0";
        }

        private static NetworkInfo GetLocalNetworkInfo()
        {
            NetworkInfo networkInfo = new NetworkInfo();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            networkInfo.IPAddress = ip.Address.ToString();
                        }
                    }
                }
                if (!networkInfo.IPAddress.Equals("0.0.0.0")) break;
            }
            return networkInfo;
        }
        public static string getStationName()
        {
            var octets = GetLocalNetworkInfo().IPAddress.Split('.');
            if (octets.Length > 3)
            {
                string station = octets[2];
                switch (station)
                {
                    case "19":
                        return "骑河";
                    case "20":
                        return "富翔路";
                    case "43":
                        return "尹中路";
                    case "44":
                        return "郭巷";
                    case "45":
                        return "郭苑路";
                    case "46":
                        return "尹山湖";
                    case "47":
                        return "独墅湖南";
                    case "48":
                        return "独墅湖邻里中心";
                    case "49":
                        return "月亮湾";
                    case "50":
                        return "松涛街";
                    case "51":
                        return "金谷路";
                    case "52":
                        return "金尚路";
                    case "53":
                        return "桑田岛";
                    default:
                        return "获取失败";
                }
            }
            return "获取失败";
        }
    }
}
