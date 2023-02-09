using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace DummyClient
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
    class PlayerInfoReq :Packet
    {
        public int id;
        public int name;
    }
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2,
    } 
    class ServerSession : PacketSession
    {
        public override void OnConneceted(EndPoint endpoint)
        {
            Console.WriteLine("Login State : Connection \r");
            PlayerInfoReq packet = new PlayerInfoReq() {size = 4,id = (ushort)PacketID.PlayerInfoReq};
            ArraySegment<byte> openSegmente = SendBufferHelper.Open(4096);

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);
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
