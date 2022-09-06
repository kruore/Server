using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace DummyClient
{
    abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }
        public override void Read(ArraySegment<byte> s)
        {
            int pos = 0;

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            pos += 2;
            // ushort id = BitConverter.ToUInt16(s.Array, s.Offset + pos);
            pos += 2;
            long playerId = BitConverter.ToInt16(s.Array, s.Offset + pos);
            // TODO

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096);
            ushort size = 0;
            bool success = true;

            size += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.packetId);
            size += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.playerId);
            size += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), size);

            if (success == false)
            {
                return null;
            }

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(size);
            return sendBuff;
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }
    class ServerSession : Session
    {

        public override void OnConnected(EndPoint endpoint)
        {
            Console.WriteLine("Login State : Connection \r");
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 101};
            var s = packet.Write();
            if(s!=null)
            {
                Send(s);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            Console.WriteLine($"[From Server] {buffer.Count}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            // throw new NotImplementedException();
        }
    }
}
