using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace KINL_Server
{

    public class KINL_ServerCore
    {
        ClientManager clientManager = null;
        ConcurrentBag<string> AccessLog = null;
        ConcurrentBag<string> DataLog = null;
        Thread connectCheckThread = null;

        public KINL_ServerCore()
        {
            clientManager = new ClientManager();
            connectCheckThread = new Thread(connectCheck);
            connectCheckThread.Start();
            DataLog = new ConcurrentBag<string>();
            AccessLog = new ConcurrentBag<string>();
            clientManager.EventHandler += ClientEvent;
            clientManager.messageParsingAction += MessageParsing;
            Task serverStart = Task.Run(() =>
            {
                AsyncServerStarted();
            });
        }
        public void connectCheck()
        {
            while (true)
            {
                foreach (var item in ClientManager.clientDic)
                {
                    try
                    {
                        string sendStringData = "Admin<CHECK>";
                        byte[] sendBytes = new byte[sendStringData.Length];
                        sendBytes = Encoding.Default.GetBytes(sendStringData);

                        item.Value._client.GetStream().Write(sendBytes, 0, sendBytes.Length);
                    }
                    catch (Exception ex)
                    {
                        RemoveClient(item.Value);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void RemoveClient(ClientData targetClient)
        {

            ClientData result = null;
            if (result != null)
            {
                return;
            }
            ClientManager.clientDic.TryRemove(targetClient._clientNumber, out result);
            Console.WriteLine("Removed");
            //string leaveMsg = string.Format("[{0}]{1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result._clientUserName);
            // AccessLog.Add(leaveMsg);
        }
        private void MessageParsing(string sender, string message)
        {
            List<string> msgList = new List<string>();

            string[] msgArray = message.Split('>');
            foreach (var item in msgArray)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                msgList.Add(item);
            }
            SendMsgToClient(msgList, sender);
        }

        private void SendMsgToClient(List<string> msgList, string sender)
        {
            string LogMessage = "";
            string parsedMessage = "";
            string receiver = "";

            int senderNumber = -1;
            int receiverNumber = -1;

            foreach (var item in msgList)
            {
                string[] splitedMsg = item.Split(',');

                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);

                senderNumber = GetClinetNumber(sender);
                receiverNumber = GetClinetNumber(receiver);

                if (senderNumber == -1 || receiverNumber == -1)
                {
                    return;
                }

                if (parsedMessage.Contains("<GiveMeUserList>"))
                {
                    string userListStringData = "Admin<";
                    foreach (var el in ClientManager.clientDic)
                    {
                        userListStringData += string.Format("${0}", el.Value._clientUserName);
                    }
                    userListStringData += ">";
                    byte[] userListByteData = new byte[userListStringData.Length];
                    userListByteData = Encoding.Default.GetBytes(userListStringData);
                    ClientManager.clientDic[receiverNumber]._client.GetStream().Write(userListByteData, 0, userListByteData.Length);
                    return;
                }



                LogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, receiver, splitedMsg[1]);

                ClientEvent(LogMessage, StaticDefine.ADD_CHATTING_LOG);

                byte[] sendByteData = Encoding.Default.GetBytes(parsedMessage);

                ClientManager.clientDic[receiverNumber]._client.GetStream().Write(sendByteData, 0, sendByteData.Length);
            }
        }


        private int GetClinetNumber(string targetClientName)
        {
            foreach (var item in ClientManager.clientDic)
            {
                if (item.Value._clientUserName == targetClientName)
                {
                    return item.Value._clientNumber;
                }
            }
            return -1;
        }

        private void ClientEvent(string message, int key)
        {
            switch (key)
            {
                case StaticDefine.ADD_ACCESS_LOG:
                    {
                        AccessLog.Add(message);
                        break;
                    }
                case StaticDefine.ADD_CHATTING_LOG:
                    {
                        DataLog.Add(message);
                        break;
                    }
            }
        }
        public void AsyncServerStarted()
        {
            int port = 4545;
            IPAddress address = IPAddress.Any;
            IPEndPoint iPEndPoint = new IPEndPoint(address, port);

            TcpListener listener = new TcpListener(iPEndPoint);
            listener.Start();


            Console.WriteLine("Server Started");

            while (true)
            {
                Console.WriteLine("Waiting");
                TcpClient acceptClient = listener.AcceptTcpClient();
                ClientData clientData = new ClientData(acceptClient);
                clientData._client.GetStream().BeginRead(clientData._recvData, 0, clientData._recvData.Length, new AsyncCallback(DataRecived), clientData);
                Console.WriteLine("Accept");
            }
        }
        private void DataRecived(IAsyncResult ar)
        {
            try
            {
                ClientData callbackClient = ar.AsyncState as ClientData;
                int byteRead = callbackClient._client.GetStream().EndRead(ar);
                string readString = Encoding.Default.GetString(callbackClient._recvData, 0, byteRead);

                Console.WriteLine($"{callbackClient._clientNumber}의 사용자 : {readString}");
                callbackClient._client.GetStream().BeginRead(callbackClient._recvData, 0, callbackClient._recvData.Length, new AsyncCallback(DataRecived), callbackClient);
            }
            catch (Exception ex)
            {
                RemoveClient(ar.AsyncState as ClientData);

            }
        }
    }
}
