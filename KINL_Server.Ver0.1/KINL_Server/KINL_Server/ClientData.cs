using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
{
    class ClientData
    {

        public TcpClient _client { get; set; }
        public byte[] _recvData { get; set; }
        public byte[] _sendData { get; set; }

        public int _clientNumber;
        public enum Client_Type
        {
            Tablet1,Tablet2,Tablet3,Tablet4,Tablet5,Tablet6,Tablet7,User,CameraData
        }
        Client_Type _client_type;

        public ClientData(TcpClient client)
        {
            this._client = client;
            this._recvData = new byte[1024];
            this._sendData = new byte[1024];

            this._client_type = Client_Type.Tablet1;

            string clientEndPoint = _client.Client.RemoteEndPoint.ToString();
            char[] point = { '.', ':' };
            string[] clientData = clientEndPoint.Split(point);
            this._clientNumber = int.Parse(clientData[0] + clientData[1] + clientData[2] + clientData[3]) ;
            Console.WriteLine($"{_clientNumber}의 아이피를 지닌 사용자 접속");
        }
    }
}
