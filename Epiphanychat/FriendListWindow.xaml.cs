using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    /// FriendListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FriendListWindow : Window
    {
        //三个弹出框
        Searchfriend searchwin = null;
        Remarkname_window remarkwin = null;
        Chatwindow chatwin = null;

        private TreeViewItem select_treeitem = null;
        public static List<String> List_of_friend = new List<String>();
        public static String Username = null;
        private String select_ID = null; //当前选中的ID
        MyP2P p2p = new MyP2P();
        MyUDP udp = new MyUDP();
        private List<iMessage> msg_list = null;
        public static List<String> Current_chat_ID = new List<String>(); //当前正在聊天的人
        public static List<Groupchat> List_of_groups = new List<Groupchat>(); //群聊列表
        public int select_mode = 0; //选择0代表好友 1代表群组
        public String select_groupid = null;

        //初始化朋友列表
        public FriendListWindow(String user)
        {
            InitializeComponent();
            Username = user;
            String path = Username + "_friendlist.txt";
            if(File.Exists(path))
            {
                String[] lists_mine = File.ReadAllLines(path); //读取列表
                int num = lists_mine.Length;
                for (int i = 0; i < num; i++)
                {
                    List_of_friend.Add(lists_mine[i]);
                    Myfriend.Items.Add(lists_mine[i]);
                }
            }
            String group_path = "./group/" + Username + "_grouplist.txt";
            if(File.Exists(group_path))
            {
                String[] lists_group = File.ReadAllLines(group_path);
                int num = lists_group.Length;
                for(int i = 0;i<num;i++)
                {
                    if(lists_group[i] == "--BEGIN--A--GROUP--")
                    {
                        i++;
                        Groupchat thisgroup = new Groupchat();
                        thisgroup.ChatID = lists_group[i];
                        i++;
                        int member_num = Convert.ToInt32(lists_group[i]);
                        for(int j = 0;j<member_num;j++)
                        {
                            Unit onece = new Unit(lists_group[i + j + 1]);
                            thisgroup.Addfriend(onece);
                        }
                        List_of_groups.Add(thisgroup);
                        TreeViewItem tex = new TreeViewItem();
                        tex.Header = thisgroup.ChatID;
                        MyGroup.Items.Add(tex);
                        i = i + member_num;
                    }
                }
            }
            p2p.Startlistening();
            udp.StartListening();
            msg_list = MyP2P.MsgList;
        }



        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown += delegate { DragMove(); };
       
        }

        //将群聊列表及成员信息转成String进行保存
        private List<String> Changegroup(List<Groupchat> group)
        {
            List<String> result = new List<String>();
            int num = group.Count;
            for(int i =0;i<num;i++)
            {
                Groupchat gr = group[i];
                String id = gr.ChatID;
                String flag = "--BEGIN--A--GROUP--";
                //加入群组的标识 下面加入群组id
                result.Add(flag);
                result.Add(id);
                String length = Convert.ToString(gr.IDtosend.Count);
                result.Add(length);
                for(int j = 0; j<gr.IDtosend.Count; j++)
                {
                    result.Add(gr.IDtosend[j].ID);
                }
            }
            return result;

        }

        //退出程序
        private void Quit_btn_Click(object sender, RoutedEventArgs e)
        {
            String quit_pass = "logout" + Username;
            LoginCS mylogin = new LoginCS(quit_pass);
            String recv = mylogin.Connect_Server();
            if (recv == "loo")
            {
                String path = Username + "_friendlist.txt";
                System.IO.File.WriteAllLines(path, List_of_friend); //写入文件 保存好友列表
                //保存群组信息至本地
                List<String> group_info = Changegroup(List_of_groups);
                String group_path = "./group/" + Username + "_grouplist.txt";
                System.IO.File.WriteAllLines(group_path, group_info);
                p2p.Endlistening();
                udp.EndListening();
                this.Close();
                Environment.Exit(0);
                return;
            }
        }
    


        //查找好友
        private void Search_btn_Click(object sender, RoutedEventArgs e)
        {
            searchwin = new Searchfriend(Username);
            Application.Current.MainWindow = searchwin;
            searchwin.Show();
            searchwin.MyEvent += new Searchfriend.MyDelegate(Addfriend_Event);//监听添加框
            return;
        }

        //触发的添加好友事件 此Str为添加的好友的ID
        void Addfriend_Event(String str)
        {
            int num_of_friend = 0;
            if (List_of_friend != null)
            {
                num_of_friend = List_of_friend.Count();
            }
            //查看是否重复添加好友
            if (num_of_friend > 0)
            {
                for (int i = 0; i < num_of_friend; i++)
                {
                    if (Obtain_ID(List_of_friend[i]) == str)
                    {
                        MessageBox.Show("您已经添加了此好友", "提示");
                        return;
                    }
                }
            }
            TreeViewItem tex = new TreeViewItem();
            tex.Header = str + "(" + str + ")";
            tex.Name = "tree" + str;
            Myfriend.Items.Add(tex); //树形列表显示好友
            List_of_friend.Add(str + "(" + str + ")"); //内存列表存入好友
        }

        
        //创建群组事件
        void Addgroup_Event(String Groupid)
        {
            int num_of_group = 0;
            if (List_of_groups != null)
            {
                num_of_group = List_of_groups.Count();
            }
            //查看是否重复添加群组
            if (num_of_group > 0)
            {
                for (int i = 0; i < num_of_group; i++)
                {
                    if (List_of_groups[i].ChatID == Groupid)
                    {
                        MessageBox.Show("您已经进入了此群聊", "提示");
                        return;
                    }
                }
            }
            TreeViewItem tex = new TreeViewItem();
            tex.Header = Groupid;
            MyGroup.Items.Add(tex); //树形列表显示群组
            Groupchat thisgroup = new Groupchat();
            thisgroup.ChatID = Groupid;
            Unit temp = new Unit(Username);
            thisgroup.Addfriend(temp);
            List_of_groups.Add(thisgroup);
        }


        //选中某位好友
        private void Select_friend(object sender, RoutedEventArgs e)
        {
            select_treeitem = e.OriginalSource as TreeViewItem;
            var ID = select_treeitem.Header as String;
            //String ooo = ID.Text;
            if(ID == "我的好友")
            {
                return;
            }
            else
            {
                select_mode = 0;
                String tempid = Obtain_ID(ID);
                select_ID = tempid;
            }
           
        }

        //从备注名获取ID
        public static String Obtain_ID(String name)
        {
            int len = name.Length;
            int left = 0, right = 0;
            for(int i = 0;i<len;i++)
            {
                Char temp = name[i];
                if(temp == '(')
                {
                    left = i;
                }
                if(temp == ')')
                {
                    right = i;
                }
            }
            String thisID = name.Substring(left+1, right - left-1);
            return thisID;
        }

        //获取备注名
        public static String Obtain_notename(String name)
        {
            int len = name.Length;
            int end = 0;
            for (int i = 0; i < len; i++)
            {
                Char temp = name[i];
                if (temp == '(')
                {
                    end = i;
                }
            }
            String thisnotename = name.Substring(0, end);
            return thisnotename;

        }
        //设置备注名
        private void rename_btn_Click(object sender, RoutedEventArgs e)
        {
            var ID = select_treeitem.Header as String;
            if (select_treeitem != null && select_ID !=null && ID!="我的好友")
            {
                remarkwin = new Remarkname_window();
                remarkwin.Show();
                //添加监听
                remarkwin.NoteEvent += new Remarkname_window.MyDelegate(Remarkname_Event);
                return;
                
            }

        }
        //设置备注名事件
        void Remarkname_Event(String str)
        {
            var ID = select_treeitem.Header as String;
            select_treeitem.Header = str + "(" + select_ID + ")";
            for (int i = 0; i < List_of_friend.Count; i++)
            {
                if (List_of_friend[i] == ID)
                    List_of_friend[i] = str + "(" + select_ID + ")";
            }
        }


        //显示群聊列表
        private void Group_list_show(object sender, MouseButtonEventArgs e)
        {
            ListGroup_tree.Visibility = Visibility.Visible;
            ListFriend_tree.Visibility = Visibility.Hidden;
            MyGroup.Items.Clear();
            int num = List_of_groups.Count;
            for (int i = 0; i < num; i++)
            {
                TreeViewItem tex = new TreeViewItem();
                tex.Header = List_of_groups[i].ChatID;
                MyGroup.Items.Add(tex);
            }
        }
        //显示好友列表
        private void Friend_list_show(object sender, MouseButtonEventArgs e)
        {
            ListGroup_tree.Visibility = Visibility.Hidden;
            ListFriend_tree.Visibility = Visibility.Visible;
        }


        //选中某个群组
        private void Select_group(object sender, RoutedEventArgs e)
        {
            select_treeitem = e.OriginalSource as TreeViewItem;
            var ID = select_treeitem.Header as String;
            //String ooo = ID.Text;
            if (ID == "我的群组")
            {
                return;
            }
            else
            {
                select_mode = 1;
                select_groupid = ID;
            }
        }


        //删除好友
        private void Delete_friend_btn(object sender, RoutedEventArgs e)
        {
            var ID = select_treeitem.Header as String;
            Myfriend.Items.Clear();
            for (int i = 0; i < List_of_friend.Count; i++)
            {
                if (List_of_friend[i] == ID)
                    List_of_friend.Remove(List_of_friend[i]);
            }
            for (int i = 0; i < List_of_friend.Count; i++)
            {
                Myfriend.Items.Add(List_of_friend[i]);
            }
        }

        //弹出聊天框
        private void Chat_btn_Click(object sender, RoutedEventArgs e)
        {
            if(select_treeitem != null)
            {
                if(select_mode == 0)
                {
                    var ID = select_treeitem.Header as String;
                    String notename = Obtain_notename(ID);

                    String FriendID = "q" + select_ID;
                    LoginCS Srch_fr = new LoginCS(FriendID);
                    String recv = Srch_fr.Connect_Server();
                    if (recv == "n")
                    {
                        MessageBox.Show("对方没有在线无法聊天", "提醒");
                        return;
                    }
                    else if (recv == "Ero")
                    {
                        MessageBox.Show("与服务器连接出现错误", "提示");
                        return;
                    }
                    else
                    {
                        if (!IsChating(select_ID))
                        {
                            chatwin = new Chatwindow(select_ID, notename, recv, Username);
                            Application.Current.MainWindow = chatwin;
                            chatwin.Show();
                            Current_chat_ID.Add(select_ID);
                        }
                    }
                }
                else if(select_mode == 1)
                {
                    var ID = select_treeitem.Header as String;
                    Groupchat chat = new Groupchat();
                    for (int i = 0;i<List_of_groups.Count;i++)
                    {
                        if(ID == List_of_groups[i].ChatID)
                        {
                            chat = List_of_groups[i];
                            break;
                        }
                    }
                    if (!IsChating(ID))
                    {
                        chatwin = new Chatwindow(chat, Username);
                        Application.Current.MainWindow = chatwin;
                        chatwin.Show();
                        Current_chat_ID.Add(ID);
                    }
                }
                
            }
            
        }

        //判断是否已经打开了某个ID的聊天窗口
        public static bool IsChating(String ID)
        {
            if(Current_chat_ID.Count > 0 )
            {
                int num = Current_chat_ID.Count;
                for(int i = 0 ; i < num ; i++ )
                {
                    if(Current_chat_ID[i] == ID)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        private void Create_group_btn(object sender, RoutedEventArgs e)
        {
            Dialogwin dialog = new Dialogwin();
            Application.Current.MainWindow = dialog;
            dialog.Show();
            dialog.MyEvent += new Dialogwin.MyDelegate(Addgroup_Event);//监听添加框
            return;
        }

        //显示好友列表
        private void friend_now_cli(object sender, RoutedEventArgs e)
        {
            ListGroup_tree.Visibility = Visibility.Hidden;
            ListFriend_tree.Visibility = Visibility.Visible;
        }
        //显示群聊列表
        private void group_now_cli(object sender, RoutedEventArgs e)
        {
            ListGroup_tree.Visibility = Visibility.Visible;
            ListFriend_tree.Visibility = Visibility.Hidden;
            MyGroup.Items.Clear();
            int num = List_of_groups.Count;
            for (int i = 0; i < num; i++)
            {
                TreeViewItem tex = new TreeViewItem();
                tex.Header = List_of_groups[i].ChatID;
                MyGroup.Items.Add(tex);
            }
        }
    }
}
