using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;


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

            while(true)
            {
                Socket socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(iPEndPoint);
                    Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");

                    byte[] buffer = Encoding.UTF8.GetBytes("Hello World");
                    int sendBytes = socket.Send(buffer);
                    byte[] recvBuffer = new byte[1024];
                    int recvBytes = socket.Receive(recvBuffer);
                    string recvData = Encoding.UTF8.GetString(recvBuffer);
                    Console.WriteLine($"Form Server { recvData }");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

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