using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace KINL_Server
{
    public class Server
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Start(clientSocket);
                byte[] sendBuffer = Encoding.UTF8.GetBytes("Server Connected");
                session.Send(sendBuffer);

                Thread.Sleep(1000);

                session.Disconnect();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            int port = 4545;
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

            _listener.init(ipEndPoint, OnAcceptHandler);
            Console.WriteLine("Server Started");
            while (true)
            {
                ;
            }
        }

    }

}