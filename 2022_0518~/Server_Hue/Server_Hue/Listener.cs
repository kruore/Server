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
        public void Init(IPEndPoint ipEndPoint)
        {
            listenerSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listenerSocket.Bind(ipEndPoint);

            listenerSocket.Listen(10);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }
        void RegisterAccept(SocketAsyncEventArgs args)
        {
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
