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
        TcpClient tcpClient = null;
        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("210.94.216.195"), 4545);

        tcpClient = new TcpClient();

        tcpClient.Connect("210.94.216.195", 4545);
        for (int i = 0; i < 1000; i++)
        {
            byte[] buf = Encoding.Default.GetBytes(tcpClient.Client.RemoteEndPoint.ToString()+$"클라이언트2 is Comming~+{i}");
            tcpClient.GetStream().Write(buf, 0, buf.Length);
            Thread.Sleep(1000);
        }
        
        while (true)
        {
             ;
        }
    }
}

