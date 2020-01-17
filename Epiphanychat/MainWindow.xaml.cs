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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace EpiphanyChat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
 

        }
       


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }



        //使其可移动窗口
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown += delegate { DragMove(); };
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // Begin dragging the window
            this.DragMove();
        }


        //退出程序
        private void ext_btn_click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        //登录键
        private void Login_btn_click(object sender, RoutedEventArgs e)
        {
            String User = Username.Text;
            //获取密码
            IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.PasswordBox.SecurePassword);
            String Pass = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);
            if(Pass != "net2019")
            {
                MessageBox.Show("密码错误", "提醒");
                return;
            }
            String login_pass = User + "_" + Pass;
            LoginCS mylogin = new LoginCS(login_pass);
            String recv = mylogin.Connect_Server();
            if(recv == "lol")
            {
                FriendListWindow friendwin = new FriendListWindow(Username.Text);
                Application.Current.MainWindow = friendwin;
                this.Close();
                friendwin.Show();
                return;
            }
            else if(recv == "Ero")
            {
                MessageBox.Show("与服务器连接发生错误", "错误");
                return;
            }
            
        }
    }
}
