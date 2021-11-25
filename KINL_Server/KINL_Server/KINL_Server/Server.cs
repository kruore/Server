using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
{
    public enum connectCheckProtocool
    {
        check = 0,
        connect = 1,
        disconnect = 2,

        send_message = 3,
        recv_message = 4,

        send_text = 5,
        recv_text = 6,

        send_file = 7,
        recv_file = 8,

    }

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
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = IPAddress.Parse("210.94.216.195"); ;
            int portNum = 4545;
            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNum);

            //Connecter.
            Connector connector = new Connector();
            connector.Connect(ipEndPoint);

        }
    }
}