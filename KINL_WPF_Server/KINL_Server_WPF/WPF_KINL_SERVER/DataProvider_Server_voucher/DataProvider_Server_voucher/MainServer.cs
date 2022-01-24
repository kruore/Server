using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider_Server_voucher
{
    internal class MainServer
    {
        ClientManager _clientManager = new ClientManager();

        public MainServer()
        {
            Task serverStart = Task.Run(() =>
            {
                ServerStart();
            });
        }

        private void ServerStart()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 4545));
            listener.Start();
            Console.WriteLine("Server Opened");

            while (true)
            {
                Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
                acceptTask.Wait();

                Console.WriteLine("SomeClientConnect");
                TcpClient newClient = acceptTask.Result;
                //Console.WriteLine(newClient.Client.LocalEndPoint.ToString());
                //Console.WriteLine(newClient.Client.RemoteEndPoint.ToString());

                _clientManager.AddClient(newClient);
            }
        }

    }
}
