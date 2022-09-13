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
        /// {2} : 맴버 변수의 Read
        /// {3} : 맴버 변수들의 Write
        /// </summary>
        public static string parketFormat =
@"
 class {0}
    {{
        {1}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort pos = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


        pos += sizeof(ushort);
        pos += sizeof(ushort);
        
        {2}
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
        success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)PacketID.{0});
        size += sizeof(ushort);


        {3}


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
        /// <summary>
        /// {0} : 변수 형식
        /// {1} : 변수 이름
        /// </summary>
        public static string memberFormat =
@"public {0} {1}";

        /// <summary>
        /// {0} : 변수 이름
        /// {1} : To~ 변수 형식
        /// {2} : 변수 형식
        /// </summary>
        public static string readFormat =
@"
this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof{2});
";
        /// <summary>
        /// {0} : 변수 이름
        /// </summary>
        public static string readStringFormat =
@"
ushort {0}Len = BitConverter.ToUInt16(s.Slice(pos, s.Length - pos));
pos += sizeof(ushort);
this.name = Encoding.Unicode.GetString(s.Slice(pos, {0}Len));
pos += {0}Len;
";
        /// <summary>
        /// {0} : 변수 이름
        /// {1} : 변수 형식
        /// </summary>
        public static string writeFormat =
@"
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});
";
        /// <summary>
        /// {0} : 변수 이름
        /// </summary>
        public static string writeStringFormat =
@"
ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + size + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), {0}Len);
size += sizeof(ushort);
size += {0}Len;

";
    }
}
