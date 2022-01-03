using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_KINL_Server
{
    public class MainServer
    {
        ClientManager _clientManager = new ClientManager();

        public MainServer()
        {
            Task serverStart = Task.Run(() =>
            {
                ServerRun();
            });
        }
        private void ServerRun()
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

                _clientManager.AddClient(newClient);
            }
        }
    }
}