using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KINL_ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBufer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(CurrentBufer.Value == null)
            {
                CurrentBufer.Value = new SendBuffer(ChunkSize);
            }
            if (CurrentBufer.Value.FreeSize < reserveSize)
            {
                CurrentBufer.Value = new SendBuffer(ChunkSize);
            }
            return CurrentBufer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int userSize)
        {
            return CurrentBufer.Value.Close(userSize);
        }
    }
    public class SendBuffer
    {

        byte[] _buffer;
        int _userSize = 0;

        public int FreeSize { get { return _buffer.Length - _userSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
            {
                return null;
            }
            return new ArraySegment<byte>(_buffer, _userSize, reserveSize);

        }
        public ArraySegment<byte> Close(int userSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _userSize, userSize);
            _userSize += userSize;
            return segment;
        }
    }
}
