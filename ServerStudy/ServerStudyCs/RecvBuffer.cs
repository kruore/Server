using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;


        /// <summary>
        /// 리시브 버퍼
        /// 버퍼에 bufferSize 만큼의 데이터를 담아 해당 데이터를 해독할 수 있어야 한다.
        /// 매번 새로운 버퍼를 구현하여 하는 것은 조금 그렇다. 추후 -> 리드,라이트의 버퍼를 크게 제작하여
        /// 오프셋부터 데이터를 넣을 수 있도록 구현할 예정이다.
        /// </summary>
        /// <param name="bufferSize"></param>
        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }
        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get
            {
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);
            }
        }
        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                //남은 데이터가 없으면 커서 리셋
                _readPos = 0;
                _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }
        public bool OnRead(int numberOfBytes)
        {
            if (numberOfBytes > DataSize)
                return false;
            _readPos += numberOfBytes;
            return true;
        }
        public bool OnWrite(int numberOfBytes)
        {
            if (numberOfBytes > FreeSize)
                return false;
            _writePos += numberOfBytes;
            return true;
        }

    }
}
