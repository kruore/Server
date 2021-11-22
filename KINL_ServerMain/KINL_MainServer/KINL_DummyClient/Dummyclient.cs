using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using KINL_ServerCore;
using System.Text;


namespace _KINL_DummyClient
{
    class ClientSession : Session
    {
        public override void OnConnected(EndPoint _endPoint)
        {
            Console.WriteLine($"OnConnected : {_endPoint}");

            for (int i = 0; i < 5; i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello? : {i}");
                Send(sendBuff);
            }

        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"From Server : { recvData }");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes :  {numOfBytes}");
        }
    }
    class Client
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = ipHost.AddressList[0];
            int portNum = 4545;
            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNum);

            Connector connector = new Connector();

            connector.Connect(ipEndPoint, () => { return new ClientSession(); });

            while (true)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(100);
            }

        }
    }

}