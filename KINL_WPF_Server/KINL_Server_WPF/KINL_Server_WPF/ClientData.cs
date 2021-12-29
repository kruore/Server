using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KINL_Server_WPF
{
    class ClientData
    {

        public TcpClient _client { get; set; }
        public byte[] _recvData { get; set; }
        public byte[] _sendData { get; set; }
        public StringBuilder currentMsg { get; set; }
        public string _clientUserName { get; set; }
        public int _clientNumber { get; set; }
        public Client_Type _client_type { get; set; }

        public enum Client_Type
        {
            Tablet1, Tablet2, User, CameraData, NULL
        }

        public ClientData(TcpClient client)
        {
            this._client = client;
            this._recvData = new byte[1024];
            this._sendData = new byte[1024];
            this._client_type = Client_Type.NULL;

            try
            {
                string clientEndPoint = client.Client.RemoteEndPoint.ToString();
                char[] point = { '.', ':' };
                string[] clientData = clientEndPoint.Split(point);
                this._clientNumber = int.Parse(clientData[0] + clientData[1] + clientData[2] + clientData[3]);
                Console.WriteLine($"{client}의 아이피를 지닌 사용자 접속");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
            }
        }
    }

    class StaticDefine
    {
        public const int SHOW_CURRENT_CLIENT = 1;
        public const int SHOW_ACCESS_LOG = 2;
        public const int SHOW_CHATTING_LOG = 3;
        public const int ADD_ACCESS_LOG = 5;
        public const int ADD_CHATTING_LOG = 6;
        public const int EXIT = 0;
    }

}
