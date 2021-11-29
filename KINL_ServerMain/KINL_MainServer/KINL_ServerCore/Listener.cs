using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace KINL_ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        //해당 작업 변경
        //Action<Socket> _onAcceptHandler;
        Func<Session> _sessionFactory;

        public void InitListener(IPEndPoint ipEndPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            _listenSocket.Bind(ipEndPoint);

            _listenSocket.Listen(100);


            // 이걸 여러개 만들면. 빠르게 많은 장비를 사용한다.
            //for(int i = 0; i< 100; i++)
            //{
            //    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            //    args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            //    RegisterAccept(args);
            //}

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            //2바퀴 돌면 null이 아니라서 이미 접속중이니까 제외
            args.AcceptSocket = null;

            bool pending=_listenSocket.AcceptAsync(args);

            //Pending 없이 바로 시전됐다면.
            if(pending ==false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        //멀티 스레드로 가능 할 것이라는 문제가 있으니 해결해보자...
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                // TODO
                // 콜백으로 던져준다.
                Session session = _sessionFactory.Invoke();
                session.Start_Session(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
                //_onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
            RegisterAccept(args);
        }

    }
}
