using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;



namespace KINL_Server
{
    class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;
            _listenSocket.Bind(endPoint);

            _listenSocket.Listen(5);
            SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        } 
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if(pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError==SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine("에러");
            }
            RegisterAccept(args);
        }
    }
}
