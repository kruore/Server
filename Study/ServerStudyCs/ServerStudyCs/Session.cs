using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        bool _pending = false;

        ArraySegment<byte> _sendBuff = new byte[1024];

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        List<ArraySegment<byte>> _sendPendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        abstract public void OnConneceted(EndPoint endpoint);
        abstract public void OnSend(int numOfBytes);
        abstract public int OnRecv(ArraySegment<byte> buffer);
        abstract public void OnDiscoonected(EndPoint endPoint);
        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            RegisterRecv();
        }

        public void Disconnect()
        {

            // 이 경우 멀티 스레드 환경에서 보호받지 못함.
            //if(socket!=null)

            // 같은 아이가 2번 디스커넥트 하는 환경을 조성하지 않도록
            // 디스커넥트의 체크를 lock을 통해 진행하는 것이 좋다.

            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
                OnDiscoonected(_socket.RemoteEndPoint);
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }
        public void Send(ArraySegment<byte> sendBuff)
        {

            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);

                if (_sendPendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
            // 버퍼 세팅 버퍼는 이거구, offset부터 1024 사이즈까지
        }
        #region Data Send Region
        public void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();

                //C++ 이었다면 포인터를 통해서 진행했겠지만 여긴 C#
                // 따라서 배열의 첫 위치만을 알 수 있다.
                // 그래서 배열의 첫번째, 사이즈, 길이를 알려주어 값을 구한다.

                //ArraySegment 는 구조체라서 메모리가 아닌 힙의 영역에 올라간다.
                _sendPendingList.Add(buff);
            }
            _sendArgs.BufferList = _sendPendingList;

            //버퍼리스트는 모든 목록을 가지고 있을 것이다.

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }
        public void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {


                        _sendArgs.BufferList = null;
                        _sendPendingList.Clear();
                        OnSend(_sendArgs.BytesTransferred);
                        Console.WriteLine($"Transfered : Bytes {args.BytesTransferred.ToString()}");

                        // 만약  _sendPending 에서의 시간에 다른 스레드가 집어넣는다면.
                        if (_sendPendingList.Count > 0)
                        {
                            RegisterSend();
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
                else
                {
                    // 전송을 했는데 상대가 못 받는 상태라면...
                    Disconnect();
                }
            }
        }
      

        #endregion

        #region Recv Data Region
        public void RegisterRecv()
        {
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs);

            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }

        }

        public void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 만약 전송 된 바이트의 수가 0보다 크고, 소켓이 Success 라면..
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write 커서 이동 및 처리 
                    if(_recvBuffer.OnWrite(args.BytesTransferred)==false)
                    {
                        Disconnect();
                        return;
                    }
                    int processLen=OnRecv(_recvBuffer.ReadSegment); 
                    if(processLen<0|| _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    //Read 커서 이동 및 처리

                    if(_recvBuffer.OnRead(processLen)==false)
                    {
                        Disconnect();
                        return;
                    }
                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                Disconnect();
            }

        }
        #endregion
    }
}
