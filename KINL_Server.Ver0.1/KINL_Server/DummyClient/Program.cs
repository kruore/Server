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
        byte[] buf = Encoding.Default.GetBytes("%^&IPHONE");
        tcpClient.GetStream().Write(buf, 0, buf.Length);

        for (int i = 0; i < 50000; i++)
        {
            byte[] buf2 = Encoding.Default.GetBytes($"DEVICE,TIME,4,DATA : {i};");
            tcpClient.GetStream().Write(buf2, 0, buf2.Length);
            Thread.Sleep(10);
        }
        byte[] bu2f = Encoding.Default.GetBytes("vp,3,ID,Name,Date,Data,Controll,Device,DeviceData");
        tcpClient.GetStream().Write(bu2f, 0, bu2f.Length);


        byte[] recvbuff = new byte[1024];
        

        while (true)
        {
             ;
        }
        Console.ReadLine();
    }
}

