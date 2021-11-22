using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using System.Text;


namespace _KINL_DummyClient
{
    class Client
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = ipHost.AddressList[0];
            int portNum = 4545;
            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNum);

            while (true)
            {
                Socket socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {

                    socket.Connect(ipEndPoint);
                    Console.WriteLine($"Connect To {socket.RemoteEndPoint.ToString()}");

                    for(int i = 0; i< 1000; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello? : {i}");
                        int sendByte = socket.Send(sendBuff);
                    }

                    byte[] reciveBuff = new byte[1024];
                    int reciveByte = socket.Receive(reciveBuff);
                    string recvData = Encoding.UTF8.GetString(reciveBuff, 0, reciveByte);
                    Console.WriteLine($"Server Said : {recvData}");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

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