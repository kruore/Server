﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KINL_ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;


        //그그 커서 깜빡이는 것 처럼, 그 버퍼의 읽기 쓰기 위치
        int _readPos;
        int _writePos;


        public RecvBuffer(int buffer)
        {
            _buffer = new ArraySegment<byte>(new byte[buffer], 0, buffer);
        }
        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get
            {
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);
            }
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
            if(dataSize ==0)
            {
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
        public bool OnRead(int numOfBytes)
        {
            if(numOfBytes > DataSize)
            {
                return false;
            }
            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if(numOfBytes > FreeSize)
            {
                return false;
            }
            _writePos += numOfBytes;
            return true;
        }
    }
}
