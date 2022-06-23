using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        Socket socket;
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        //  IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint ipEndPont = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4545);
        socket = new Socket(ipEndPont.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(ipEndPont);
            Console.WriteLine($"connected");

            for (int i = 0; i < 10; i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes("16");
                int sendBytes = socket.Send(sendBuff);
                Console.WriteLine($"SendSuccessfully");
            }

            byte[] recvBuff = new byte[1024];
            int recvbytes = socket.Receive(recvBuff);
            string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvbytes);
            Console.WriteLine($"[FromServer]{recvData}");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}