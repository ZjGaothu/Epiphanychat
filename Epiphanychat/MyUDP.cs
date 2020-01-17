using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EpiphanyChat
{
    class MyUDP
    {
        //为重传提供基础
        private bool isACK = true; //当前是否接收到ACK
        private iMessage Last_msg = null; //用以记录上次发送数据 是否重传
        private String Last_ip = null; //上次发送的ip
        private int Last_port; //上次发送的端口


        static UdpClient udpcl = null;
        static IPEndPoint localIpep = null;
        public static bool IsudpclStart = false;
        private const int port = 54000;
        static Thread UDPrecv; //监听udp报文
        public MyUDP() { }
        //开始监听
        public void StartListening()
        {
            if (!IsudpclStart) // 未监听的情况，开始监听
            {
                localIpep = new IPEndPoint(GetIP(), port); // 本机IP和监听端口号
                udpcl = new UdpClient(localIpep);
                IsudpclStart = true;
                UDPrecv = new Thread(Wait_recv);
                UDPrecv.Start();
            }
        }

        //关闭监听
        public void EndListening()
        {
            if (IsudpclStart)
            {
                UDPrecv.Abort();
                //UDPrecv.Join();
                udpcl.Close();
                IsudpclStart = false;
            }
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


        //等待接收报文
        public void Wait_recv()
        {
            while (true)
            {
                try
                {
                    byte[] recvBuffer = udpcl.Receive(ref localIpep);
                    byte[] revchecksum = new byte[2]; //接受到的校验和
                    Buffer.BlockCopy(recvBuffer, 0, revchecksum, 0, 2);
                    byte[] realdata = new byte[recvBuffer.Length - 2];
                    Buffer.BlockCopy(recvBuffer, 2, realdata, 0, recvBuffer.Length - 2);
                    String str = Encoding.Unicode.GetString(realdata);
                    iMessage msgonce = new iMessage();
                    iMessage msgdecode = msgonce.Decode_Message(str);
                    if(msgdecode.MsgType == 0 || msgdecode.MsgType == 3) //文字信息直接接收
                    {
                        byte[] mychecksum = Code_checksum(msgdecode); //计算校验和
                        mychecksum[0] = (byte)~mychecksum[0];
                        mychecksum[1] = (byte)~mychecksum[1];
                        byte[] check = new byte[2];
                        check[0] = (byte)(revchecksum[0] + mychecksum[0]);
                        check[1] = (byte)(revchecksum[1] + mychecksum[1]);
                        String temp_ip = Obtain_ip(msgdecode);
                        msgdecode = Decode_IP(msgdecode);
                        //判断校验和是否为全1
                        if (check[0] == 255 && check[1] == 255)
                        {          
                            //接收并显示消息
                            MyP2P.MsgList.Add(msgdecode);
                            //首先发送ACK,向回传
                            iMessage ACK_msg = new iMessage("ACK", msgdecode.DstID, msgdecode.SrcID, DateTime.Now.ToLongTimeString().ToString(), 101);
                            SendMessage(ACK_msg, temp_ip, port);
                        }
                        //校验不正确回传NAK 
                        else
                        {
                            iMessage NAK_msg = new iMessage("NAK", msgdecode.DstID, msgdecode.SrcID, DateTime.Now.ToLongTimeString().ToString(), 100);
                            SendMessage(NAK_msg, temp_ip, port);
                        }
                        
                    }
                    else if(msgdecode.MsgType == 101)//表示ACK
                    {
                        isACK = true;
                        //MessageBox.Show("收到ACK", "艹");
                    }
                    else if(msgdecode.MsgType == 100) //表示NAK
                    {
                        isACK = false;
                        //接收到NAK进行重传
                        String message = Last_msg.Code_Message();
                        Last_msg = Code_IP(Last_msg);
                        byte[] Data = new byte[0];
                        byte[] checksum = Code_checksum(Last_msg); //计算校验和
                        if (Last_msg.MsgType == 0 && Last_msg.MsgType == 2 && Last_msg.MsgType == 3)
                        {
                            //Data = Encoding.Unicode.GetBytes(message);
                            Data = Last_msg.Code_Mesaage_file();
                        }
                        else
                        {
                            Data = Last_msg.Code_Mesaage_file();
                        }
                        byte[] newdata = new byte[Data.Length + 2];
                        newdata[0] = checksum[0];
                        newdata[1] = checksum[1];
                        Buffer.BlockCopy(Data, 0, newdata, 2, Data.Length);
                        try
                        {
                            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse(Last_ip), Last_port);
                            udpcl.Send(newdata, newdata.Length, remoteIpep);
                            //udpcSend.Close();
                        }
                        catch { }
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示");
                    break;
                }
            }
        }

        //发送报文
        public void SendMessage(iMessage imsg, String send_IP, int send_Port)
        {
            if(isACK)
            {
                isACK = false; //等待ACK
                //记录重传的信息
                Last_msg = imsg;
                Last_ip = send_IP;
                Last_port = send_Port;
                String message = imsg.Code_Message();
                imsg = Code_IP(imsg);
                byte[] Data = new byte[0];
                byte[] checksum = Code_checksum(imsg); //计算校验和
                if (imsg.MsgType == 0 && imsg.MsgType == 2 && imsg.MsgType == 3)
                {
                    //Data = Encoding.Unicode.GetBytes(message);
                    Data = imsg.Code_Mesaage_file();
                }
                else
                {
                    Data = imsg.Code_Mesaage_file();
                }
                byte[] newdata = new byte[Data.Length + 2];
                newdata[0] = checksum[0];
                newdata[1] = checksum[1];
                Buffer.BlockCopy(Data, 0, newdata, 2, Data.Length);
                try
                {
                    IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse(send_IP), send_Port);
                    udpcl.Send(newdata, newdata.Length, remoteIpep);
                    imsg = Decode_IP(imsg);
                    //udpcSend.Close();
                }
                catch { }
            }
            else
            {
                int start = Environment.TickCount;
                while (Math.Abs(Environment.TickCount - start) < 500)//毫秒
                {
                    if(isACK) //重传
                    {
                        break;
                    }
                }
                isACK = true; //计时结束后
                SendMessage(imsg, send_IP, send_Port);
            }
            
        }

        //计算并包装校验和
        public byte[] Code_checksum(iMessage imsg)
        {
            String text = imsg.iText;
            byte[] temp = Encoding.Unicode.GetBytes(text);
            int cks = 0;
            foreach (byte item in temp)
            {
                cks = (cks + item) % 0xffff;
            }
            byte high = (byte)((cks & 0xff00) >> 8);
            byte low = (byte)(cks & 0xff);
            byte[] checksum = new byte[] { high, low };
            //取反码
            checksum[0] = (byte)~checksum[0];
            checksum[1] = (byte)~checksum[1];

            return checksum; //返回两字节的校验和
        }

        //在本身的Text中封装ip信息
        public iMessage Code_IP(iMessage imsg)
        {
            String begin_ip = "--BEGIN--IP--";
            String end_ip = "--END--IP--";
            imsg.iText = imsg.iText + begin_ip + GetIP().ToString() + end_ip;
            return imsg;
        }
        //将封装的ip消息去除
        public iMessage Decode_IP(iMessage imsg)
        {
            int begin_ip = imsg.iText.IndexOf("--BEGIN--IP--");
            int end_ip = imsg.iText.IndexOf("--END--IP--");
            String content = imsg.iText.Substring(0,begin_ip);
            imsg.iText = content;
            return imsg;
        }
        //从封装中获取发送端的IP
        public String Obtain_ip(iMessage imsg)
        {
            String begin_IP = "--BEGIN--IP--";
            String end_IP = "--END--IP--";
            int begin_ip = imsg.iText.IndexOf("--BEGIN--IP--");
            int end_ip = imsg.iText.IndexOf("--END--IP--");
            String ip = imsg.iText.Substring(begin_ip + begin_IP.Length, end_ip - begin_ip - begin_IP.Length);
            return ip;
        }
    }
}
