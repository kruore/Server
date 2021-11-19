using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0;

        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        Queue<byte[]> _recvQueue = new Queue<byte[]>();

        bool _pending = false;

        object _lock = new object();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public void Start_Session(Socket socket)
        {
            _socket = socket;

            //콜백 
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);


            //recvArgs.UserToken = _socket.RemoteEndPoint;
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

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
                if (_pending == false)
                {
                    RegisterSend();
                }
            }
            //_socket.Send(sendBuff);
            //_sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
        }
        public void RegisterSend()
        {
            _pending = true;
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);
            //같은 기능 최적화

            //while(_sendQueue.Count > 0)
            //{
            //    byte[] buff = _sendQueue.Dequeue();
            //    _sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            //}

            _sendArgs.BufferList = null;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
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
                        if(_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        _pending = false;
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
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        #region 네트워크 통신 Recive
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

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
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"From Client : { recvData }");
                    // TODO
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