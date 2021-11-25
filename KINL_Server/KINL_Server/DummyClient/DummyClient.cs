using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


class DummyClient
{
    TcpClient client = null;
    public void Run()
    {
        while (true)
        {
            Console.WriteLine("=============클라이언트 오픈=============");
        }
    }


    static void Main(string[] args)
    {
    
        DummyClient client = new DummyClient();
        client.Run();

    }

    private void Connect()
    {
        client = new TcpClient();
        client.Connect("210.94.216.195", 4545);
    }
}