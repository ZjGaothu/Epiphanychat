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
    /// Dialogwin.xaml 的交互逻辑
    /// </summary>
    public partial class Dialogwin : Window
    {
        public Dialogwin()
        {
            InitializeComponent();
        }


        private void Reject_btn_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Confirm_btn_click(object sender, RoutedEventArgs e)
        {
            String id = Textbox.Text;
            if (MyEvent != null)
                MyEvent(id);
            this.Close();

        }

        //无边框时拖动窗口
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown += delegate { DragMove(); };
        }

        //定义委托 发送此添加的ID
        public delegate void MyDelegate(String groupID);
        //定义事件
        public event MyDelegate MyEvent;
    }
}
