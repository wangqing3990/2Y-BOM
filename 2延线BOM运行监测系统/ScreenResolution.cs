﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace _2延线BOM运行监测系统
{
    class ScreenResolution
    {
        //定义结构体，用于存储分辨率信息
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;

            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;

            public int dmDisplayFlags;
            public int dmDisplayFrequency;

            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;

            public int dmPanningWidth;
            public int dmPanningHeight;
        };

        //定义常量，用于指定要设置的参数
        const int ENUM_CURRENT_SETTINGS = -1; //获取当前分辨率
        const int CDS_UPDATEREGISTRY = 0x01; //更新注册表
        const int CDS_TEST = 0x02; //测试模式，不实际设置
        const int DISP_CHANGE_SUCCESSFUL = 0; //设置成功
        const int DISP_CHANGE_RESTART = 1; //需要重启
        const int DISP_CHANGE_FAILED = -1; //设置失败


        //定义API函数，用于获取和设置分辨率
        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        [DllImport("user32.dll")]
        static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

        //[DllImport("user32.dll")]
        //static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        //private static bool isShown = false;

        //定义监测函数
        static void monitoring(int i, ref DEVMODE devMode, int width, int height, Screen[] screens)
        {

            if (EnumDisplaySettings(screens[i - 1].DeviceName, ENUM_CURRENT_SETTINGS, ref devMode)) //获取当前分辨率
            {
                //if (!isShown)
                //{
                //    Console.WriteLine($"副屏{i}的当前分辨率为：{devMode.dmPelsWidth} x {devMode.dmPelsHeight}");
                //    isShown = true;
                //}

                if (devMode.dmPelsWidth != width || devMode.dmPelsHeight != height) //如果分辨率跳变，就尝试修改
                {
                    Console.WriteLine(DateTime.Now + $" 监测到副屏{i}的分辨率跳变！");
                    //Console.WriteLine(DateTime.Now + $" 尝试将副屏{i}的分辨率修改为：{width} x {height}");
                    devMode.dmPelsWidth = width;
                    devMode.dmPelsHeight = height;
                    int result = ChangeDisplaySettingsEx(screens[i - 1].DeviceName, ref devMode, IntPtr.Zero, CDS_TEST, IntPtr.Zero); //测试是否可以修改
                    if (result == DISP_CHANGE_FAILED) //如果失败，提示用户
                    {
                        Console.WriteLine(DateTime.Now + $" 无法修改副屏{i}的分辨率");
                    }
                    else //如果成功，更新注册表并提示用户
                    {
                        ChangeDisplaySettingsEx(screens[i - 1].DeviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
                        if (result == DISP_CHANGE_RESTART) //如果需要重启，提示用户
                        {
                            Console.WriteLine(DateTime.Now + " 修改成功，请重启电脑以应用更改");
                        }
                        else //如果不需要重启，提示用户
                        {
                            Console.WriteLine(DateTime.Now + $" 副屏{i}的分辨率还原成功\n");
                            ThreadPool.QueueUserWorkItem(state =>
                            {
                                try
                                {
                                    string dir = @"\\172.22.100.13\2ydata\2yBOMLog\2yBOMMonitorLog";
                                    if (!Directory.Exists(dir))
                                        Directory.CreateDirectory(dir);
                                    string resolutionLogPath = Path.Combine(dir, $"{DateTime.Today.ToString("yyMMdd")}.txt");
                                    if (!File.Exists(resolutionLogPath))
                                        using (File.Create(resolutionLogPath)) { }

                                    File.AppendAllText(resolutionLogPath, $"{DateTime.Now} {GetStationName.getStationName()}{Environment.MachineName}的副屏{i}的分辨率跳变并修复成功" + Environment.NewLine);
                                }
                                catch (Exception) { }
                            });
                        }
                    }
                }
            }
            //else //如果无法获取当前分辨率，提示用户
            //{
            //    Console.WriteLine(DateTime.Now + $" 无法获取副屏{i}的当前分辨率");
            //}
        }

        private static int ChangeDisplaySettingsEx(ref DEVMODE devMode, int cDS_TEST)
        {
            throw new NotImplementedException();
        }

        public static void CheckScreen()
        {
            Screen[] screens = Screen.AllScreens; //获取所有屏幕对象
            DEVMODE devMode = new DEVMODE(); //创建DEVMODE对象
            devMode.dmSize = (short)Marshal.SizeOf(devMode); //设置结构体大小
            Console.WriteLine(DateTime.Now + " 显示器分辨率监测中...\n");
            while (true)
            {
                if (screens.Length == 1)
                {
                    monitoring(1, ref devMode, 1280, 1024, screens);
                }
                else if (screens.Length == 2)
                {
                    monitoring(1, ref devMode, 1280, 1024, screens);
                    monitoring(2, ref devMode, 1024, 768, screens);
                }
                else if (screens.Length == 3)
                {
                    monitoring(1, ref devMode, 1280, 1024, screens);
                    monitoring(2, ref devMode, 1024, 768, screens);
                    monitoring(3, ref devMode, 1024, 768, screens);
                }
                Thread.Sleep(1000);
            }
        }
    }
}
