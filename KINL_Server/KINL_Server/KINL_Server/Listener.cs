using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
{
    public class Listener
    {

        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint iPEndPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;


            _listenSocket.Bind(iPEndPoint);

            _listenSocket.Listen(10);

            SocketAsyncEventArgs args  = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }


        public void OnAcceptCompleted(object sender , SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                // 접속 된거

                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                // 접속 실패
                _listenSocket.Shutdown(SocketShutdown.Both);
                _listenSocket.Close();
                Console.WriteLine(args.SocketError.ToString());
            }
            RegisterAccept(args);
        }

        public void RegisterAccept(SocketAsyncEventArgs args)
        {

            //만약 소켓이 이미 자료를 들고 있을 수 있으니 미리 날리기
            args.AcceptSocket = null;

            //Pending = 만약에 접속이 안되었다면
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }

        }
    }
}
