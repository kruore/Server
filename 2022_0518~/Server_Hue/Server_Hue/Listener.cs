using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server_Hue
{
    class Listener
    {
        Socket listenerSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint ipEndPoint, Action<Socket> onAcceptHandler, int backLog)
        {
            listenerSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;
            listenerSocket.Bind(ipEndPoint);

            listenerSocket.Listen(backLog);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = listenerSocket.AcceptAsync(args);
            if(!pending)
            {
                OnAcceptCompleted(null,args);
              
            }
        }
        void OnAcceptCompleted(object sender ,SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            
                // 접속
            }
            else
            {
                //접속 실패
            }
            RegisterAccept(args);
        }

        public Socket Accept()
        {
            return listenerSocket.Accept();
        }
    }

}
