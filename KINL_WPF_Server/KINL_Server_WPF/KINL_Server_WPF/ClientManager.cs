using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;

namespace KINL_Server_WPF
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


    class ClientManager
    {
        public static ConcurrentDictionary<int, ClientData> clientDic = new ConcurrentDictionary<int, ClientData>();
        public event Action<string, string> messageParsingAction = null;
        public event Action<string, int> EventHandler = null;

        public void AddClient(TcpClient newClient)
        {
            ClientData currentClient = new ClientData(newClient);

            try
            {
                currentClient._client.GetStream().BeginRead(currentClient._recvData, 0, currentClient._recvData.Length, new AsyncCallback(DataReceived), currentClient);
                clientDic.TryAdd(currentClient._clientNumber, currentClient);
            }

            catch (Exception ex)
            {
            }

        }
        private void DataReceived(IAsyncResult ar)
        {
            ClientData client = ar.AsyncState as ClientData;

            try
            {
                int byteLength = client._client.GetStream().EndRead(ar);

                string strData = Encoding.Default.GetString(client._recvData, 0, byteLength);

                client._client.GetStream().BeginRead(client._recvData, 0, client._recvData.Length, new AsyncCallback(DataReceived), client);

                if (string.IsNullOrEmpty(client._clientUserName))
                {
                    if (EventHandler != null)
                    {
                        if (CheckID(strData))
                        {
                            string userName = strData.Substring(3);
                            client._clientUserName = userName;
                            string accessLog = string.Format("[{0}] {1} Access Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), client._clientUserName);
                            EventHandler.Invoke(accessLog, 1);
                            return;
                        }
                    }
                }
                if (messageParsingAction != null)
                {
                    messageParsingAction.BeginInvoke(client._clientUserName, strData, null, null);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private bool CheckID(string ID)
        {
            if (ID.Contains("%^&"))
                return true;

            return false;
        }
    }
}
