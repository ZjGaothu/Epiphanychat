using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;

namespace EpiphanyChat
{
    /// <summary>
    /// P2P模块：对等方连接和开启侦听 等待接收 以及发送消息
    /// </summary>
    class MyP2P
    {
        private TcpListener tlListen;
        private bool listenerRun = true; //是否接收连接请求
        private TcpClient Tcpchat = null;
        private const int port = 53000;
        public static List<iMessage> MsgList = new List<iMessage>();
        public static int isreceive = 0;
        public static List<Groupchat> List_of_groups = FriendListWindow.List_of_groups;
        public static int addgroup_flag = 0; //是否有加入群组的消息到来 
        Thread Recvmsg = null;
        public MyP2P()
        {

        }



        //获取本机IP
        public IPAddress GetIP()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipentry = Dns.GetHostEntry(hostName);
            for (int i = 0; i < ipentry.AddressList.Length; i++)
            {

                if (ipentry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipentry.AddressList[i];
                }
            }
            return null;
        }
        //开始监听
        public void Startlistening()
        {
            try
            {
                tlListen = new TcpListener(GetIP(), port);
                tlListen.Start();
                Recvmsg = new Thread(Wait_recv);
                Recvmsg.Start(); 
            }
            catch(Exception e)
            {
                MessageBox.Show( e.Message , "提示");
            }
        }


        //发送消息
        public void SendMessage(iMessage imsg, String send_IP, int send_Port)
        {
            String message = imsg.Code_Message();
            byte[] Data = new byte[0];
            if(imsg.MsgType == 0 && imsg.MsgType == 2 && imsg.MsgType == 3)
            {
                //Data = Encoding.Unicode.GetBytes(message);
                Data = imsg.Code_Mesaage_file();
            }
            else
            {
                Data = imsg.Code_Mesaage_file();
            }
            Tcpchat = new TcpClient();
            try
            {
                Tcpchat.Connect(send_IP, send_Port);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "和对方连接失败");
                return;
            }
            using (NetworkStream stream = Tcpchat.GetStream())
            {
                try
                {
                    stream.Write(Data, 0, Data.Length);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "流写入错误");
                    return;
                }
                stream.Close();
            }
            Tcpchat.Close();
        }



        //线程的委托 等待接收
        public async void Wait_recv()
        {
            while (true)
            {   //接受消息
                if (tlListen.Pending())
                {
                    TcpClient client = null;
                    int bufferSize = 22 * 1024 * 1024;
                    client = tlListen.AcceptTcpClient();
                    client.ReceiveBufferSize = bufferSize;
                    byte[] recvBuffer = new byte[bufferSize];
                    using (NetworkStream myDatastream = client.GetStream())
                    {
                        int once = 1024 * 1024;
                        int datalength = 0;
                        byte[] bytes = new byte[once];
                        if (myDatastream.CanRead)
                        {
                            do
                            {
                                int read_length = 0;
                                try
                                {   //得到实际读出的长度
                                    read_length = myDatastream.Read(bytes, 0, once);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message, "流读出错误");
                                }
                                Buffer.BlockCopy(bytes, 0, recvBuffer, datalength, read_length);
                                datalength += read_length; //偏移量修改
                                Thread.Sleep(100);
                            } while (myDatastream.DataAvailable);
                        }
                        myDatastream.Close();
                        byte[] msg = new byte[datalength];
                        Buffer.BlockCopy(recvBuffer, 0, msg, 0, msg.Length);
                        string str = Encoding.Unicode.GetString(recvBuffer);
                        iMessage msgonce = new iMessage();
                        iMessage msgdecode = msgonce.Decode_Message(str);
                        iMessage msgdecode_file = msgonce.Decode_Message_file(msg);
                        if(msgdecode.MsgType == 0)
                        {
                            MsgList.Add(msgdecode);
                            msgdecode_file.Orient = HorizontalAlignment.Left;
                            //对不是正在聊天的朋友的消息进行提示
                            if (!FriendListWindow.IsChating(msgdecode.SrcID) && msgdecode.SrcID != FriendListWindow.Username)
                            {
                                String source = msgdecode.SrcID;
                                if(IsMyFriend(source))
                                {
                                    String note = Obtain_note(source);
                                    MessageBox.Show("您的好友"+note +"发来了一条消息", "提示");
                                }
                                else
                                {
                                    MessageBox.Show("账号" + source + "发来了一条消息", "提示");
                                }
                            }
                        }
                        else if(msgdecode_file.MsgType == 1) //接受文件
                        {
                            String fromid = msgdecode_file.SrcID;
                            String name = Path.GetFileName(msgdecode_file.iText);
                            MessageBoxResult save = MessageBox.Show("您收到了来自" + fromid + "的文件\"" + name + "\",是否接收？","接受文件提醒", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                            if (save == MessageBoxResult.OK)
                            {
                                String exten = Path.GetExtension(msgdecode_file.iText);
                                SaveFileDialog save_file = new SaveFileDialog();
                                save_file.AddExtension = true;
                                save_file.FileName = name;
                                if (save_file.ShowDialog() == true)
                                {
                                    String path = Path.GetFullPath(save_file.FileName);
                                    FileStream fs = File.OpenWrite(path);
                                    await fs.WriteAsync(msgdecode_file.FileMsg, 0, msgdecode_file.FileMsg.Length);
                                    fs.Close();
                                    MessageBox.Show("保存成功", "提示");
                                }
                                isreceive = 0;
                            }
                        }
                        else if(msgdecode.MsgType == 2)
                        {
                            MsgList.Add(msgdecode);
                            int count = FriendListWindow.List_of_groups.Count;
                            int flag = 0;
                            String id_member = msgdecode.iText;
                            int index = id_member.IndexOf("+");
                            String member_of_group = id_member.Substring(0, index);
                            String groupid = id_member.Substring(index+1);
                            for(int m = 0;m<count;m++)
                            {
                                //存在此群组则加入新成员
                                if(FriendListWindow.List_of_groups[m].ChatID == groupid)
                                {
                                    Unit temp = new Unit(member_of_group);
                                    FriendListWindow.List_of_groups[m].Addfriend(temp);
                                    flag = 1;
                                }
                            }
                            //不存在此群组则新建群组
                            if(flag == 0)
                            {
                                Groupchat chat = new Groupchat();
                                chat.ChatID = groupid;
                                chat.Addfriend(new Unit(msgdecode.DstID));
                                chat.Addfriend(new Unit(member_of_group));
                                FriendListWindow.List_of_groups.Add(chat);
                                String id = groupid;
                                MessageBox.Show("收到群组邀请消息" + msgdecode.iText, "提醒");
                            }
                            
                        }
                        else if (msgdecode.MsgType == 3)
                        {
                            msgdecode.Orient = HorizontalAlignment.Left;
                            MsgList.Add(msgdecode);
                        }
                        //接收图片
                        else if (msgdecode_file.MsgType == 121)
                        {
                            msgdecode_file.iText = "";
                            msgdecode_file.height = "0";
                            MsgList.Add(msgdecode_file);
                        }
                        Buffer.BlockCopy(recvBuffer, 0, msg, 0, datalength);
                    }
                    client.Close();
                }
            }
        }

        private bool IsMyFriend(String ID)
        {
            int num_of_friend = 0;
            if (FriendListWindow.List_of_friend != null)
            {
                num_of_friend = FriendListWindow.List_of_friend.Count();
            }
            if (num_of_friend > 0)
            {
                for (int i = 0; i < num_of_friend; i++)
                {
                    if (FriendListWindow.Obtain_ID(FriendListWindow.List_of_friend[i]) == ID)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        //通过ID直接获得备注名
        private String Obtain_note(String ID)
        {
            int num_of_friend = 0;
            if (FriendListWindow.List_of_friend != null)
            {
                num_of_friend = FriendListWindow.List_of_friend.Count();
            }
            if (num_of_friend > 0)
            {
                for (int i = 0; i < num_of_friend; i++)
                {
                    if (FriendListWindow.Obtain_ID(FriendListWindow.List_of_friend[i]) == ID)
                    {
                        String note = FriendListWindow.Obtain_notename(FriendListWindow.List_of_friend[i]);
                        return note;
                    }
                }
                return "";
            }
            return "";
        }

        public void Endlistening()
        {
            Recvmsg.Abort();
            Recvmsg.Join();
            tlListen.Stop();
        }


    }

}
