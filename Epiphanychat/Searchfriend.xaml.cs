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
    /// Searchfriend.xaml 的交互逻辑
    /// </summary>
    public partial class Searchfriend : Window
    {
        private String myID = null;
        private int Frien_State = 0; //对方是否在线 0未查找 1在线 2不在线 3错误学号
        private String Friend_ID_now = null;
        private Chatwindow chatwin = null;
        private String Friend_IP_now = null;
        public Searchfriend(String str)
        {
            InitializeComponent();
            myID = str;
        }

        private void Cancel_btn_click(object sender, RoutedEventArgs e)
        {
            this.Close();
            return;
        }


        //查找好友
        private void Search_btn_click(object sender, RoutedEventArgs e)
        {
            Friend_ID_now = null;
            if(Friend_ID.Text == myID)
            {
                MessageBox.Show("您查找的为您自己", "提示");
                return;
            }
            else if(Friend_ID == null)
            {
                MessageBox.Show("请先输入要查找的学号", "提示");
                return;
            }
            String FriendID = "q" + Friend_ID.Text;
            LoginCS Srch_fr = new LoginCS(FriendID);
            String recv = Srch_fr.Connect_Server();
            if(recv == "n")
            {
                Text_condi1.Visibility = Visibility.Visible;
                Text_condi2.Text = "不在线";
                Text_condi2.Visibility = Visibility.Visible;
                Text_condi3.Visibility = Visibility.Hidden;
                Frien_State = 2;
                Friend_ID_now = Friend_ID.Text;
                Friend_IP_now = null;
                return;
            }
            else if (recv == "Ero")
            {
                MessageBox.Show("与服务器连接出现错误", "提示");
                Friend_IP_now = null;
                return;
            }
            else if (recv == "Please send the correct message.")
            {
                MessageBox.Show("请输入正确的学号", "提示");
                Frien_State = 3;
                Friend_IP_now = null;
                return;
            }
            else if (recv == "Incorrect No.")
            {
                Text_condi1.Visibility = Visibility.Visible;
                Text_condi2.Text = "该学号不在课程范围";
                Text_condi2.Visibility = Visibility.Visible;
                Text_condi3.Visibility = Visibility.Hidden;
                Frien_State = 3;
                Friend_IP_now = null;
                return;
            }
            else
            {
                Text_condi1.Visibility = Visibility.Visible;
                Text_condi2.Text = "在线";
                Text_condi2.Visibility = Visibility.Visible;
                Text_condi3.Text = "对方IP：" + recv;
                Text_condi3.Visibility = Visibility.Visible;
                Frien_State = 1;
                Friend_ID_now = Friend_ID.Text;
                Friend_IP_now = recv;
                return;
            }
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


        //发起聊天按钮
        private void Begin_chat_btn_click(object sender, RoutedEventArgs e)
        {
           if(Frien_State == 1)
           {
                if(!FriendListWindow.IsChating(Friend_ID_now))
                {
                    chatwin = new Chatwindow(Friend_ID_now, Friend_ID_now, Friend_IP_now, myID);
                    Application.Current.MainWindow = chatwin;
                    chatwin.Show();
                    FriendListWindow.Current_chat_ID.Add(Friend_ID_now);
                }
                
           }
           else if(Frien_State == 0)
            {
                MessageBox.Show("请先进行查找", "提示");
                return;
            }
           else if(Frien_State == 2)
            {
                MessageBox.Show("对方不在线", "提示");
                return;
            }
        }

        //当输入新的ID时 当前对方状态为未查找
        private void input_change(object sender, TextChangedEventArgs e)
        {
            Frien_State = 0;
            Text_condi1.Visibility = Visibility.Hidden;
            Text_condi2.Visibility = Visibility.Hidden;
            Text_condi3.Visibility = Visibility.Hidden;
        }
        //添加好友
        private void Add_btn_click(object sender, RoutedEventArgs e)
        {
            //查找的学号合法再进行添加操作
            if(Frien_State == 1||Frien_State == 2)
            {   //触发原窗口的添加事件
                if (MyEvent != null)
                    MyEvent(Friend_ID_now);
                this.Close();
            }
            else if(Frien_State == 3)
            {
                MessageBox.Show("此学号不正确，不能添加好友", "提示");
                return;
            }
            else if (Frien_State == 0)
            {
                MessageBox.Show("请先查找欲添加的好友", "提示");
                return;
            }
        }

        //定义委托 发送此添加的ID
        public delegate void MyDelegate(String FridID);
        //定义事件
        public event MyDelegate MyEvent;
    }
}
