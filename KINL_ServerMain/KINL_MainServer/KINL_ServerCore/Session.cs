using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_ServerCore
{

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;
        
        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        Queue<byte[]> _recvQueue = new Queue<byte[]>();

        object _lock = new object();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint _endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);


        public void Start_Session(Socket socket)
        {
            _socket = socket;

            //콜백 
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            //recvArgs.UserToken = _socket.RemoteEndPoint;
            //_recvArgs.SetBuffer(new byte[1024], 0, 1024);

            //콜백 
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            RegisterRecv(_recvArgs);
        }
        #region 네트워크 통신 Send
        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
            //_socket.Send(sendBuff);
            //_sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
        }
        public void RegisterSend()
        {

            // 리스트로 대체
            //_pending = true;

            // byte[] buff = _sendQueue.Dequeue();
            //_sendArgs.SetBuffer(buff, 0, buff.Length);
            //같은 기능 최적화

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }

            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (_pendingList.Count < 0)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }
        public void OnSendCompleted(object send, SocketAsyncEventArgs args)
        {
            // 콜백이라서 다른 스레드에서 부를수도 있기 떄문에 락
            lock (_lock)
            {
                if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
                    try
                    {

                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        //Console.WriteLine($"Transferred bytes :  {_sendArgs.BytesTransferred}");

                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnSendCompleted Failed{ex}");
                    }
            }
        }
        #endregion
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
                return;
            }
            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        #region 네트워크 통신 Recive
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
            {
                OnRecvCompleted(null, args);
            }
        }
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {

            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write 커서 이동
                    if(_recvBuffer.OnWrite(args.BytesTransferred)==false)
                    {
                        Disconnect();
                        return;
                    }
                    // Data Send
                    
                    int processLen =  OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return; 
                    }

                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }
                    //string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    //Console.WriteLine($"From Client : { recvData }");
                    //// TODO
                    RegisterRecv(args);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                // Disconnect
            }
        }
        #endregion

    }
}