using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{

    class Packet
    {
        public ushort size;
        public ushort packetId;
        public ushort packetType;
    }

    class LoginOKPacket : Packet
    {
        public int data;
    }
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2,
    }
    class PlayerInfoReq : Packet
    {
        public int id;
        public int name;
    }
    class ClientSession : PacketSession
    {
        public override void OnConneceted(EndPoint endpoint)
        {
            Console.WriteLine("EndPoint Connection");
        }

        public override void OnDiscoonected(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public override void OnSend(int numOfBytes)
        {
            throw new NotImplementedException();
        }
    }
}
