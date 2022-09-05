using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {

        Socket _listenSocket;
        /// <summary>
        /// 이 녀석은 Action으로 Socket에 관련된 처리를 하는데 해당 처리를 변경하여 Func로 진행
        /// </summary>
        /// Action<Socket> _OnAcceptHandler;

        
        /// Func 는 return Type 이 있다.
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, int backlog, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(backlog);

            // 선언 후 재사용을 무한히 할 수 있다는 장점이 있다.
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }
        /// <summary>
        /// 접속 전 데이터를 pending을 반환하여, 해당 데이터가 들어왔을 경우 접속 승인을 진행한다.
        /// </summary>
        /// <param name="args"></param>
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 이벤트 재사용 시 , 초기화 시켜주는 것이 중요함
            args.AcceptSocket = null;

            var pending = _listenSocket.AcceptAsync(args);

            if (pending == false)
                OnAcceptCompleted(null, args);
        }


        /// <summary>
        /// 접속 가능 상태가 되었다면 pending==false로 전환되며 _OnAcceptHandler의 Invoke를 통해 args.AcceptSocket을 진행한다.
        /// </summary>
        /// <param name="sender">이벤트의 호출 델리게이트를 맞추기 위해 진행</param>
        /// <param name="args">SocketAsyncEventArgs를 호출하여 소켓의 이벤트 Args를 읽는다.</param>
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {

            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConneceted(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            // 이벤트 재사용을 통한 효율성 증대 작업 진행
            RegisterAccept(args);
        }
        public Socket Accept()
        {
            return _listenSocket.Accept();
        }
    }
}
