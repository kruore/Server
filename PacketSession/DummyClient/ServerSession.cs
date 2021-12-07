using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string data;

        public PlayerInfoReq()
        {
            this.playerId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort pos = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            pos += sizeof(ushort);
            pos += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(s.Slice(pos, s.Length - pos));
            pos += sizeof(long);

            //String
            ushort nameLen = BitConverter.ToUInt16(s.Slice(pos, s.Length - pos));
            pos += sizeof(ushort);
            this.data = Encoding.UTF8.GetString(s.Slice(pos, nameLen));
            pos += nameLen;

            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            //pos += sizeof(ushort);
            //// ushort id = BitConverter.ToUInt16(s.Array, s.Offset + pos);
            //pos += sizeof(ushort);
            //long playerId = BitConverter.ToInt64(s.Slice(pos,s.Length-pos));
            //pos += sizeof(long);


        }

        public override ArraySegment<byte> Write()
        {
            //버퍼 사이즈 예약
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort size = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            size += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.packetId);
            size += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.playerId);
            size += sizeof(long);
            success &= BitConverter.TryWriteBytes(s, size);

            ushort nameLen = (ushort)Encoding.UTF8.GetByteCount(this.data);
            success &= BitConverter.TryWriteBytes(s.Slice(s.Length - size), nameLen);
            size += sizeof(ushort);
            Array.Copy(Encoding.UTF8.GetBytes(this.data), 0, segment.Array, segment.Count, nameLen);
            size += nameLen;

            if (success == false)
            {
                return null;
            }

            return SendBufferHelper.Close(size);
            //size += sizeof(ushort);
            //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.playerId);
            //size += sizeof(ushort);
            //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.playerId);
            //size += sizeof(long);
            //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), size);

            ////버퍼 사이즈 결정
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(size);


            //String Len[2]
            // UTF-8 로 전환해서 보낸다.

        }
    }

    //class PlayerInfoOk : Packet
    //{
    //    public int hp;
    //    public int attack;
    //}
    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    class ServerSession : Session
    {
        static unsafe void ToBytes(byte[] array, int offset, ulong value)
        {
            fixed (byte* ptr = &array[offset])
                *(ulong*)ptr = value;
        }

        static unsafe void ToBytes<T>(byte[] array, int offset, T value) where T : unmanaged
        {
            fixed (byte* ptr = &array[offset])
                *(T*)ptr = value;
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001,data = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" };


            // 보낸다
            for (int i = 0; i < 5; i++)
            {
                //버퍼 사이즈 예약
                ArraySegment<byte> s = SendBufferHelper.Open(4096);
                ArraySegment<byte> data = packet.Write();

                if (data != null)
                {
                    Send(data);
                }

                ////버퍼 사이즈 결정
                //ArraySegment<byte> sendBuff = SendBufferHelper.Close(size);

                //if (success)
                //    Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }

}
