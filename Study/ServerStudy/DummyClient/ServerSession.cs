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

    //class PlayerInfoReq
    //{
    //    public long playerId;
    //    public string name;

    //    public struct SkillInfo
    //    {
    //        public int id;
    //        public short level;
    //        public short duration;

    //        public bool Write(Span<byte> s, ref ushort count)
    //        {
    //            bool success = true;
    //            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
    //            count += sizeof(int);
    //            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
    //            count += sizeof(ushort);
    //            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
    //            count += sizeof(ushort);
    //            return success;
    //        }

    //        public void Read(ReadOnlySpan<byte> s, ref ushort count)
    //        {

    //            id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
    //            count += sizeof(int);
    //            level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
    //            count += sizeof(ushort);
    //            duration = BitConverter.ToInt16(s.Slice(count, s.Length - count));
    //            count += sizeof(ushort);
    //        }
    //    }

    //    public List<SkillInfo> skills = new List<SkillInfo>();
    //    public PlayerInfoReq()
    //    {
    //        this.name = "Welcome";
    //    }
    //    public void Read(ArraySegment<byte> segment)
    //    {
    //        ushort pos = 0;
    //        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


    //        pos += sizeof(ushort);
    //        pos += sizeof(ushort);
    //        this.playerId = BitConverter.ToInt16(s.Slice(pos, s.Length - pos));
    //        pos += sizeof(long);



    //        ushort nameLen = BitConverter.ToUInt16(s.Slice(pos, s.Length - pos));
    //        pos += sizeof(ushort);
    //        this.name = Encoding.Unicode.GetString(s.Slice(pos, nameLen));
    //        pos += nameLen;
    //        // Skill List
    //        this.skills.Clear(); 
    //        ushort skillLen = BitConverter.ToUInt16(s.Slice(pos, s.Length - pos));
    //        pos += sizeof(ushort);
    //        for (int i = 0; i < skillLen; i++)
    //        {
    //            SkillInfo skill = new SkillInfo();
    //            skill.Read(s, ref pos);
    //            skills.Add(skill);
    //        }


    //    }

    //    public ArraySegment<byte> Write()
    //    {
    //        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
    //        ushort size = 0;
    //        bool success = true;

    //        //ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(s.Array, s.Offset, s.Count);
    //        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

    //        //Slice  = 
    //        size += sizeof(ushort);
    //        success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)PacketID.PlayerInfoReq);
    //        size += sizeof(ushort);
    //        success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.playerId);
    //        size += sizeof(long);
    //        //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + size, s.Count - size), this.playerId);
    //        //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), size);

    //        // String Data를 샌드하기 위해서는 우선 길이를 구한다.
    //        // 길이를 집어넣는다.
    //        //// 값을 집어넣는다.

    //        //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
    //        //success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), nameLen);
    //        //size += sizeof(ushort);
    //        //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, size, nameLen);
    //        //size += nameLen;
    //        // => 한방 처리
    //        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + size + sizeof(ushort));
    //        success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), nameLen);
    //        size += sizeof(ushort);
    //        size += nameLen;


    //        // Skill 을 집어 넣는다.
    //        success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)skills.Count);
    //        size += sizeof(ushort);
    //        foreach (SkillInfo skill in skills)
    //        {
    //            success &= skill.Write(s, ref size);
    //            //TODO
    //        }

    //        // Size(패킷 사이즈를 맨 앞에 넣는다.)
    //        success &= BitConverter.TryWriteBytes(s, size);

    //        if (success == false)
    //        {
    //            return null;
    //        }

    //        ArraySegment<byte> sendBuff = SendBufferHelper.Close(size);
    //        return sendBuff;
    //    }
    //}

    class PlayerInfoReq
    {
        public long playerId;
        public string name;

        public struct Skill
        {
            public int id;
            public short level;
            public float duration;

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {

                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);


                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);


                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

            }

            public bool Write(Span<byte> s, ref ushort size)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.id);
                size += sizeof(int);


                success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.level);
                size += sizeof(short);


                success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.duration);
                size += sizeof(float);

                return success;
            }

        }

        public List<Skill> skills = new List<Skill>();



        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


            count += sizeof(ushort);
            count += sizeof(ushort);


            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);


            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;


            this.skills.Clear();
            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLen; i++)
            {
                Skill skill = new Skill();
                skill.Read(s, ref count);
                skills.Add(skill);
            }

        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            ushort size = 0;
            bool success = true;

            //ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(s.Array, s.Offset, s.Count);
            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            //Slice  = 
            size += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)PacketID.PlayerInfoReq);
            size += sizeof(ushort);



            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.playerId);
            size += sizeof(long);


            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + size + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), nameLen);
            size += sizeof(ushort);
            size += nameLen;



            success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)this.skills.Count);
            size += sizeof(ushort);
            foreach (Skill skill in this.skills)
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
    class ServerSession : Session
    {

        public override void OnConnected(EndPoint endpoint)
        {
            Console.WriteLine("Login State : Connection \r");
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 101,name = "client" };
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 101, level = 2, duration = 101 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 201, level = 2, duration = 201 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 301, level = 2, duration = 301 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 401, level = 2, duration = 301 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 501, level = 2, duration = 301 });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 601, level = 2, duration = 401 });
            Console.WriteLine(packet.skills.Count);
            var s = packet.Write();
            if (s != null)
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
