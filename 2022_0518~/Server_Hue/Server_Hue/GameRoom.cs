using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Hue
{
    class GameRoom
    {
        List<GameSession> _sessions = new List<GameSession>();
        object _lock = new object();

        public void Enter(GameSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }
        public void Enter(GameSession session, ushort id)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.SessionID = id;
                session.Room = this;
            }
        }
        public void Leave(GameSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }

        }

        public void FeedBack(int id, ushort feedback)
        {
            Console.WriteLine($"{id},{feedback} FEED BACK");
            lock (_lock)
            {
                foreach (var s in _sessions)
                {
                    if (id == s.SessionID)
                    {
                        FeedBack_PacketGenerate(s, feedback);
                    }
                }
            }
        }


    

        public void FeedBack_PacketGenerate(GameSession session, ushort feedback)
        {
            bool success = true;
            ushort size = 0;
            ushort sendbyte = 0;
            ArraySegment<byte> s = SendBufferHelper.Open(4096);
            FeedBack_Packet packet = new FeedBack_Packet() { packetId = 0, packetType = (ushort)packTypes.AI_report};

            // 패킷 시작 전송 = 패킷 스타트 카운터 = 1 , 타임 1 에 유닉스 타임 집어넣기

            //PacketSize short 만큼 추가
            size += 2;
            packet.packetId = 0;
            size += 2;
            var count = 0;
            size += 2;
            packet.packetType = (ushort)packTypes.AI_report;
            size += 2;
            packet.feedBack = feedback;
            size += 2;

            packet.size = size;

            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.size);
            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.packetId);
            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.packetType);
            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.feedBack);
            sendbyte += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + sendbyte, s.Count - sendbyte), packet.feedBack);
            //byte[] data;
            //byte[] time;
            //session.OnSend();

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(sendbyte);


            if (success)
            {
                session.Send(sendBuff);
                Console.WriteLine($"FeedBack Send, The SeesionID :{session.SessionID}");
            }
        }
    }
}
