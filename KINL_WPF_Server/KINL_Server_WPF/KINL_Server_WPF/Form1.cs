using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using System.Data;

namespace KINL_Server_WPF
{
    public partial class Form1 : Form
    {
        private object lockObj = new object();
        private ObservableCollection<string> chattingLogList = new ObservableCollection<string>();
        private ObservableCollection<string> userList = new ObservableCollection<string>();
        private ObservableCollection<string> AccessLogList = new ObservableCollection<string>();
        Task conntectCheckThread = null;

        public Form1()
        {
            InitializeComponent();


        }


        private void ConnectCheckLoop()
        {
            while (true)
            {
                foreach (var item in ClientManager.clientDic)
                {
                    try
                    {
                        string sendStringData = "관리자<TEST>";
                        byte[] sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.Default.GetBytes(sendStringData);

                        item.Value._client.GetStream().Write(sendByteData, 0, sendByteData.Length);
                    }
                    catch (Exception e)
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
            ClientManager.clientDic.TryRemove(targetClient._clientNumber, out result);
            string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result._clientUserName);
            //ChangeListView(leaveLog, StaticDefine.ADD_ACCESS_LIST);
            //ChangeListView(result._clientUserName, StaticDefine.REMOVE_USER_LIST);
        }


        private void ChangeListView(string Message, int key)
        {
            switch (key)
            {
                //case StaticDefine.ADD_ACCESS_LIST:
                //    {
                //        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                //        {
                //            AccessLogList.Add(Message);
                //        }));
                //        break;
                //    }
                //case StaticDefine.ADD_CHATTING_LIST:
                //    {
                //        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                //        {
                //            chattingLogList.Add(Message);
                //        }));
                //        break;
                //    }
                //case StaticDefine.ADD_USER_LIST:
                //    {
                //        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                //        {
                //            userList.Add(Message);
                //        }));
                //        break;
                //    }
                //case StaticDefine.REMOVE_USER_LIST:
                //    {
                //        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                //        {
                //            userList.Remove(Message);
                //        }));
                //        break;
                //    }
                default:
                    break;
            }
        }

        private void MessageParsing(string sender, string message)
        {
            lock (lockObj)
            {
                List<string> msgList = new List<string>();

                string[] msgArray = message.Split('>');
                foreach (var item in msgArray)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    msgList.Add(item);
                }
                SendMsgToClient(msgList, sender);
            }
        }

        private void SendMsgToClient(List<string> msgList, string sender)
        {
            string parsedMessage = "";
            string receiver = "";

            int senderNumber = -1;
            int receiverNumber = -1;

            foreach (var item in msgList)
            {
                string[] splitedMsg = item.Split('<');

                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);

                if (parsedMessage.Contains("<GroupChattingStart>"))
                {
                    string[] groupSplit = receiver.Split('#');

                    foreach (var el in groupSplit)
                    {
                        if (string.IsNullOrEmpty(el))
                            continue;
                        string groupLogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), groupSplit[0], el, splitedMsg[1]);
                  //      ChangeListView(groupLogMessage, StaticDefine.ADD_CHATTING_LIST);

                        receiverNumber = GetClinetNumber(el);

                        parsedMessage = string.Format("{0}<GroupChattingStart>", receiver);
                        byte[] sendGroupByteData = Encoding.Default.GetBytes(parsedMessage);
                        ClientManager.clientDic[receiverNumber]._client.GetStream().Write(sendGroupByteData, 0, sendGroupByteData.Length);
                    }
                    return;
                }

                if (receiver.Contains('#'))
                {
                    string[] groupSplit = receiver.Split('#');

                    foreach (var el in groupSplit)
                    {
                        if (string.IsNullOrEmpty(el))
                            continue;
                        if (el == groupSplit[0])
                            continue;
                        string groupLogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), groupSplit[0], el, splitedMsg[1]);
                      //  ChangeListView(groupLogMessage, StaticDefine.ADD_CHATTING_LIST);

                        receiverNumber = GetClinetNumber(el);

                        parsedMessage = string.Format("{0}<{1}>", receiver, splitedMsg[1]);
                        byte[] sendGroupByteData = Encoding.Default.GetBytes(parsedMessage);
                        ClientManager.clientDic[receiverNumber]._client.GetStream().Write(sendGroupByteData, 0, sendGroupByteData.Length);
                    }
                    return;
                }


                senderNumber = GetClinetNumber(sender);
                receiverNumber = GetClinetNumber(receiver);


                if (senderNumber == -1 || receiverNumber == -1)
                {
                    return;
                }

                byte[] sendByteData = new byte[parsedMessage.Length];
                sendByteData = Encoding.Default.GetBytes(parsedMessage);

                if (parsedMessage.Contains("<GiveMeUserList>"))
                {
                    string userListStringData = "관리자<";
                    foreach (var el in userList)
                    {
                        userListStringData += string.Format("${0}", el);
                    }
                    userListStringData += ">";
                    byte[] userListByteData = new byte[userListStringData.Length];
                    userListByteData = Encoding.Default.GetBytes(userListStringData);
                    ClientManager.clientDic[receiverNumber]._client.GetStream().Write(userListByteData, 0, userListByteData.Length);
                    return;
                }




                string logMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, receiver, splitedMsg[1]);
              //  ChangeListView(logMessage, StaticDefine.ADD_CHATTING_LIST);

                if (parsedMessage.Contains("<ChattingStart>"))
                {
                    parsedMessage = string.Format("{0}<ChattingStart>", receiver);
                    sendByteData = Encoding.Default.GetBytes(parsedMessage);
                    ClientManager.clientDic[senderNumber]._client.GetStream().Write(sendByteData, 0, sendByteData.Length);

                    parsedMessage = string.Format("{0}<ChattingStart>", sender);
                    sendByteData = Encoding.Default.GetBytes(parsedMessage);
                    ClientManager.clientDic[receiverNumber]._client.GetStream().Write(sendByteData, 0, sendByteData.Length);

                    return;
                }



                if (parsedMessage.Contains(""))

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

        private void MainServerStart()
        {
            Form1 a = new Form1();
        }



        #region Button Event
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
   
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }


        private void CreateDataTable(DataTable dt)
        {

      
            dt.Columns.Add("UserID");
            dt.Columns.Add("UserName");
            dt.Columns.Add("UserDate");
            dt.Columns.Add("UserData");
            dt.Columns.Add("ControllDevice");
            dt.Columns.Add("DeviceData");

            dataGridView1.DataSource = dt;
        }
        #endregion
    }
}
