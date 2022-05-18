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

        static void Main(string[] args)
        {
            Socket socket;
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEnd = new IPEndPoint(ipAddr, 4545);
            listener.Init(ipEnd);
            try
            {
                Console.WriteLine("Listen");
                while (true)
                {
                  

                    Socket client = listener.Accept();


                    byte[] recvBuff = new byte[1024];
                    int recvBytes = client.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Client]{recvData}");

                    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello");
                    client.Send(sendBuff);

                    //socket.Shutdown(SocketShutdown.Both);
                    //socket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }

        }

        public void Init(IPEndPoint endPoint)
        {

        }

    }
}
