﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    class PacketFormat
    {
        public static string managerFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;


 class PacketManager
    {{
        #region SingleTon
        static PacketManager _instance;
        public static PacketManager Instance
        {{
            get
            {{
                if (_instance == null)
                {{
                    _instance = new PacketManager();
                }}
                return _instance;
            }}
        }}
        #endregion


        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
        Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession,IPacket>>();

        public void Register()
        {{
{0}
        }}
        public void OnRecvPacket(PacketSession session,ArraySegment<byte> buffer)
        {{
            int pos = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            pos += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + pos);
            pos += 2;

            Action<PacketSession, ArraySegment<byte>> action = null;
            if(_onRecv.TryGetValue(id, out action))
            {{
                action.Invoke(session, buffer);
            }}
        }}

        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
{{
    T p = new T();
    p.Read(buffer);

    Action<PacketSession, IPacket> action = null;
    if (_handler.TryGetValue(p.Protocol, out action))
        action.Invoke(session, p);
}}
    }}

";

        public static string managerRegisterFormat =
@"
        _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);
";

        //{0} 패킷 이름/ 번호
        //{1} 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

public enum PacketID
{{
    {0}
}}

{1}

interface IPacket
{{
    ushort Protocol {{ get; }}
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}}
";

        public static string packetEnumFormat =
@"{0} = {1},";






        /// <summary>
        /// {0} : 패킷 이름
        /// {1} : 맴버 변수들
        /// {2} : 맴버 변수의 Read
        /// {3} : 맴버 변수들의 Write
        /// </summary>
        public static string packetFormat =
@"
 class {0} : IPacket
    {{
        {1}
    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}
    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


        count += sizeof(ushort);
        count += sizeof(ushort);
        
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
@"public {0} {1};";

        // {0} : 리스트 이름 [대문자]
        // {1} : 리스트 이름 [소문자]
        public static string readListFormat =
@"
this.{1}s.Clear(); 
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(s, ref count);
    {1}s.Add({1});
}}
";
        // {0} : 리스트 이름 [대문자]
        // {1} : 리스트 이름 [소문자]
        public static string writeListFormat =
@"
success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), (ushort)this.{1}s.Count);
size += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
{{
    success &= {1}.Write(s, ref size);
    //TODO
}}

";
        /// <summary>
        /// {0} : 변수 이름
        /// {1} : To~ 변수 형식
        /// {2} : 변수 형식
        /// </summary>
        public static string readFormat =
@"
this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});
";
        /// <summary>
        /// {0} : 변수 이름
        /// </summary>
        public static string readStringFormat =
@"
ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.name = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;
";
        /// <summary>
        /// {0} : 변수 이름
        /// {1} : 변수 형식
        /// </summary>
        public static string writeFormat =
@"
success &= BitConverter.TryWriteBytes(s.Slice(size, s.Length - size), this.{0});
size += sizeof({1});
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

        // {0} : 리스트 이름 [대문자]
        // {1} : 리스트 이름 [소문자]
        // {2} : 맴버 변수들
        // {3} : 맴버 변수 Read
        // {4} : 맴버 변수 write
        public static string memberListFormat =
@"       
public struct {0}
{{
   {2}

    public void Read(ReadOnlySpan<byte> s, ref ushort count)
    {{
     {3}
    }}

    public bool Write(Span<byte> s, ref ushort size)
    {{
        bool success = true;
    {4}
        return success;
    }}

}}

public List<{0}> {1}s = new List<{0}>();

";
    }
}
