using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;


namespace KINL_Server
{

    class StaticDefine
    {
        public const int SHOW_CURRENT_CLIENT = 1;
        public const int SHOW_ACCESS_LOG = 2;
        public const int SHOW_CHATTING_LOG = 3;
        public const int ADD_ACCESS_LOG = 5;
        public const int ADD_CHATTING_LOG = 6;
        public const int EXIT = 0;
    }

    class MainServer
    {

        ClientManager _clientManager;
        ConcurrentBag<string> _data = null;
        ConcurrentBag<string> AccessLog = null;
        Thread connectCheckThread = null;

        public MainServer()
        {
            _clientManager = new ClientManager();
            _data = new ConcurrentBag<string>();
            AccessLog = new ConcurrentBag<string>();
            _clientManager.EventHandler += ClientEvent;
            _clientManager.messageParsingAction += MessageParsing;
            Task serverStart = Task.Run()=>
            {
                ServerRun();
            });

        }

        private void ClientEvent(string message, int key)
        {
            switch (key)
            {
                case StaticDefine.ADD_ACCESS_LOG:
                    {
                        AccessLog.Add(message);
                        break;
                    }
                case StaticDefine.ADD_CHATTING_LOG:
                    {
                        _data.Add(message);
                        break;
                    }
            }
        }


        private void AsyncServerStart()
        {
            string host = Dns.GetHostName();
            //      IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = IPAddress.Any;
            int portNum = 4545;
            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNum);

            // Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            // socket.Bind(ipEndPoint);
            // socket.Listen(10);
            TcpListener listener = new TcpListener(ipEndPoint);
            listener.Start();
            Console.WriteLine("Server Started");

            while (true)
            {
                TcpClient acceptClient = listener.AcceptTcpClient();
                ClientData clientData = new ClientData(acceptClient);
                clientData.client.GetStream().BeginRead(clientData.readByteData, 0, clientData.readByteData.Length, new AsyncCallback(DataReceived), clientData);
            }

        }

        private void DataReceived(IAsyncResult ar)
        {
            ClientData callbackClient = ar.AsyncState as ClientData;

            int byteRead = callbackClient.client.GetStream().EndRead(ar);
            string readString = Encoding.UTF8.GetString(callbackClient.readByteData, 0, byteRead);

            Console.WriteLine($"{callbackClient.clientNumber}번 사용자 : {readString}");

        }

        static void Main(string[] args)
        {
            MainServer server = new MainServer();
        }
    }
}