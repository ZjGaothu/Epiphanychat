using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

/// <summary>
/// 模块名：Message
/// 功能： 发送文字信息的包装与格式
/// </summary>
namespace EpiphanyChat
{
    class iMessage
    {
        //消息类型 
        public int MsgType = 0; //发送类型 目前仅有文字
        // 0 - 文字 1 - 文件 3 - 群聊添加信息 4 - 群聊文字 100 - NAK 101 - ACK 121 - 图片
        public String iText { get; set; } //文字
        public String SrcID { get; set; } //发送者ID
        public String DstID { get; set; } //目的ID
        public String Time { get; set; } //发送时间
        public HorizontalAlignment Orient { get; set; }//消息属于自己发送还是对方
        public byte[] FileMsg { get; set; } //文件内容
        public String ChatID { get; set; }
        public String Myimage { get; set; } //发送的图片
        public String height { get; set; }//动态改变UI的长和宽
        public iMessage() { }   
        public iMessage(String _Text , String _SrcID,String _DstID,String _Time,int _MsgType = 0 )
        {
            MsgType = _MsgType;
            iText = _Text;
            SrcID = _SrcID;
            DstID = _DstID;
            Time = _Time;
            Orient = HorizontalAlignment.Left;
            FileMsg = new byte[0];
            ChatID = "null";
            Myimage = "";
        }
        public iMessage(String _Text, String _SrcID, String _DstID, String _Time,String _chatid, int _MsgType = 0)
        {
            MsgType = _MsgType;
            iText = _Text;
            SrcID = _SrcID;
            DstID = _DstID;
            Time = _Time;
            Orient = HorizontalAlignment.Left;
            FileMsg = new byte[0];
            ChatID = _chatid;
            Myimage = "";
        }
        //重载初始化方法 发送文件
        //此时的Text用来保存
        public iMessage(String _Text, String _SrcID, String _DstID, String _Time, byte[] _Filemsg , int _MsgType = 0)
        {
            MsgType = _MsgType;
            iText = _Text;
            SrcID = _SrcID;
            DstID = _DstID;
            Time = _Time;
            Orient = HorizontalAlignment.Left;
            FileMsg = new byte[_Filemsg.Length];
            Buffer.BlockCopy(_Filemsg, 0, FileMsg, 0, _Filemsg.Length);
            ChatID = "null";
            Myimage = "";
        }
        //重载 传入length参数
        public iMessage(String _Text, String _SrcID, String _DstID, String _Time, byte[] _Filemsg, int length ,int _MsgType = 0)
        {
            MsgType = _MsgType;
            iText = _Text;
            SrcID = _SrcID;
            DstID = _DstID;
            Time = _Time;
            Orient = HorizontalAlignment.Left;
            FileMsg = new byte[length];
            Buffer.BlockCopy(_Filemsg, 0, FileMsg, 0, length);
            ChatID = "null";
            Myimage = "";
        }
        //定义数据报中的分隔符
        private String Separa_Ty_L = "--BEGIN--Ty--";
        private String Separa_Ty_R = "--Ty--END--";
        private String Separa_Te_L = "--BEGIN--Te--";
        private String Separa_Te_R = "--Te--End--";
        private String Separa_Sr_L = "--BEGIN--Sr--";
        private String Separa_Sr_R = "--Sr--END--";

        private String Separa_Ds_L = "--BEGIN--Ds--";
        private String Separa_Ds_R = "--Ds--END--";
        private String Separa_Ti_L = "--BEGIN--Ti--";
        private String Separa_Ti_R = "--Ti--END--";

        private String Separa_Fi_L = "--BEGIN--Fi--";
        private String Separa_Fi_R = "--Fi--END--";

        //chat id
        private String Separa_Ci_L = "--BEGIN--Ci--";
        private String Separa_Ci_R = "--Ci--END--";
        //将信息进行封装编码
        public String Code_Message()
        {
            //封装结果
            String CodedeMsg = "";
            CodedeMsg = CodedeMsg + Separa_Ty_L + MsgType.ToString() + Separa_Ty_R;
            CodedeMsg = CodedeMsg + Separa_Te_L + iText + Separa_Te_R;
            CodedeMsg = CodedeMsg + Separa_Sr_L + SrcID + Separa_Sr_R;
            CodedeMsg = CodedeMsg + Separa_Ds_L + DstID + Separa_Ds_R;
            CodedeMsg = CodedeMsg + Separa_Ti_L + Time + Separa_Ti_R;
            CodedeMsg = CodedeMsg + Separa_Ci_L + ChatID + Separa_Ci_R;
            return CodedeMsg;
        }

        //将一个数据报解码为一个Message类型
        public iMessage Decode_Message(String codemsg)
        {
            int begin_Ty = codemsg.IndexOf(Separa_Ty_L);
            int end_Ty = codemsg.IndexOf(Separa_Ty_R);
            int begin_Te = codemsg.IndexOf(Separa_Te_L);
            int end_Te = codemsg.IndexOf(Separa_Te_R);
            int begin_Sr = codemsg.IndexOf(Separa_Sr_L);
            int end_Sr = codemsg.IndexOf(Separa_Sr_R);
            int begin_Ds = codemsg.IndexOf(Separa_Ds_L);
            int end_Ds = codemsg.IndexOf(Separa_Ds_R);
            int begin_Ti = codemsg.IndexOf(Separa_Ti_L);
            int end_Ti = codemsg.IndexOf(Separa_Ti_R);
            int begin_Ci = codemsg.IndexOf(Separa_Ci_L);
            int end_Ci = codemsg.IndexOf(Separa_Ci_R);
            String _text = codemsg.Substring(begin_Te + Separa_Ds_L.Length, end_Te - begin_Te - Separa_Te_L.Length);
            String _Src = codemsg.Substring(begin_Sr + Separa_Sr_L.Length, end_Sr - begin_Sr - Separa_Sr_L.Length);
            String _Dst = codemsg.Substring(begin_Ds + Separa_Te_L.Length, end_Ds - begin_Ds - Separa_Ds_L.Length);
            String _time = codemsg.Substring(begin_Ti + Separa_Ti_L.Length, end_Ti - begin_Ti - Separa_Ti_L.Length);
            String _chatid = codemsg.Substring(begin_Ci + Separa_Ci_L.Length, end_Ci - begin_Ci - Separa_Ci_L.Length);
            int _msgtype = int.Parse(codemsg.Substring(begin_Ty + Separa_Ty_L.Length, end_Ty - begin_Ty - Separa_Ty_L.Length));
            iMessage msg = new iMessage(_text, _Src, _Dst, _time  ,_chatid, _msgtype);
            return msg;
        }

        //对传输的文件编码
        public byte[] Code_Mesaage_file()
        {
            String CodedeMsg = "";
            CodedeMsg = CodedeMsg + Separa_Ty_L + MsgType.ToString() + Separa_Ty_R;
            CodedeMsg = CodedeMsg + Separa_Te_L + iText + Separa_Te_R;
            CodedeMsg = CodedeMsg + Separa_Sr_L + SrcID + Separa_Sr_R;
            CodedeMsg = CodedeMsg + Separa_Ds_L + DstID + Separa_Ds_R;
            CodedeMsg = CodedeMsg + Separa_Ti_L + Time + Separa_Ti_R;
            CodedeMsg = CodedeMsg + Separa_Ci_L + ChatID + Separa_Ci_R;
            CodedeMsg = CodedeMsg + Separa_Fi_L + "k" + Separa_Fi_R;
            byte[] temp = Encoding.Unicode.GetBytes(CodedeMsg);
            byte[] result = new byte[temp.Length + FileMsg.Length];
            Buffer.BlockCopy(temp, 0, result, 0, temp.Length);
            Buffer.BlockCopy(FileMsg, 0, result, temp.Length, FileMsg.Length);
            return result;
        }
        //解码传输的文件
        public iMessage Decode_Message_file(byte[] codemsg)
        {
            String text = Encoding.Unicode.GetString(codemsg);
            int begin_Ty = text.IndexOf(Separa_Ty_L);
            int end_Ty = text.IndexOf(Separa_Ty_R);
            int begin_Te = text.IndexOf(Separa_Te_L);
            int end_Te = text.IndexOf(Separa_Te_R);
            int begin_Sr = text.IndexOf(Separa_Sr_L);
            int end_Sr = text.IndexOf(Separa_Sr_R);
            int begin_Ds = text.IndexOf(Separa_Ds_L);
            int end_Ds = text.IndexOf(Separa_Ds_R);
            int begin_Ti = text.IndexOf(Separa_Ti_L);
            int end_Ti = text.IndexOf(Separa_Ti_R);
            int begin_Ci = text.IndexOf(Separa_Ci_L);
            int end_Ci = text.IndexOf(Separa_Ci_R);
            String _text = text.Substring(begin_Te + Separa_Ds_L.Length, end_Te - begin_Te - Separa_Te_L.Length);
            String _Src = text.Substring(begin_Sr + Separa_Sr_L.Length, end_Sr - begin_Sr - Separa_Sr_L.Length);
            String _Dst = text.Substring(begin_Ds + Separa_Te_L.Length, end_Ds - begin_Ds - Separa_Ds_L.Length);
            String _time = text.Substring(begin_Ti + Separa_Ti_L.Length, end_Ti - begin_Ti - Separa_Ti_L.Length);
            String _chatid = text.Substring(begin_Ci + Separa_Ci_L.Length, end_Ci - begin_Ci - Separa_Ci_L.Length);
            int _msgtype = int.Parse(text.Substring(begin_Ty + Separa_Ty_L.Length, end_Ty - begin_Ty - Separa_Ty_L.Length));
            String CodedeMsg = "";
            CodedeMsg = CodedeMsg + Separa_Ty_L +_msgtype.ToString() + Separa_Ty_R;
            CodedeMsg = CodedeMsg + Separa_Te_L + _text + Separa_Te_R;
            CodedeMsg = CodedeMsg + Separa_Sr_L + _Src + Separa_Sr_R;
            CodedeMsg = CodedeMsg + Separa_Ds_L + _Dst + Separa_Ds_R;
            CodedeMsg = CodedeMsg + Separa_Ti_L + _time + Separa_Ti_R;
            CodedeMsg = CodedeMsg + Separa_Ci_L + _chatid + Separa_Ci_R;
            CodedeMsg = CodedeMsg + Separa_Fi_L + "k" + Separa_Fi_R;
            byte[] real_text = Encoding.Unicode.GetBytes(CodedeMsg);
            int length = real_text.Length;
            int length_whole = codemsg.Length;
            byte[] filedata = new byte[length_whole - length];
            Array.Copy(codemsg, length, filedata,0, length_whole - length);
            iMessage msg = new iMessage(_text, _Src, _Dst, _time, filedata, filedata.Length, _msgtype);
            return msg;
        }
    }
}
