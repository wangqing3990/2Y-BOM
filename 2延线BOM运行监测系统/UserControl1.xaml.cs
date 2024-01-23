using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _2延线BOM运行监测系统
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void TbxIP1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            TextBox tbx = sender as TextBox;
            if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9))
            {
            }
            else if (key == Key.Delete)
            {
            }
            else if (key == Key.Enter)
            {
            }
            else if (key == Key.Back)
            {
                // 删除光标前面的数字，如果光标前没有数字，会跳转到前面一个输入框
                // 先Focus到了前面一个输入框，再执行的删除操作，所以会删除前面输入框中的一个数字
                if (tbx.CaretIndex == 0)
                {
                    SetTbxFocus(tbx, false, false);
                }
            }
            else if (key == Key.Tab)
            {
            }
            else if (key == Key.Left)
            {
                // 光标已经在当前输入框的最左边，则跳转到前面一个输入框
                if (tbx.CaretIndex == 0)
                {
                    SetTbxFocus(tbx, false, false);
                    // 得设置true，不然光标在前面一个输入框里也会移动一次
                    // 例如前一个输入框中的数字是123，Focus后，光标在数字3右边
                    // 不设置true，会移动到数字2和数字3之间
                    e.Handled = true;
                }
            }
            else if (key == Key.Right)
            {
                // 光标已经在当前输入框的最右边，则跳转到后面一个输入框
                if (tbx.SelectionStart == tbx.Text.Length)
                {
                    SetTbxFocus(tbx, true, false);
                    // true同理
                    e.Handled = true;
                }
            }
            else
            {
                // 不是上述按键，就不处理
                e.Handled = true;
            }
        }

        private void TbxIP1_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            TextBox tbx = sender as TextBox;
            if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9))
            {
                // 当前输入框满三个数字后
                // 跳转到后面一个输入框
                if (tbx.Text.Length == 3)
                {
                    if (Int32.Parse(tbx.Text) < 0 || Int32.Parse(tbx.Text) > 255)
                    {
                        tbx.Text = "255";
                        MessageBox.Show(tbx.Text + "不是有效项。请指定一个介于0和255间的值。", "错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    SetTbxFocus(tbx, true, true);
                }
            }
            else if (key == Key.Delete)
            {
                // 无操作
            }
            else if (key == Key.Enter)
            {
                // 暂时不做操作
            }
            else if (key == Key.Back)
            {
            }
            else if (key == Key.Tab)
            {
                // 暂时不做操作
            }
            else if (key == Key.Left)
            {
            }
            else if (key == Key.Right)
            {
            }
            else
            {
                // 不是上述按键，就不处理
                e.Handled = true;
            }
        }

        private void SetTbxFocus(TextBox curretTbx, bool isBack, bool isSelectAll)
        {
            // 所有的ip输入框
            List<TextBox> TbxIPList = new List<TextBox>();
            foreach (UIElement item in GridIPAddress.Children)
            {
                if (item.GetType() == typeof(TextBox))
                {
                    TbxIPList.Add(item as TextBox);
                }
            }
            // 要聚焦的输入框
            TextBox nextTbx = null;
            // 往后
            if (isBack)
            {
                // 当前输入框是前三个，那么就取后一个输入框
                int index = TbxIPList.IndexOf(curretTbx);
                if (index <= 2)
                {
                    nextTbx = TbxIPList[index + 1];
                }
            }
            // 往前
            else
            {
                // 当前输入框是后三个，那么就取前一个输入框
                int index = TbxIPList.IndexOf(curretTbx);
                if (index >= 1)
                {
                    nextTbx = TbxIPList[index - 1];
                }
            }
            // 设置焦点 全选内容
            if (nextTbx != null)
            {
                nextTbx.Focus();
                if (isSelectAll)
                {
                    nextTbx.SelectAll();
                }
            }
        }
    }
}