using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    class PacketFormat
    {
        /// <summary>
        /// {0} : 패킷 이름
        /// {1} : 맴버 변수들
        /// 
        /// </summary>
        public static string parketFormat =
@"
 class {0}
    {{
        {1}

        public struct SkillInfo
        {{
            public int id;
            public short level;
            public short duration;

            public bool Write(Span<byte> s, ref ushort count)
            {{
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(ushort);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(ushort);
                return success;
            }}

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {{

                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
                duration = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
            }}
        }}

        public List<SkillInfo> skills = new List<SkillInfo>();
        public PlayerInfoReq()
        {{
            this.name = ""Welcome"";
        }}
    public void Read(ArraySegment<byte> segment)
    {{
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
        {{
            SkillInfo skill = new SkillInfo();
            skill.Read(s, ref pos);
            skills.Add(skill);
        }}


    }}

    public ArraySegment<byte> Write()
    {{
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
        {{
            success &= skill.Write(s, ref size);
            //TODO
        }}

        // Size(패킷 사이즈를 맨 앞에 넣는다.)
        success &= BitConverter.TryWriteBytes(s, size);

        if (success == false)
        {{
            return null;
        }}

        ArraySegment<byte> sendBuff = SendBufferHelper.Close(size);
        return sendBuff;
    }}
}}
";
    }
}
