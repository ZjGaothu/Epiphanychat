using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Chatwindow.xaml 的交互逻辑
    /// </summary>
    public partial class Chatwindow : Window
    {
        private String Dst_ID;          //对方ID
        private String notename;    //对方的备注名
        private String IP_to_send;  //对方IP
        private const int port = 53000; //绑定端口 发送至对方的端口
        private MyP2P iP2P = null;
        private String Src_ID;     //我的ID
        private List<iMessage> chat_msg = null; //任何人发来的消息发来的消息 全局变量
        private List<iMessage> chat_msg_thisID = new List<iMessage>(); //仅对方发来的消息
        private List<String> chat_msg_thisID_string = new List<string>(); //保存信息需要String类型
        private Thread check = null;
        private const int Filemax_size = 20 * 1024 * 1024;  //文件最大容量
        private byte[] filebuffer = new byte[Filemax_size];
        private int mode = 0; //表示单人聊天0还是群聊1
        //消息记录标记 开头0代表自己 开头1代表他人


        //群聊的私有变量
        private Groupchat thegroup = null;
        private List<iMessage> group_msg = new List<iMessage>();
        private List<String> group_msg_this_string = new List<string>();//保存群聊的的内容

        public Chatwindow(String _ID,String _notename ,String _IP_to_send,String _Src_ID)
        {
            InitializeComponent();
            friend_name.Text = _notename;
            Dst_ID = _ID;
            notename = _notename;
            IP_to_send = _IP_to_send;
            Src_ID = _Src_ID;
            Frlist_box.Items.Add(Dst_ID);
            mode = 0;
            Addfriend_btn.Visibility = Visibility.Hidden;
        }

        public Chatwindow(Groupchat group , String _ID)
        {
            InitializeComponent();
            thegroup = group;
            Src_ID = _ID;
            mode = 1;
            friend_name.Text = group.ChatID;
            clearhis_btn.Visibility = Visibility.Visible;
            udp_send_btn.Visibility = Visibility.Hidden;
            member_id.Visibility = Visibility.Visible;
            id_input.Visibility = Visibility.Visible;
            int num = group.IDtosend.Count;
            for(int i = 0;i<num;i++)
            {
                String member = group.IDtosend[i].ID;
                Frlist_box.Items.Add(member);
            }
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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        //退出
        private void exit_btn_click(object sender, RoutedEventArgs e)
        {
            if(mode == 0)
            {
                check.Abort();
                check.Join();
                int num = chat_msg_thisID.Count;
                if (num > 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        if (chat_msg_thisID[i].Orient == HorizontalAlignment.Right)
                        {
                            chat_msg_thisID_string.Add("0" + chat_msg_thisID[i].Code_Message());
                        }
                        else
                        {
                            chat_msg_thisID_string.Add("1" + chat_msg_thisID[i].Code_Message());
                        }
                    }
                }
                String path = "F" + Src_ID + "T" + Dst_ID + "_msg.txt";
                System.IO.File.WriteAllText(path, string.Empty); //首先清空
                System.IO.File.WriteAllLines(path, chat_msg_thisID_string); //写入文件 保存好友聊天信息
                //当前聊天的学号中去除此学号
                if (FriendListWindow.Current_chat_ID.Count > 0)
                {
                    int n = FriendListWindow.Current_chat_ID.Count;
                    for (int i = 0; i < n; i++)
                    {
                        if (FriendListWindow.Current_chat_ID[i] == Dst_ID)
                        {
                            FriendListWindow.Current_chat_ID.Remove(FriendListWindow.Current_chat_ID[i]);
                            i = i - 1;
                            break;
                        }
                    }
                }
                chat_msg_thisID_string.Clear();
                chat_msg_thisID.Clear();
                this.Close();
            }
            else if(mode == 1)
            {
                check.Abort();
                check.Join();
                if (FriendListWindow.Current_chat_ID.Count > 0)
                {
                    int n = FriendListWindow.Current_chat_ID.Count;
                    for (int i = 0; i < n; i++)
                    {
                        if (FriendListWindow.Current_chat_ID[i] == thegroup.ChatID)
                        {
                            FriendListWindow.Current_chat_ID.Remove(FriendListWindow.Current_chat_ID[i]);
                            i = i - 1;
                            break;
                        }
                    }
                }
                int num = group_msg.Count;
                if (num > 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        if (group_msg[i].Orient == HorizontalAlignment.Right)
                        {
                            group_msg_this_string.Add("0" + group_msg[i].Code_Message());
                        }
                        else
                        {
                            group_msg_this_string.Add("1" + group_msg[i].Code_Message());
                        }
                    }
                }
                String path = "./group/message/" + "F" + Src_ID + "T" + thegroup.ChatID + "_msg.txt";
                System.IO.File.WriteAllText(path, string.Empty); //首先清空
                System.IO.File.WriteAllLines(path, group_msg_this_string); //写入文件 保存好友聊天信息
                
                this.Close();
            }
            
        }


        //发送文字
        private void Send_btn_click(object sender, RoutedEventArgs e)
        {
            //单人聊天
            if(mode == 0)
            {
                //有文字的输入时才发送
                if (send_msg_block.Text != "")
                {
                    String tex = send_msg_block.Text;
                    iMessage msg = new iMessage(tex, Src_ID, Dst_ID, DateTime.Now.ToLongTimeString().ToString());
                    iP2P = new MyP2P();
                    iP2P.SendMessage(msg, IP_to_send, port);
                    msg.Orient = HorizontalAlignment.Right;
                    Messagehistory.Items.Add(msg);
                    Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                    chat_msg_thisID.Add(msg);
                }
                send_msg_block.Text = "";
            }
            //群聊 给群里的每一个人发送消息
            if(mode == 1)
            {
                if (send_msg_block.Text != "")
                {
                    String tex = send_msg_block.Text;
                    iMessage msg = new iMessage(tex, Src_ID, Src_ID, DateTime.Now.ToLongTimeString().ToString(), thegroup.ChatID, 3);
                    //首先轮询在线的朋友
                    for (int i =0;i<thegroup.IDtosend.Count;i++)
                    {
                        iP2P = new MyP2P();
                        String member = thegroup.IDtosend[i].ID;
                        msg = new iMessage(tex, Src_ID, member, DateTime.Now.ToLongTimeString().ToString(), thegroup.ChatID, 3);
                        //不发送给自己
                        if (member != Src_ID)
                        {
                            String FriendID = "q" + member;
                            LoginCS Srch_fr = new LoginCS(FriendID);
                            String recv = Srch_fr.Connect_Server();
                            if (recv == "Ero" || recv == "Please send the correct message." || recv == "Incorrect No.")
                            {
                                MessageBox.Show("您输入的学号有问题", "提示");
                                return;
                            }
                            //如果在线则发送
                            else
                            {
                                iP2P.SendMessage(msg, recv, port);
                            }
                        }
                        
                    }
                    msg.Orient = HorizontalAlignment.Right;
                    Messagehistory.Items.Add(msg);
                    Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                    group_msg.Add(msg);

                }
                send_msg_block.Text = "";
            }
           
        }

        //窗体加载时
        private void Chat_win_loaded(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {
                String path = "F" + Src_ID + "T" + Dst_ID + "_msg.txt";
                if (File.Exists(path))
                {
                    String[] lists_mine = File.ReadAllLines(path); //读取列表
                    int num = lists_mine.Length;
                    iMessage pack = new iMessage();
                    for (int i = 0; i < num; i++)
                    {
                        String temp = lists_mine[i];
                        iMessage tmpmsg = pack.Decode_Message(lists_mine[i]);
                        if (temp[0] == '0')
                        {
                            tmpmsg.Orient = HorizontalAlignment.Right;
                        }
                        else
                        {
                            tmpmsg.Orient = HorizontalAlignment.Left;
                        }
                        chat_msg_thisID.Add(tmpmsg);
                        Messagehistory.Items.Add(tmpmsg);
                        Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                    }
                }
                chat_msg = MyP2P.MsgList;
                check = new Thread(Check_msg);
                check.Start();
            }
            else if (mode == 1)
            {
                String path = "./group/message/" + "F" + Src_ID + "T" + thegroup.ChatID + "_msg.txt";
                if (File.Exists(path))
                {
                    String[] lists_mine = File.ReadAllLines(path); //读取列表
                    int num = lists_mine.Length;
                    iMessage pack = new iMessage();
                    for (int i = 0; i < num; i++)
                    {
                        String temp = lists_mine[i];
                        iMessage tmpmsg = pack.Decode_Message(lists_mine[i]);
                        if (temp[0] == '0')
                        {
                            tmpmsg.Orient = HorizontalAlignment.Right;
                        }
                        else
                        {
                            tmpmsg.Orient = HorizontalAlignment.Left;
                        }
                        group_msg.Add(tmpmsg);
                        Messagehistory.Items.Add(tmpmsg);
                        Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                    }
                }
                chat_msg = MyP2P.MsgList;
                check = new Thread(Check_msg);
                check.Start();
            }
        }


        //循环检查消息
        private void Check_msg()
        {
            if(mode == 0)
            {
                while (true)
                {
                    int n = chat_msg.Count;
                    iMessage temp_msg = new iMessage();
                    if (n > 0)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            temp_msg = chat_msg[i];
                            String this_source = temp_msg.SrcID;
                            if (this_source == Dst_ID)
                            {   //文字消息则显示在页面
                                if (temp_msg.MsgType == 0 || temp_msg.MsgType == 121)
                                {
                                    Dispatcher.BeginInvoke(new EventShow(ReceiveShow), temp_msg);
                                    chat_msg.Remove(chat_msg[i]);
                                    i = i - 1;
                                    n = n - 1;
                                }
                            }
                        }
                    }
                    Thread.Sleep(500);
                }
            }
            if(mode == 1)
            {
                while (true)
                {
                    int n = chat_msg.Count;
                    iMessage temp_msg = new iMessage();
                    if (n > 0)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            temp_msg = chat_msg[i];
                            String this_source = temp_msg.ChatID;
                            if (this_source == thegroup.ChatID )
                            {   //文字消息则显示在页面
                                if (temp_msg.MsgType == 3)
                                {
                                    Dispatcher.BeginInvoke(new EventShow(ReceiveShow), temp_msg);
                                    chat_msg.Remove(chat_msg[i]);
                                    i = i - 1;
                                    n = n - 1;
                                }
                            }
                        }
                    }
                    Thread.Sleep(500);
                }
            }
          
        }

        //线程委托事件 显示消息
        private delegate void EventShow(iMessage a);

        private void ReceiveShow(iMessage msg)
        {
            if(msg.MsgType == 0)
            {
                chat_msg_thisID.Add(msg);
                msg.Time = DateTime.Now.ToLongTimeString().ToString();
                msg.Orient = HorizontalAlignment.Left;
                Messagehistory.Items.Add(msg);
                Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
            }
            if(msg.MsgType == 3)
            {
                group_msg.Add(msg);
                msg.Time = DateTime.Now.ToLongTimeString().ToString();
                msg.Orient = HorizontalAlignment.Left;
                Messagehistory.Items.Add(msg);
                Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
            }
            if (msg.MsgType == 121)
            {
                msg.Time = DateTime.Now.ToLongTimeString().ToString();
                msg.Orient = HorizontalAlignment.Left;
                Messagehistory.Items.Add(msg);
                Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
            }
        }


        //清除聊天记录
        private void Clearhis_btn_click(object sender, RoutedEventArgs e)
        {
            if(mode == 0)
            {
                Messagehistory.Items.Clear();
                chat_msg_thisID.Clear();
                chat_msg_thisID_string.Clear();
            }
            if(mode == 1)
            {
                Messagehistory.Items.Clear();
                group_msg.Clear();
                group_msg_this_string.Clear();
            }
        }


        //发送文件
        private async void Sendfile_btn_click(object sender, RoutedEventArgs e)
        {
            if(mode == 0)
            {
                OpenFileDialog ChoseFile = new OpenFileDialog();
                ChoseFile.Title = "选择文件";
                ChoseFile.FileName = "选择文件夹.";
                ChoseFile.FilterIndex = 1;
                ChoseFile.ValidateNames = false;
                ChoseFile.CheckFileExists = false;
                ChoseFile.CheckPathExists = true;
                bool? result = ChoseFile.ShowDialog();
                if (result != true)
                {
                    return;
                }
                else
                {
                    Array.Clear(filebuffer, 0, filebuffer.Length);
                    string filename = ChoseFile.FileName;
                    FileInfo _fileinfo = new FileInfo(filename);
                    if (_fileinfo.Length > Filemax_size)
                    {
                        MessageBox.Show("您选择的文件过大", "提示");
                        return;
                    }
                    FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                    String filepath = System.IO.Path.GetFileName(filename);
                    int fileLength = await fs.ReadAsync(filebuffer, 0, filebuffer.Length);
                    iMessage filemsg = new iMessage(filename, Src_ID, Dst_ID, DateTime.Now.ToLongTimeString().ToString(), filebuffer, fileLength, 1);
                    iP2P = new MyP2P();
                    progress.Visibility = Visibility.Visible;
                    iP2P.SendMessage(filemsg, IP_to_send, port);
                    progress.Visibility = Visibility.Hidden;
                    //显示发送消息
                    iMessage msg = filemsg;
                    msg.iText = "成功发送文件: "+ System.IO.Path.GetFileName(msg.iText);
                    msg.FileMsg = null;
                    msg.Orient = HorizontalAlignment.Right;
                    Messagehistory.Items.Add(msg);
                    Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                    chat_msg_thisID.Add(msg);
                }
            }
        }



        //添加群组成员
        private void Add_member_btn(object sender, RoutedEventArgs e)
        {
            if(id_input.Text != "")
            {
                String member = id_input.Text;
                String FriendID = "q" + member;
                LoginCS Srch_fr = new LoginCS(FriendID);
                String recv = Srch_fr.Connect_Server();
                if(recv == "Ero" || recv == "Please send the correct message." || recv == "Incorrect No.")
                {
                    MessageBox.Show("您输入的学号有问题", "提示");
                    return;
                }
                Unit mem = new Unit(member);
                if (thegroup.Obtain_one(member))
                {
                    MessageBox.Show("群组中包含这名同学", "提示");
                    return;
                }
                iP2P = new MyP2P();
                thegroup.Addfriend(mem);
                Frlist_box.Items.Add(member);
                //将其自身ID也发送
                for (int i = 0; i < thegroup.IDtosend.Count-1; i++)
                {
                    if(thegroup.IDtosend[i].ID != member)
                    {
                        //发送所有群组成员的id以及群号至当前添加的同学
                        String tex = thegroup.IDtosend[i].ID + "+" + thegroup.ChatID;
                        //发送第三类消息 添加群组
                        iMessage msg = new iMessage(tex, Src_ID, member, DateTime.Now.ToLongTimeString().ToString(), 2);
                        iP2P.SendMessage(msg, recv, port);
                    }
                    if(thegroup.IDtosend[i].ID != Src_ID)
                    {
                        String updatemsg = member + "+" + thegroup.ChatID;
                        iMessage updatemember = new iMessage(updatemsg, Src_ID, thegroup.IDtosend[i].ID, DateTime.Now.ToLongTimeString().ToString(), 2);
                        FriendID = "q" + thegroup.IDtosend[i].ID;
                        Srch_fr = new LoginCS(FriendID);
                        recv = Srch_fr.Connect_Server();
                        if (recv == "Ero" || recv == "Please send the correct message." || recv == "Incorrect No.")
                        {
                            MessageBox.Show("您输入的学号有问题", "提示");
                            return;
                        }
                        mem = new Unit(member);
                        if (thegroup.Obtain_one(member))
                        {
                            MessageBox.Show("群组中包含这名同学", "提示");
                            return;
                        }
                        iP2P.SendMessage(updatemember, recv, port);
                    }
                    
                }
            }
        }

        //改变输入框的字体大小
        private void changesize(object sender, SelectionChangedEventArgs e)
        {
            int index = sizeoftex.SelectedIndex;
            switch(index)
            {
                case 0:
                    send_msg_block.FontSize = 12;
                    break;
                case 1:
                    send_msg_block.FontSize = 14;
                    break;
                case 2:
                    send_msg_block.FontSize = 18;
                    break;
                case 3:
                    send_msg_block.FontSize = 16;
                    break;
                default:
                    break;
            }
        }

        //使用udp发送文字
        private void Sendudp_btn_click(object sender, RoutedEventArgs e)
        {
            //单人聊天
            if (mode == 0)
            {
                //有文字的输入时才发送
                if (send_msg_block.Text != "")
                {
                    String tex = send_msg_block.Text;
                    iMessage msg = new iMessage(tex, Src_ID, Dst_ID, DateTime.Now.ToLongTimeString().ToString());
                    MyUDP iUDP = new MyUDP();
                    iUDP.SendMessage(msg, IP_to_send,54000);
                    msg.Orient = HorizontalAlignment.Right;
                    Messagehistory.Items.Add(msg);
                    Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                    chat_msg_thisID.Add(msg);
                }
                send_msg_block.Text = "";
            }
            //群聊 给群里的每一个人发送消息
            if (mode == 1)
            {
                if (send_msg_block.Text != "")
                {
                    String tex = send_msg_block.Text;
                    iMessage msg = new iMessage(tex, Src_ID, Src_ID, DateTime.Now.ToLongTimeString().ToString(), thegroup.ChatID, 3);
                    //首先轮询在线的朋友
                    for (int i = 0; i < thegroup.IDtosend.Count; i++)
                    {
                        MyUDP iUDP = new MyUDP();
                        String member = thegroup.IDtosend[i].ID;
                        msg = new iMessage(tex, Src_ID, member, DateTime.Now.ToLongTimeString().ToString(), thegroup.ChatID, 3);
                        //不发送给自己
                        if (member != Src_ID)
                        {
                            String FriendID = "q" + member;
                            LoginCS Srch_fr = new LoginCS(FriendID);
                            String recv = Srch_fr.Connect_Server();
                            if (recv == "Ero" || recv == "Please send the correct message." || recv == "Incorrect No.")
                            {
                                MessageBox.Show("您输入的学号有问题", "提示");
                                return;
                            }
                            //如果在线则发送
                            else
                            {
                                
                                iUDP.SendMessage(msg, recv, port);
                            }
                        }

                    }
                    msg.Orient = HorizontalAlignment.Right;
                    Messagehistory.Items.Add(msg);
                    Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                    group_msg.Add(msg);

                }
                send_msg_block.Text = "";
            }
        }

        //发送图片
        private async void Picture_send_click(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {
                OpenFileDialog ChoseFile = new OpenFileDialog();
                ChoseFile.Title = "选择图片";
                ChoseFile.Filter = "image(*.jpg)|*.jpg|image(*.png)|*.png|image(*.bmp)|*.bmp|所有文件(*.*)|*.*";
                ChoseFile.FileName = "选择图片";
                ChoseFile.FilterIndex = 1;
                ChoseFile.ValidateNames = false;
                ChoseFile.CheckFileExists = false;
                ChoseFile.CheckPathExists = true;
                bool? result = ChoseFile.ShowDialog();
                if (result != true)
                {
                    return;
                }
                else
                {
                    Array.Clear(filebuffer, 0, filebuffer.Length);
                    string filename = ChoseFile.FileName;
                    FileInfo _fileinfo = new FileInfo(filename);
                    if (_fileinfo.Length > Filemax_size)
                    {
                        MessageBox.Show("您选择的文件过大", "提示");
                        return;
                    }
                    FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                    String filepath = System.IO.Path.GetFileName(filename);
                    int fileLength = await fs.ReadAsync(filebuffer, 0, filebuffer.Length);
                    iMessage filemsg = new iMessage(filename, Src_ID, Dst_ID, DateTime.Now.ToLongTimeString().ToString(), filebuffer, fileLength, 121);
                    iP2P = new MyP2P();
                    progress.Visibility = Visibility.Visible;
                    iP2P.SendMessage(filemsg, IP_to_send, port);
                    progress.Visibility = Visibility.Hidden;

                    filemsg.iText = "";
                    filemsg.height = "0";
                    //进行上载并重新初始化结果图片
                    filemsg.Myimage = System.IO.Path.GetFullPath(filename);
                    filemsg.Orient = HorizontalAlignment.Right;
                    Messagehistory.Items.Add(filemsg);
                    Messagehistory.ScrollIntoView(Messagehistory.Items[Messagehistory.Items.Count - 1]);
                }
            }

           
        }
    }

}
