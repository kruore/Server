using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using KINL_Server;

class KINL_ServerCore
{
    public KINL_ServerCore()
    {
        AsyncServerStarted();
    }

    public void AsyncServerStarted()
    {
        int port = 4545;
        IPAddress address = IPAddress.Any;
        IPEndPoint iPEndPoint = new IPEndPoint(address, port);

        TcpListener listener = new TcpListener(iPEndPoint);
        listener.Start();


        Console.WriteLine("Server Started");

        while (true)
        {
            Console.WriteLine("Waiting");
            TcpClient acceptClient = listener.AcceptTcpClient();
            ClientData clientData = new ClientData(acceptClient);
            clientData._client.GetStream().BeginRead(clientData._recvData, 0, clientData._recvData.Length, new AsyncCallback(DataRecived), clientData);
            Console.WriteLine("Accept");
        }
    }

    private void DataRecived(IAsyncResult ar)
    {
        try
        {
            ClientData callbackClient = ar.AsyncState as ClientData;
            int byteRead = callbackClient._client.GetStream().EndRead(ar);
            string readString = Encoding.Default.GetString(callbackClient._recvData, 0, byteRead);

            Console.WriteLine($"{callbackClient._clientNumber}의 사용자 : {readString}");
            callbackClient._client.GetStream().BeginRead(callbackClient._recvData, 0, callbackClient._recvData.Length, new AsyncCallback(DataRecived), callbackClient);
        }
        catch (Exception ex)
        {

        }
    }

    static void Main(string[] args)
    {
        KINL_ServerCore a = new KINL_ServerCore();
    }
}