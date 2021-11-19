using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using System.Text;

public enum DataType
{
    Text, Image, Video
}

namespace _KINLAB
{
    class Server
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = ipHost.AddressList[0];
            int portNum = 4545;
            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNum);

            Socket _socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {


                while (true)
                {
                    Console.WriteLine(Dns.GetHostEntry(host).ToString());
                    Console.WriteLine(iPAddress.ToString());

                    // Client = Bind ipEndPoint
                    Socket clientSocket = _socket.Accept();
                    // Reciver

                    byte[] recvBuff = new byte[1024];
                    int recvByte = clientSocket.Receive(recvBuff);

                    //전송 데이터 인코딩
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvByte);
                    Console.WriteLine($"받는 데이터 : {recvData}");

                    //Sender

                    byte[] sendBuff = new byte[1024];
                    int sendByte = clientSocket.Send(sendBuff);

                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}