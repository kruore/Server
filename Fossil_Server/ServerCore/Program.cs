using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry iphost = Dns.GetHostEntry(host);
            //트래픽이 큰 경우 해당 사이트에 많은 주소값이 들어갈 수 도 있다.
            IPAddress ipAddr = iphost.AddressList[0];
            //Port = 식당 문 암호
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 4545);
            // 하드 코딩으로 IP를 넣으면 해결이 안되는데 해당을 도메인으로 놓고
            // ID를 찾아내면 해당 주소로 이름을 찾아내게 한다.-> 관리가 쉽다. 융통성있게...

            //문지기의 휴대폰을 만들어줌
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            try
            {
                //문지기 교육-> 

                listenSocket.Bind(endPoint);

                //backlog = 10 : 최대 대기수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Listen");

                    //손님 입장
                    //retrun 값 socket
                    Socket clientSocket = listenSocket.Accept();

                    //받는다.
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    //recvBuff,시작바이트,end
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    for (int i = 0; i < 100000; i++)
                    {
                        Console.WriteLine($"[FromClient]{recvData}");
                    }

                    //보낸다.
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server");


                    //서버에서 안받아주면? 계속 대기가 될건데... 그걸 어떻게 처리할까?
                    clientSocket.Send(sendBuff);

                    //쫒아낸다.
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

        }
    }
}
