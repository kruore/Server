using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
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
        public string name;

        public PlayerInfoReq()
        {
            //this.playerId = (ushort)packetid.playerInfoReq;

        }

        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;

            count += 2;
            count += 2;
            this.playerId = BitConverter.ToUInt16(s.Array, s.Offset+count);
            BitConverter.ToUInt16(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count, s.Count - count));
            count += 8;
            //String
            //int messageSize = Encoding.UTF8.GetBytes(ac).Length;
            //ushort size = BitConverter.ToUInt16(openSegment, openSegment.Offset+messageSize);


            //count += 2;
            ////ushort size = BitConverter.ToUInt16(openSegment.Array, openSegment.Offset);
            //count += 2;
            ////ushort id = BitConverter.ToUInt16(openSegment.Array, openSegment.Offset + count);
            //count += 2;
            //this.playerId = BitConverter.ToUInt16(openSegment.Array, openSegment.Offset + count);
            //count += 8;
            //Console.WriteLine($" DATA RECV : {playerId}");

        }
        public override ArraySegment<byte> Write()
        {

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), this.packetid);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset + count, openSegment.Count - count), this.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count), count);

            if (success == false)
            {
                return null;
            }

            return SendBufferHelper.Close(count);
        }
    }

    public abstract class ClientSession : Session
    {
        public static readonly int HeaderSize = 2;
        // 다른 곳에서 해당 리시브를 접근할 수 없음
        // [size, packetid ][....][size, packetid ][....]
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true)
            {
                //최소한의 헤더는 파싱가능한가?
                if (buffer.Count < HeaderSize)
                {
                    break;
                }

                // 패킷 도착

                int dataSize = BitConverter.ToInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                {
                    break;
                }
                //스택에 복사하는 개념이라 new 써도 ㄱㅊ
                OnRecvRacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }
            return processLen;
        }
        public abstract void OnRecvRacket(ArraySegment<byte> buffer);
    }


    public class ServerSession : ClientSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            //   Packet packet = new Packet() { size = 8, packetid = 5 };


            //byte[] sendbuff = Encoding.UTF8.GetBytes("Welcome");
            //Send(sendbuff);
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnDisConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisConnected : {endPoint}");
        }
        //public override int OnRecv(ArraySegment<byte> buffer)
        //{
        //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        //    Console.WriteLine($"[From Client] : {recvData}");
        //    return buffer.Count;
        //}
        public override void OnRecvRacket(ArraySegment<byte> buffer)
        {
            //int size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            //int id = BitConverter.ToUInt16(buffer.Array, buffer.Offset+2);

            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq req = PlayerInfoReq.Read();
                        Console.WriteLine($" DATA RECV : {playerId}");
                    }
                    break;
            }
            //string a = Encoding.UTF8.GetString(buffer);

        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Byte Transfered{numOfBytes}");
        }
    }
}
