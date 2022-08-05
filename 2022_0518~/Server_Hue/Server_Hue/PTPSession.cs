using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server_Hue
{
    
    class PTPSession
    {
        public void PTP_Start(Session session)
        {
            bool success = true;
            ushort size = 0;
            ushort sendbyte = 0;
            ArraySegment<byte> s = SendBufferHelper.Open(4096);
            PTP_Packet packet = new PTP_Packet() { packetId = 0,packetType = (ushort)packTypes.ptp,counts = 1};

            // 패킷 시작 전송 = 패킷 스타트 카운터 = 1 , 타임 1 에 유닉스 타임 집어넣기
            
            //PacketSize short 만큼 추가
            size += 2;

            packet.counts = 1;
            size += 2;
            packet.packetId = 0;
            size += 2;
            packet.packetType = (ushort)packTypes.ptp;
            size += 2;
            packet.time00 = DateTimeOffset.Now.ToUnixTimeSeconds();
            size += 8;
            packet.time01 = 0;
            size += 8;
            packet.time02 = 0;
            size += 8;
            packet.time03 = 0;
            size += 8;
            packet.size = size;

            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.size);
            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.packetId);
            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.counts);
            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.packetType);
            sendbyte += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.time00);
            sendbyte += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.time01);
            sendbyte += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.time02);
            sendbyte += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.time03);
            Console.WriteLine("PTP SEND START");
            //byte[] data;
            //byte[] time;
            //session.OnSend();
            
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(sendbyte);

            if(success)
            {
                session.Send(sendBuff);
                Console.WriteLine("SUCCESSFULLY SEND");
            }
        }

        void PTP_RecvPacket()
        {

        }
    }
}
