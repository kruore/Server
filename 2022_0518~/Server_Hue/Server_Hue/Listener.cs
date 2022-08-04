using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server_Hue
{
    public class Listener
    {
        Socket listenerSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint ipEndPoint, Func<Session> sessionFactory, int backLog)
        {
            listenerSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;
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
                Session session = _sessionFactory.Invoke();
                args.UserToken = sender;
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
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
