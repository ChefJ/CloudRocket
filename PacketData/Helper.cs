using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketData
{
    public static class Helper
    {
        public static byte[] ForePlay(Packet PacketToSend)
        {
            int PacketLength = PacketToSend.ToBytes().Length;
            List<byte> PacketLenthByte = BitConverter.GetBytes(PacketLength).ToList();
            PacketLenthByte.AddRange(PacketToSend.ToBytes());
            return PacketLenthByte.ToArray();
        }
    }
}
