﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace WPF_KINL_Server
{
    class ClientManager
    {
        public static ConcurrentDictionary<int, ClientData> clientDic = new ConcurrentDictionary<int, ClientData>();
        public static event Action<string, string> messageParsingAction = null;
        public static event Action<string, int> ChangeListViewAction = null;
        public static event Action<string, string> messageSendingAction = null;

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
                //RemoveClient(currentClient);
            }
        }

        private void DataReceived(IAsyncResult ar)
        {
            ClientData client = ar.AsyncState as ClientData; 

            try
            {
                int byteLength = client.tcpClient.GetStream().EndRead(ar);
                string strData = Encoding.Default.GetString(client.readBuffer, 0, byteLength);
                client.tcpClient.GetStream().BeginRead(client.readBuffer, 0, client.readBuffer.Length, new AsyncCallback(DataReceived), client);

                if (string.IsNullOrEmpty(client.clientName))
                {
                    if (ChangeListViewAction != null)
                    {
                        if (CheckID(strData))
                        {
                            string userName = strData.Substring(1);
                            client.clientName = userName;
                            ChangeListViewAction.Invoke(client.clientName, StaticDefine.ADD_USER);
                            string accessLog = string.Format("[{0}] {1} Access Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), client.clientName);
                            Console.WriteLine(accessLog);
                            //ChangeListViewAction.Invoke(accessLog, StaticDefine.ADD_USER);
                            File.AppendAllText("AccessRecored.txt", accessLog + "\n");
                            return;
                        }
                    }
                }

                if (messageParsingAction != null)
                {
                    messageParsingAction.BeginInvoke(client.clientName, strData, null, null);
                }

            }
            catch (Exception e)
            {
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
