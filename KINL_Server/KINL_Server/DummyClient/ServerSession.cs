using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using KINL_Server;

namespace DummyClient
{

    public abstract class Packet
    {
        public ushort size;
        public ushort packetid;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

 
    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.playerId =(ushort)PacketID.PlayerInfoReq;

        }

        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;

            //ushort size = BitConverter.ToUInt16(openSegment.Array, openSegment.Offset);
            count += 2;
            //ushort id = BitConverter.ToUInt16(openSegment.Array, openSegment.Offset + count);
            count += 2;
            this.playerId = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += 8;
            Console.WriteLine($" DATA RECV : {playerId}");

        }

        public override ArraySegment<byte> Write()
        {

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            //count += 2;
            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), this.packetid);
            //count += 2;
            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), this.playerId);
            //count += 8;
            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset , openSegment.Count), count);


            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(openSegment.Slice(count,openSegment.))
            if (success==false)
            {
                return null;
            }

            return SendBufferHelper.Close(count);
        }
    }


    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };
            ArraySegment<byte> s = packet.Write();

            if(s!=null)
            {
                Console.WriteLine("Send");
                Send(s);
            }
            //byte[] size = BitConverter.GetBytes(packet.size);
            //byte[] packitId = BitConverter.GetBytes(packet.packetid);
            //byte[] playerId = BitConverter.GetBytes(packet.playerId);
            // Console.WriteLine(Encoding.UTF8.GetString(buffer3));

            //Array.Copy(size, 0, openSegment.Array, openSegment.Offset, size.Length);
            //Array.Copy(packitId, 0, openSegment.Array, openSegment.Offset + count, packitId.Length);
            //count += 2;
            //Array.Copy(playerId, 0, openSegment.Array, openSegment.Offset + count, playerId.Length);
            //count += 8;



        }

        public override void OnDisConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] : {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {

        }
    }


}
