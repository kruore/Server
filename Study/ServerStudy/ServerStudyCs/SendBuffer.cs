using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{

    public class SendBufferHelper
    {
        //멀티 스레드 환경이라 전역으로 두면 경합(레이스 컨디션)이 동작
        // 따라서 ThreadLocal
        // ThreadLocal.Value = <T> type 을 캐스팅하여 해당 인스턴스의 값을 get; set; 하는 프로퍼티
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096 * 100;
        
        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(CurrentBuffer.Value==null)
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

    //SendBuffer는 RecvBuffer과 다르게 이 친구는 일회용, 왜? 다른 친구가 참조해서 쓸 수도 있기 때문에
    //재사용을 하면 해당 사용 위치의 재 사용이 불가능해지기 때문

    /// <summary>
    /// 일회용 데이터 전송을 위한 버퍼 사이즈를 제작한다. Send Buffer
    /// </summary>
    public class SendBuffer
    {
        ArraySegment<byte> _buffer;
        int _usedSize = 0;

        public SendBuffer(int chunkSize)
        {
            _buffer = new ArraySegment<byte>(new byte[chunkSize]);

        }
        public int FreeSize { get { return _buffer.Count - _usedSize; } }
        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
            {
                return null;
            }
            return new ArraySegment<byte>(_buffer.Array, _usedSize, reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer.Array, _usedSize, usedSize);
            _usedSize += _usedSize;
            return segment;
        }

    }
}
