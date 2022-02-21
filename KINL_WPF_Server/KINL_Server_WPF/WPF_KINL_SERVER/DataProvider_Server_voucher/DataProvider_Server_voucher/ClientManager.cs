using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataProvider_Server_voucher
{
    class ClientManager
    {
        public static ConcurrentDictionary<int, ClientData> clientDic = new ConcurrentDictionary<int, ClientData>();
        public static event Action<string, string> messageParsingAction = null;
        public static event Action<string, int, string,string> ChangeListViewAction = null;
        public static event Action<string> PTP_Synchronized = null;
        public GM_DB gm_DB = new GM_DB();
        public void AddClient(TcpClient newClient)
        {
            ClientData currentClient = new ClientData(newClient);

            try
            {
                newClient.GetStream().BeginRead(currentClient.readBuffer, 0, currentClient.readBuffer.Length, new AsyncCallback(DataReceived), currentClient);
                clientDic.TryAdd(currentClient.clientNumber, currentClient);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e);
                //RemoveClient(currentClient);
            }
        }

        private void DataReceived(IAsyncResult ar)
        {
            ClientData client = ar.AsyncState as ClientData;

            try
            {
                //Console.WriteLine(client.tcpClient.Client.LocalEndPoint.ToString());
                int byteLength = client.tcpClient.GetStream().EndRead(ar);
                string strData = Encoding.Default.GetString(client.readBuffer, 0, byteLength);
                client.tcpClient.GetStream().BeginRead(client.readBuffer, 0, client.readBuffer.Length, new AsyncCallback(DataReceived), client);

                if (string.IsNullOrEmpty(client.clientName))
                {
                    if (ChangeListViewAction != null)
                    {
                        if (CheckID(strData))
                        {
                            string[] str = strData.Split(';');
                            string userName = str[0].Substring(3);
                            Console.WriteLine(userName);
                            client.clientName = userName;
                            ChangeListViewAction.Invoke(client.clientNumber.ToString(), StaticDefine.ADD_USER, null,client.clientNumber.ToString());
                            messageParsingAction.BeginInvoke(client.clientNumber.ToString(), strData, null, null);
                            string accessLog = string.Format("[{0}] {1} Access Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), client.clientName);
                            Console.WriteLine(accessLog);
                           // ChangeListViewAction.Invoke(accessLog, StaticDefine.ADD_USER, null,null);
                            File.AppendAllText("AccessRecored.txt", accessLog + "\n");
                            PTP_Synchronized.Invoke(client.clientNumber.ToString());
                            if (client.clientName.Contains("DEVICE"))
                            {
                                Form1.clientNames = client.clientName;
                                Console.WriteLine("DDD"+Form1.clientNames);
                            }
                            else if (client.clientName.Contains("IOS"))
                            {
                                gm_DB.CheckID(client.clientName.ToString());
                            }
                            return;
                        }
                    }
                }

                if (messageParsingAction != null)
                {
                    messageParsingAction.BeginInvoke(client.clientNumber.ToString(), strData, null, null);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("asccccccccc" + e);
                //RemoveClient(client);
            }
        }

        private bool CheckID(string ID)
        {
            if (ID.Contains("%^&"))
                return true;

            File.AppendAllText("IDErrLog.txt", ID);
            return false;
        }
    }

}
