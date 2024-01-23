﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;

namespace _2延线BOM运行监测系统
{
    /// <summary>
    /// ChangeHostWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeHostWindow : Window
    {
        ShowLog sl = Monitor.sl;
        public ChangeHostWindow()
        {
            InitializeComponent();
            uc_ip.TbxIP1.TextChanged += (s, e) => { uc_gateway.TbxIP1.Text = uc_ip.TbxIP1.Text.ToString(); };
            uc_ip.TbxIP2.TextChanged += (s, e) => { uc_gateway.TbxIP2.Text = uc_ip.TbxIP2.Text.ToString(); };
            uc_ip.TbxIP3.TextChanged += (s, e) => { uc_gateway.TbxIP3.Text = uc_ip.TbxIP3.Text.ToString(); };
            uc_ip.TbxIP4.TextChanged += (s, e) => { uc_gateway.TbxIP4.Text = "254"; };
        }

        private void ChangeHostWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            uc_subnetmask.TbxIP1.Text = uc_subnetmask.TbxIP2.Text = uc_subnetmask.TbxIP3.Text = "255";
            uc_subnetmask.TbxIP4.Text = "0";
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn == btnOK)
            {
                if (!string.IsNullOrEmpty(tbHostname.Text))
                {
                    if (!string.IsNullOrEmpty(uc_ip.TbxIP1.Text) && !string.IsNullOrEmpty(uc_ip.TbxIP2.Text) && !string.IsNullOrEmpty(uc_ip.TbxIP3.Text) && !string.IsNullOrEmpty(uc_ip.TbxIP4.Text))
                    {
                        //MessageBox.Show("请输入完整的IP地址");
                        try
                        {
                            changeIP();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"修改IP地址出错：{ex.Message}");
                        }
                    }

                    try
                    {
                        changeHostname();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"修改计算机名出错：{ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("请输入要修改的主机名！");
                }
            }

            if (btn == btnCancel)
            {
                this.Close();
            }
        }

        private void changeHostname()
        {

            RegistryKey key1 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", true);
            key1.SetValue("ComputerName", tbHostname.Text, RegistryValueKind.String);
            key1.Close();
            RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName", true);
            key2.SetValue("ComputerName", tbHostname.Text, RegistryValueKind.String);
            key2.Close();
            RegistryKey key3 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true);
            key3.SetValue("Hostname", tbHostname.Text, RegistryValueKind.String);
            key3.SetValue("NV Hostname", tbHostname.Text, RegistryValueKind.String);
            key3.Close();
            sl.showLog($"计算机名已被修改为：{tbHostname.Text}");
        }

        private void changeIP()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface networkInterface = interfaces[0];
            string interfaceName = networkInterface.Name;
            ProcessStartInfo ps = new ProcessStartInfo();
            ps.FileName = "cmd.exe";
            ps.Arguments =
            /*$"/c netsh interface ip set address \"{interfaceName}\" static {Convert.ToInt32(uc_ip.TbxIP1.Text)}.{Convert.ToInt32(uc_ip.TbxIP2.Text)}.{Convert.ToInt32(uc_ip.TbxIP3.Text)}.{Convert.ToInt32(uc_ip.TbxIP4.Text)} " +
            $"{Convert.ToInt32(uc_subnetmask.TbxIP1.Text)}.{Convert.ToInt32(uc_subnetmask.TbxIP2.Text)}.{Convert.ToInt32(uc_subnetmask.TbxIP3.Text)}.{Convert.ToInt32(uc_subnetmask.TbxIP4.Text)} " +
            $"{Convert.ToInt32(uc_gateway.TbxIP1.Text)}.{Convert.ToInt32(uc_gateway.TbxIP2.Text)}.{Convert.ToInt32(uc_gateway.TbxIP3.Text)}.{Convert.ToInt32(uc_gateway.TbxIP4.Text)} 1";*/
            $"/c netsh interface ip set address \"{interfaceName}\" static {uc_ip.TbxIP1.Text}.{uc_ip.TbxIP2.Text}.{uc_ip.TbxIP3.Text}.{uc_ip.TbxIP4.Text} " +
                $"{uc_subnetmask.TbxIP1.Text}.{uc_subnetmask.TbxIP2.Text}.{uc_subnetmask.TbxIP3.Text}.{uc_subnetmask.TbxIP4.Text} " +
                $"{uc_gateway.TbxIP1.Text}.{uc_gateway.TbxIP2.Text}.{uc_gateway.TbxIP3.Text}.{uc_gateway.TbxIP4.Text} 1";
            ps.CreateNoWindow = true;
            ps.UseShellExecute = false;
            ps.RedirectStandardOutput = true;
            ps.RedirectStandardError = true;
            using (Process p = Process.Start(ps))
            {
                p.WaitForExit();
                p.Close();
                /*sl.showLog($"IP地址已被修改为：{Convert.ToInt32(uc_ip.TbxIP1.Text)}.{Convert.ToInt32(uc_ip.TbxIP2.Text)}.{Convert.ToInt32(uc_ip.TbxIP3.Text)}.{Convert.ToInt32(uc_ip.TbxIP4.Text)}");
                sl.showLog($"子网掩码已被修改为：{Convert.ToInt32(uc_subnetmask.TbxIP1.Text)}.{Convert.ToInt32(uc_subnetmask.TbxIP2.Text)}.{Convert.ToInt32(uc_subnetmask.TbxIP3.Text)}.{Convert.ToInt32(uc_subnetmask.TbxIP4.Text)}");
                sl.showLog($"默认网关已被修改为：{Convert.ToInt32(uc_gateway.TbxIP1.Text)}.{Convert.ToInt32(uc_gateway.TbxIP2.Text)}.{Convert.ToInt32(uc_gateway.TbxIP3.Text)}.{Convert.ToInt32(uc_gateway.TbxIP4.Text)}");*/
                sl.showLog($"IP地址已被修改为：{uc_ip.TbxIP1.Text}.{uc_ip.TbxIP2.Text}.{uc_ip.TbxIP3.Text}.{uc_ip.TbxIP4.Text}");
                sl.showLog($"子网掩码已被修改为：{uc_subnetmask.TbxIP1.Text}.{uc_subnetmask.TbxIP2.Text}.{uc_subnetmask.TbxIP3.Text}.{uc_subnetmask.TbxIP4.Text}");
                sl.showLog($"默认网关已被修改为：{uc_gateway.TbxIP1.Text}.{uc_gateway.TbxIP2.Text}.{uc_gateway.TbxIP3.Text}.{uc_gateway.TbxIP4.Text}");
            }
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


    }
}
