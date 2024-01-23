using System;
using System.Collections.Generic;
using System.Linq;
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
        public ChangeHostWindow()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(part1, TextBox_Pasting);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn == btnOK)
            {
                if (!string.IsNullOrEmpty(tbHostname.Text))
                {
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
            ShowLog sl = Monitor.sl;
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

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            var vm = this.DataContext as IpAddressViewModel;
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String pastingText = (String)e.DataObject.GetData(typeof(String));
                vm.SetAddress(pastingText);
                part1.Focus();
                e.CancelCommand();
            }

        }

        private void Part4_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back && part4.Text == "")
            {
                part3.CaretIndex = part3.Text.Length;
                part3.Focus();
            }
            if (e.Key == Key.Left && part4.CaretIndex == 0)
            {
                part3.Focus();
                e.Handled = true;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (part4.SelectionLength == 0)
                {
                    var vm = this.DataContext as IpAddressViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }

        private void Part2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back && part2.Text == "")
            {
                part1.CaretIndex = part1.Text.Length;
                part1.Focus();
            }
            if (e.Key == Key.Right && part2.CaretIndex == part2.Text.Length)
            {
                part3.Focus();
                e.Handled = true;
            }
            if (e.Key == Key.Left && part2.CaretIndex == 0)
            {
                part1.Focus();
                e.Handled = true;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (part2.SelectionLength == 0)
                {
                    var vm = this.DataContext as IpAddressViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }

        private void Part3_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Back && part3.Text == "")
            {
                part2.CaretIndex = part2.Text.Length;
                part2.Focus();
            }
            if (e.Key == Key.Right && part3.CaretIndex == part3.Text.Length)
            {
                part4.Focus();
                e.Handled = true;
            }
            if (e.Key == Key.Left && part3.CaretIndex == 0)
            {
                part2.Focus();
                e.Handled = true;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (part3.SelectionLength == 0)
                {
                    var vm = this.DataContext as IpAddressViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }

        private void Part1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right && part1.CaretIndex == part1.Text.Length)
            {
                part2.Focus();
                e.Handled = true;
            }
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                if (part1.SelectionLength == 0)
                {
                    var vm = this.DataContext as IpAddressViewModel;
                    Clipboard.SetText(vm.AddressText);
                }
            }
        }
    }
}
