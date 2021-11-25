using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
{
    public class Connector
    {
        public void Connect(IPEndPoint endPoint)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnRegisterConnected;
            Console.WriteLine(endPoint);
        }

        public void OnRegisterConnected(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if(socket == null)
            {
                Console.WriteLine(socket.RemoteEndPoint.ToString());
                return;
            }
            bool pending = socket.ConnectAsync(args);
            if(pending ==false)
            {
                OnRegisterConnected(null,args);
            }

        }

        public void OnRegisterConnected(object sender, SocketAsyncEventArgs args)
        {

            if(args.SocketError == SocketError.Success)
            {
                Console.WriteLine("Connect");
            }
        }

    }
}
