using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using KINL_ServerCore;
using System.Text;

public enum DataType
{
    Text, Image, Video
}
public class Knight
{
    public int Hp =10;
    public int Hp2 = 100;
    public string name = "Hello World";
}


namespace _KINLAB
{
    class ServerSession : Session
    {
        public override void OnConnected(EndPoint _endPoint)
        {
            Console.WriteLine($"OnConnected : {_endPoint}");
            Knight a = new Knight() { Hp= 10, Hp2 = 100, name = "Hello World"};
            ArraySegment<byte> openSegment=SendBufferHelper.Open(4096);
            byte[] sendBuff = BitConverter.GetBytes(a.Hp2);

            Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> _buffer)
        {
            string recvData = Encoding.UTF8.GetString(_buffer.Array, _buffer.Offset, _buffer.Count);
            Console.WriteLine($"From Client : { recvData }");
            return recvData.Length;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes :  {numOfBytes}");
        }
    }
    class Server
    {
        static Listener _listener = new Listener();

        //static void OnAcceptHandler(Socket clientSocket)
        //{
        //    try
        //    {
        //        //   Session session = new Session();
        //        var session = new ServerSession();
        //        session.Start_Session(clientSocket);

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = ipHost.AddressList[0];
            int portNum = 4545;
            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNum);
            ServerSession session = new ServerSession();
            _listener.InitListener(ipEndPoint, () => { return new ServerSession(); });
            Console.WriteLine("서버 오픈");

            while (true)
            {
                ;
            }

        }
    }

}