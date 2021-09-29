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

            string host = Dns.GetHostName();
            IPHostEntry iphost = Dns.GetHostEntry(host);
            //트래픽이 큰 경우 해당 사이트에 많은 주소값이 들어갈 수 도 있다.
            IPAddress ipAddr = iphost.AddressList[0];
            //Port = 식당 문 암호
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 4545);
            // 하드 코딩으로 IP를 넣으면 해결이 안되는데 해당을 도메인으로 놓고
            // ID를 찾아내면 해당 주소로 이름을 찾아내게 한다.-> 관리가 쉽다. 융통성있게...

            //휴대폰 설정
            Socket socket = new Socket(endPoint.AddressFamily,SocketType.Stream,ProtocolType.Tcp);


            try
            {
                socket.Connect(endPoint);
                Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");


                //보낸다.
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World");
                int sendBytes = socket.Send(sendBuff);

                //받는다.
                byte[] recvBuff = new byte[1024];
                int recvBytes = socket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From Server]{recvData}");

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
         
        }
    }
}
