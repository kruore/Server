using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using KINL_Server;

namespace DummyClient
{
    class Program
    {

        static void Main(string[] args)
        {
            string Host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(Host);
            IPAddress ipAddr = IPAddress.Parse("210.94.216.195");
            int port = 4545;
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddr, port);

            Connector connector = new Connector();

            connector.Connect(iPEndPoint, () => { return new ServerSession(); });

            while (true)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(1000);
            }
        }
    }
}