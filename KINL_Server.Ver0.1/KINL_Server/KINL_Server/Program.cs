using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

class Parogram
{
    static void Main(string[] args)
    {
        string host= Dns.GetHostName();
        int port = 4545;
        IPAddress iPAddress = IPAddress.Any;
        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port);

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(iPEndPoint);
        Console.WriteLine($"Connect Server");

        byte[] buffer = new byte[1024];
        int sendBytes = socket.Send(buffer);

        byte[] recvBuff = new byte[1024];
        int recvBytes = socket.Receive(recvBuff);
        string recvData = Encoding.UTF8.GetString()

    }
}