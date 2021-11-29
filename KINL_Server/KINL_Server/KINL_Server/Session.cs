using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
{
    class Session
    {

        Socket _socket;
        int _disconnect = 0;

        object _lock = new object();
        bool _pending = false;

        Queue<byte[]> _sendQueue = new Queue<byte[]>();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

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
        }

        void RegisterSend()
        {
            _pending = true;

            while(_sendQueue.Count>0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }


            bool pending = _socket.SendAsync(_sendArgs);
            if (pending)
                OnSendCompleted(null, _sendArgs);
        }
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        else
                        {
                            _pending = false;
                        }

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
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnect, 1) == 1)
            {
                return;
            }

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] : {recvData}");
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

    }
}
