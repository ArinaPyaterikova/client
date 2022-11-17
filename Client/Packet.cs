using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public enum PacketType : uint
    {
        RequestJoin = 6,
        JoinAck,
        IsHere,
        GameStartAck,
        PaddlePosition,
    }
    public class Packet
    {
        public byte[] data = new byte[0]; //data that is used as an answer to someone
        public long timestamp; //time when packet was created
        public PacketType type;

        public Packet(PacketType type)
        {
            this.type = type;
            timestamp = DateTime.Now.Ticks;
        }

        public Packet(byte[] vs)
        {
            int flag = 0; //beginning of byte array

            //type of packet
            type = (PacketType)BitConverter.ToUInt32(vs, 0);
            flag += sizeof(PacketType);

            //time of packet
            timestamp = BitConverter.ToInt64(vs, flag);
            flag += sizeof(long); //bz time.ticks is long type of value

            data = vs.Skip(flag).ToArray(); //data to load
        }

        /// <summary>
        /// Used to construct array for sending to a client
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] Construct()
        {
            int ptSize = sizeof(PacketType);
            int tsSize = sizeof(long);

            int i = 0;
            byte[] buf = new byte[ptSize + tsSize + data.Length];

            //type of packet
            BitConverter.GetBytes((uint)type).CopyTo(buf, i);
            i += ptSize;

            //time of packet
            BitConverter.GetBytes(timestamp).CopyTo(buf, i);
            i += tsSize;

            //data to load
            data.CopyTo(buf, i);
            _ = data.Length;

            return buf;
        }

        public void Send(UdpClient client)
        {
            byte[] bytes = Construct();
            client.Send(bytes, bytes.Length);
        }

    }

    #region packets Client
    /// <summary>
    /// See task https://github.com/RRPteam1/Lab1/issues/2
    /// </summary>
    /// 
    // Client Join Request
    public class RequestJoin : Packet
    {
        public RequestJoin()
            : base(PacketType.RequestJoin)
        {
        }
    }


    // Ack packet for the one above
    public class JoinAck : Packet
    {
        public JoinAck()
            : base(PacketType.JoinAck)
        {
        }
    }

    // Client tells the Server it's alive
    public class IsHere : Packet
    {
        public IsHere()
            : base(PacketType.IsHere)
        {
        }
    }

    public class GameStartAckPacket : Packet
    {
        public GameStartAckPacket()
            : base(PacketType.GameStartAck)
        {
        }
    }
    #endregion
}