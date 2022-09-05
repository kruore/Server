using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace ServerCore
{
    /// <summary>
    /// 서버는 리스너가 있으면 되지만 왜 커넥터가 필요한가?
    /// 1. 서버를 하나짜리로 만들것인지 분산처리를 할 것인지에 따라 나뉘는데 그때를 위해서
    /// 2. 서버를 메인 용도로 만들고 있지만 커넥트 하는 부분은 공용 라이브러리로 빼기위해
    /// </summary>
    public class Connector
    {

        //이게 아닌 인자로 쓰는 이유는 여러개가 커넥트 될 수 있기 때문
        //Socket _socket;
        Func<Session> _sessionFactory;
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnCompletedConnect;
            args.RemoteEndPoint =endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }

        #region Connected Region

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            // 소켓을 유저 토큰에서 소켓을 받아와 클래스로 캐스팅하여 쓴다.
            Socket socket = args.UserToken as Socket;

            if(socket == null)
            {
                return;
            }
           bool pending = socket.ConnectAsync(args);
            if(pending==false)
            {
                OnCompletedConnect(null, args);
            }
        }
        void OnCompletedConnect(object sender,SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                //덮어 씌워서 게임 세션이 아닌 다른 세션도 받을 수 있도록한다.
                //Invoke 를 이용한 생성 = 멀티 스레드에서의 생성시 데이터 보호 = 메인 스레드로의 작업 위임
               Session session = _sessionFactory.Invoke();

                //내가 연결한 소켓이다.
               session.Start(args.ConnectSocket);
               session.OnConneceted(args.ConnectSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine("onConnected Completed Fail!!!");
            }
        }
        #endregion
    }
}
