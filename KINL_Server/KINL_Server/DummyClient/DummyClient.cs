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
        while(true)
        {
            string key = Console.ReadLine();
            int order = 0;


            if(int.TryParse(key, out order))
            { 
                switch(order)
                {

                    case 1:

                        if(client != null)
                        {
                            Console.WriteLine("Already Connect");
                            Console.ReadKey();

                        }
                        else
                        {
                            Connect();
                        }
                        break;
                 case 2:

                        if (client != null)
                        {
                            Console.WriteLine("서버와 선 연결 필요");
                            Console.ReadKey();

                        }
                        else
                        {
                            SendMessage();
                        }

                        break;

                }
            }
            else
            {
                Console.WriteLine("서버와 선 연결 필요");
                Console.ReadKey();
            }
            Console.Clear();
        }
    }

    private void SendMessage()
    {
        Console.WriteLine("보낼 매세지를 작성해주세요");
    
        string message = "128,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8";
        byte[] byteData = new byte[message.Length];

        client.GetStream().Write(byteData, 0, byteData.Length);
        Console.WriteLine("전송됨");
    }

    static void Main(string[] args)
    {
    
        DummyClient client = new DummyClient();

    }

    private void Connect()
    {
        client = new TcpClient();
        client.Connect("210.94.216.195", 4545);
        Console.WriteLine("서버 연결에 성공했습니다.");
        Console.ReadKey();
    }
}