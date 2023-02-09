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