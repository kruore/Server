using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using KINL_Server;

namespace Server
{
    class Server
    {
        static Listener _listener = new Listener();

        public static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            int port = 4545;
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

            _listener.init(ipEndPoint, () => { return new ServerSession(); });
            Console.WriteLine("Server Started");
            while (true)
            {
                ;
            }
        }

    }
}