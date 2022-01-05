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
        for (int i = 0; i < 5; i++)
        {
            byte[] buf = Encoding.Default.GetBytes($"vp,1,ID,Name+{i},Date,Data,Controll,Device,DeviceData");
            tcpClient.GetStream().Write(buf, 0, buf.Length);
            Thread.Sleep(1000);
        }
        byte[] bu2f = Encoding.Default.GetBytes("vp,3,ID,Name,Date,Data,Controll,Device,DeviceData");
        tcpClient.GetStream().Write(bu2f, 0, bu2f.Length);


        byte[] recvbuff = new byte[1024];
        



        while (true)
        {
             ;
        }
    }
}

