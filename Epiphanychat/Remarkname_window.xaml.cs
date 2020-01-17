using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EpiphanyChat
{
    /// <summary>
    /// Remarkname_window.xaml 的交互逻辑
    /// </summary>
    public partial class Remarkname_window : Window
    {
        public Remarkname_window()
        {
            InitializeComponent();
        }

        //确定修改
        private void Confirm_btn_click(object sender, RoutedEventArgs e)
        {
            String notename = Friend_notename.Text;
            if(notename != null)
            {
                if (NoteEvent != null)
                    NoteEvent(notename);
                this.Close();
            }
            else
            {
                MessageBox.Show("请先输入备注名", "提示");
                return;
            }
        }

        private void Cancel_btn_click(object sender, RoutedEventArgs e)
        {
            this.Close();
            return;
        }

        //定义委托 发送此添加的ID
        public delegate void MyDelegate(String Fridnotename);
        //定义事件
        public event MyDelegate NoteEvent;
    }
}
