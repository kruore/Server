using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;
        // Sealed를 붙였을 떄 , 다른 클래스에서 얘를 오버라이드 하려고하면 오류남
        // 즉 이친구를 사용해야한다는것
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            while(true)
            {
                //최소한의 헤더 사이즈보다 작다면
                if (buffer.Count >= HeaderSize)
                    break;

                //패킷이 완전체로 도착했는지 확인

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if(buffer.Count< dataSize)
                {
                    break;
                }
                // 여기부턴 패킷 조립 가능
                // 어차피 구조체고 class가 아니라서 힙 영역이 아닌 스택영역 마음대로 불러도 됨.
                OnRecvPacket(new ArraySegment<byte>(buffer.Array,buffer.Offset,dataSize));

                //buffer.Slice = 커팅하는 것이 가능

                processLen += dataSize;
                // 버퍼의 처리한 녀석만큼을 지우고 다음 녀석을 들고 온다.
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count-dataSize);
            }
            return processLen;
        }

        //대신 이 친구를 인터페이스로 써라

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);

    }
}
