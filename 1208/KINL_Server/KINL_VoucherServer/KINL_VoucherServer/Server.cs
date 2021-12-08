using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_VoucherServer
{
    class Server
    {
        public Server()
        {
            AsyncServerStart();
        }
        public void AsyncServerStart()
        {
            int port = 4545;
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            TcpListener tcpListener = new TcpListener(endpoint);

            tcpListener.Start();

            while(true)
            {
                TcpClient accepClient = tcpListener.AcceptTcpClient();
                ClientData clientData = new ClientData(accepClient);
            }
        }
    }
}
