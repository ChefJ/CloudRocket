using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PacketData
{
    [Serializable]

    public class Packet
    {
        public string SenderGUID;
        public string SenderName;
        public string Message;
        public byte[] FileByte;
        public string FileName;
        public PacketType _PacketType;

        public Packet(PacketType Type, string InSenderGUID, string InSenderName,string InMessage="", string InFilePath = "")
        {
            SenderGUID = InSenderGUID;
            SenderName = InSenderName;
            Message = InMessage;
            _PacketType = Type;

            if (_PacketType == PacketType.File)
            {
                if (File.Exists(InFilePath))
                    try
                    {
                        FileInfo fi = new FileInfo(InFilePath);
                        this.FileName = fi.Name;
                        FileByte = File.ReadAllBytes(InFilePath);

                    }
                    catch (Exception exx)
                    {
                        Message += exx.Message + InFilePath + "文件传输失败";
                    }
            }
       //     if(_PacketType == PacketType.Registration)
       //     {
       //         Message +=InSenderName+ "已上线。";
       //     }
            if (_PacketType == PacketType.CloseConnection)
            {
                Message += InSenderName + "已下线。";
            }
        }
        public Packet(byte[] packetBytes)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(packetBytes);
            ms.Position = 0;
            Packet p = (Packet)bf.Deserialize(ms);
            ms.Close();
            this._PacketType = p._PacketType;
            this.SenderGUID = p.SenderGUID;
            this.SenderName = p.SenderName;
            this.Message = p.Message;
            this.FileByte = p.FileByte;
            this.FileName = p.FileName;

        }

        public byte[] ToBytes()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, this);
            byte[] bytes = ms.ToArray();
            ms.Close();

            return bytes;
        }
    }

    public enum PacketType
    {
        Chat,
        File,
        NextPacketInfo,
        CloseConnection
    }
}
