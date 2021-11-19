using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using System.Text;

public enum DataType
{
    Text, Image, Video
}

namespace KINL_ServerCore
{
    class Server
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Start_Session(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Server");
                session.Send(sendBuff);

                Thread.Sleep(1000);
                session.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = ipHost.AddressList[0];
            int portNum = 4545;
            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNum);

            _listener.InitListener(ipEndPoint, OnAcceptHandler);
            Console.WriteLine("서버 오픈");

            while(true)
            {
                ;
            }
          
        }
    }

}