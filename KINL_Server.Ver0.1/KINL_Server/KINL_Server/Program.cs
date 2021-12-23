using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using KINL_Server;



class StaticDefine
{
    public const int SHOW_CURRENT_CLIENT = 1;
    public const int SHOW_ACCESS_LOG = 2;
    public const int SHOW_DATA_LOG = 3;
    public const int ADD_ACCESS_LOG = 5;
    public const int ADD_CHATTING_LOG = 6;
    public const int EXIT = 0;
}

public class KINL_ServerCore
{
    //ClientManager clientManager = null;
    ConcurrentBag<string> AccessLog = null;
    ConcurrentBag<string> DataLog = null;
    Thread connectCheckThread = null;
    
    public KINL_ServerCore()
    {
        Task serverStart = Task.Run(() =>
        {
            AsyncServerStarted();
        });
        connectCheckThread = new Thread(connectCheck);
        connectCheckThread.Start();
        DataLog = new ConcurrentBag<string>();
        AccessLog = new ConcurrentBag<string>();
    }
    public void connectCheck()
    {
        while(true)
        {
            foreach(var item in ClientManager.clientDic)
            {

            }
        }
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