using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
{

    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(()=>{return null; });

        public static int ChunkSize { get; set; } = 4096;
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(CurrentBuffer.Value ==null)
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }
            if(CurrentBuffer.Value.FreeSize < reserveSize)
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }
            return CurrentBuffer.Value.Open(reserveSize);
        }
        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        //recvBuffer는 세션마다였지만.
        //SendBuffer는 세션 밖에서 보내는 순간에 외부에서 데이터를 만들어준다.
       byte[] _buffer;
       int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if(reserveSize > FreeSize)
            {
                return null;
            }
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);  
            _usedSize += usedSize;
            return segment;
        }



    }
}
