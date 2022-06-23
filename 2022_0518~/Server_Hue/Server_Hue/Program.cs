using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server_Hue
{

    class program
    {
        static Listener listener = new Listener();
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Start(clientSocket);
                byte[] sendBuff = Encoding.UTF8.GetBytes("#4");
                session.Send(sendBuff);

                Thread.Sleep(1000);
                session.Disconnect();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        static void Main(string[] args)
        {

            Socket socket;
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // IPAddress ipAddr = ipHost.AddressList[0];
            IPAddress iPAddr = IPAddress.Any;
            IPEndPoint ipEnd = new IPEndPoint(iPAddr, 4545);
            Console.WriteLine(ipEnd);
            listener.Init(ipEnd, OnAcceptHandler, 10);

            try
            {
                Console.WriteLine("Listening...");
                while (true)
                {
                    ;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }

        }


    }
}
