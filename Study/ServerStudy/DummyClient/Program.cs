using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using ServerCore;

namespace DummyClient
{
    public class DummyClient
    {
        static void Main(string[] args)
        {

            string host = Dns.GetHostName();
            int port = 4545;
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipAddr, 4545);

            Connector connector = new Connector();
            connector.Connect(endpoint, () => { return new ServerSession(); });

            Func<Session> sessionFactory;

            while (true)
            {
                //소켓은 새로운 접속을 진행할 때 마다 한번 씩 집어넣어 해결해야한다.
                Socket server = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(10);
            }
        }
    }
}