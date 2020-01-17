using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Windows;

namespace EpiphanyChat
{
    class LoginCS
    {
        //服务器IP和端口号
        private const String ServerIPaddr = "166.111.140.57";
        private const int ServerPort = 8000;

        //用户名和密码
        private String user_pass = null;
        public LoginCS(String str)
        {
            user_pass = str;
        }
        public String Connect_Server()
        {
            String Ero = "Ero";
            String receive_msg = null;
            IPAddress serverIP = IPAddress.Parse(ServerIPaddr);
            IPEndPoint endPoint = new IPEndPoint(serverIP, ServerPort);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(endPoint);
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message, "提示");
                client.Close();
                return Ero;
            }
            try
            {
                byte[] Send_msg = Encoding.UTF8.GetBytes(user_pass);
                client.Send(Send_msg);
            }
            catch(SocketException e)
            {
                MessageBox.Show(e.Message, "提示");
                client.Close();
                return Ero;
            }
            try
            {
                byte[] messageBytes = new byte[100 * 1024];
                int num = client.Receive(messageBytes);
                receive_msg = Encoding.Default.GetString(messageBytes, 0, num);
                client.Close();
                return receive_msg;
            }
            catch(Exception)
            {
                client.Close();
                return Ero;
            }
            
        }


    }
}
