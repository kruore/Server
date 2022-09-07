using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
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
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public short duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(ushort);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(ushort);
                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {

                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
                duration = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();
        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
            this.name = "Welcome";
        }
        public override void Read(ArraySegment<byte> segment)
        {
            ushort pos = 0;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


            pos += sizeof(ushort);
            pos += sizeof(ushort);
            this.playerId = BitConverter.ToInt16(s.Slice(pos, s.Length - pos));
            pos += sizeof(long);



            ushort nameLen = BitConverter.ToUInt16(s.Slice(pos, s.Length - pos));
            pos += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(pos, nameLen));
            pos += nameLen;
            // Skill List

            ushort skillLen = BitConverter.ToUInt16(s.Slice(pos, s.Length - pos));
            pos += sizeof(ushort);
            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref pos);
                skills.Add(skill);
            }


        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            ushort size = 0;
            bool success = true;

            //ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(s.Array, s.Offset, s.Count);
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            //Slice  = 
            size += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.packetId);
            size += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.playerId);
            size += sizeof(long);
            //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.playerId);
            //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), size);

            // String Data를 샌드하기 위해서는 우선 길이를 구한다.
            // 길이를 집어넣는다.
            //// 값을 집어넣는다.

            //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            //success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), nameLen);
            //size += sizeof(ushort);
            //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, size, nameLen);
            //size += nameLen;
            // => 한방 처리
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + size + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), nameLen);
            size += sizeof(ushort);
            size += nameLen;


            // Skill 을 집어 넣는다.
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)skills.Count);
            size += sizeof(ushort);
            foreach (SkillInfo skill in skills)
            {
                success &= skill.Write(s, ref size);
                //TODO
            }

            // Size(패킷 사이즈를 맨 앞에 넣는다.)
            success &= BitConverter.TryWriteBytes(s, size);

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
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            int pos = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            pos += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
            pos += 2;

            // TODO
            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"WELCOME : {p.name}");
                        foreach (PlayerInfoReq.SkillInfo skill in p.skills)
                        {
                            Console.WriteLine($"SKills : {skill.id},{skill.level},{skill.duration}");
                        }
                    }
                    break;
                case PacketID.PlayerInfoOk:
                    {
                        int hp = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
                        pos += 4;
                        int attack = BitConverter.ToInt32(buffer.Array, buffer.Offset + pos);
                        pos += 4;
                    }
                    //Handle_PlayerInfoOk();
                    break;
                default:
                    break;
            }

            Console.WriteLine($"RecvPacketId: {id}, Size {size}");
        }

        // TEMP
        public void Handle_PlayerInfoOk(ArraySegment<byte> buffer)
        {

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
