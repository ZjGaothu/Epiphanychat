using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EpiphanyChat
{
    //一个群聊的整体信息
    public class Groupchat
    {
        public List<Unit> IDtosend = new List<Unit>();
        public String ChatID { get; set; }
        public String Chatname { get; set; }
        public Groupchat(String chatid_, String chatname_)
        {
            ChatID = chatid_;
            Chatname = chatname_;
        }
        public Groupchat() { }
        public void Addfriend(String id, String ip)
        {
            if(Obtain_one(id))
            {
                MessageBox.Show("此群组中包含这位同学", "提示");
                return;
            }
            Unit temp = new Unit(ip, id);
            IDtosend.Add(temp);
        }
        public void Addfriend(Unit onece)
        {
            if(Obtain_one(onece.ID))
            {
                MessageBox.Show("此群组中包含这位同学", "提示");
                return;
            }
            IDtosend.Add(onece);
        }
        //群组是否包含某ID
        public bool Obtain_one(String id)
        {
            int num = 0;
            if(IDtosend != null)
            {
                num = IDtosend.Count;
                for( int i = 0;i<num;i++)
                {
                    Unit temp = IDtosend[i];
                    if(temp.ID == id)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }

    //每个个体 封装ip地址和id
    public class Unit
    {
        public String IP { get; set; }
        public String ID { get; set; }
        public Unit(String ip_, String id_)
        {
            IP = ip_;
            ID = id_;
        }
        public Unit(String id_)
        {
            ID = id_;
        }
    }
}
