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
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint ipEndPont = new IPEndPoint(ipAddr, 4545);
        socket = new Socket(ipEndPont.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(ipEndPont);
            Console.WriteLine($"connected");

            byte[] sendBuff = Encoding.UTF8.GetBytes("Hello");
            int sendBytes = socket.Send(sendBuff);

            byte[] recvBuff = new byte[1024];
            int recvbytes = socket.Receive(recvBuff);
            string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvbytes);
            Console.WriteLine($"[FromServer]{recvData}");


            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            while(true)
            {

            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
     
    }
}