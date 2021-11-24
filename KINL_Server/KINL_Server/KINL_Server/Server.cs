using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

public enum DataType
{
    Text = 0, File = 1, Image = 2, Video = 3
}

class Server
{
    public Server()
    {
        AsyncServerStart();
    }
    
    private void AsyncServerStart()
    {
        int port = 4545;
        TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
        listener.Start(); 
        Console.WriteLine("서버를 시작합니다. 접속 대기중");

        TcpClient acceptClient = listener.AcceptTcpClient();
        Console.WriteLine("클라이언트 접속성공.");

        ClientData clientData = new ClientData(acceptClient);


        while (true)
        {
            TcpClient acceptClient = listener.AcceptTcpClient();
            ClientData 
        }
    }

    static void Main(string[] args)
    {

        string host = Dns.GetHostName();

        // Port Define 4545
        int portNum = 4545;


        IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, portNum);
        Console.WriteLine($" Server Open : Host IP = {iPAddress} ");

        while (true)
        {
            ;
        }
    }

}