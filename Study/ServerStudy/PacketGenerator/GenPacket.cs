using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

public enum PacketID
{
    PlayerInfoReq = 1,
    Test = 2,

}


class PlayerInfoReq : IPacket
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


    public ushort Protocol { get { return (ushort)PacketID.PlayerInfoReq; } }
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

class Test : IPacket
{
    public int testInt;
    public ushort Protocol { get { return (ushort)PacketID.Test; } }
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


        count += sizeof(ushort);
        count += sizeof(ushort);


        this.testInt = BitConverter.ToInt32(s.Slice(count, s.Length - count));
        count += sizeof(int);

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
        success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)PacketID.Test);
        size += sizeof(ushort);



        success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.testInt);
        size += sizeof(int);



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


interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}
