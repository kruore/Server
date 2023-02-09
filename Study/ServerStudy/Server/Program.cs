using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using ServerCore;

namespace Server
{
    public class Server
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            //처음에만 등록을 잘 해주면 그 후에는 문제가 없다.
            //Map을 사용해서 해당 데이터의 구분을 둔다 Protocool Settings
            PacketManager.Instance.Register();

            string host = Dns.GetHostName();
            int port = 4545;
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndpoint = new IPEndPoint(ipAddr, 4545);
            Socket socket = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 리스너 선언 및 동작

            listener.Init(ipEndpoint, 10, () => { return new ClientSession(); });
            while (true)
            {
                ;
            }
        }
    }
}