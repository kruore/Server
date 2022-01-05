using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace WPF_KINL_Server
{
    public partial class Server : Form
    {

        private object lockObj = new object();
        private ObservableCollection<string> chattingLogList = new ObservableCollection<string>();
        private ObservableCollection<string> userList = new ObservableCollection<string>();
        private ObservableCollection<string> AccessLogList = new ObservableCollection<string>();
        Task conntectCheckThread = null;

        public Server()
        {
            InitializeComponent();
            MainServerStart();
            ClientManager.messageParsingAction += MessageParsing;
            ClientManager.ChangeListViewAction += ChangeListView;
            dataGridView1.DataSource = chattingLogList;
            //ChattingLogListView.ItemActivate = chattingLogList;
            //UserListView.ItemsSource = userList;
            //AccessLogListView.ItemsSource = AccessLogList;
            conntectCheckThread = new Task(ConnectCheckLoop);
            conntectCheckThread.Start();
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
                        item.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
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
            ClientManager.clientDic.TryRemove(targetClient.clientNumber, out result);
            string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.clientName);
            //ChangeListView(leaveLog, StaticDefine.ADD_ACCESS_LIST);
            ChangeListView(result.clientName, StaticDefine.REMOVE_USER_LIST);
        }
        private void ChangeListView(string Message, int key)
        {
            switch (key)
            {
                case StaticDefine.ADD_DATA_LIST:
                    {
                        Console.WriteLine("DATACOMMING");

                        break;
                    }
                case StaticDefine.ADD_USER:
                    {
                        Console.WriteLine("USER LIST ++");
                        break;
                    }
                case StaticDefine.REMOVE_USER_LIST:
                    {
                        Console.WriteLine("REMOVEEEEE");
                        Console.WriteLine(Message + "DATA");
                        dataGridView1.BeginInvoke((Action)(() =>
                        {
                            dataGridView1.Rows.Remove(dataGridView1.Rows[0]);
                        }));
                        break;
                    }
                default:
                    break;
            }
        }
        private void MainServerStart()
        {
            MainServer a = new MainServer();
        }

        private void MessageParsing(string sender, string message)
        {
            lock (lockObj)
            {
                List<string> msgList = new List<string>();
                foreach (var item in msgList)
                {
                    if (item != null)
                    {
                        Console.WriteLine(item.ToString());
                    }
                }
                if (!message.Contains("vp,"))
                {
                    Console.WriteLine("ERROR : Doesn't Match Format");
                    return;
                }
                string[] separatingStrings = { "vp," };
                string[] msgArray = message.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
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
            try
            {
                Console.WriteLine("msgs: " + msgList[0]);
                foreach (var item in msgList)
                {
                    string[] splitedMsg = item.Split(',');
                    Console.WriteLine("msg: " + msgList[0]);
                    switch(splitedMsg[0])
                    {
                        // if, Connect
                        case "1" :
                            if(userList.Count==0)
                            {
                                userList.Add(splitedMsg[2]);
                                Console.WriteLine($"User :{splitedMsg[2].ToString()} Connect ");
                            }
                            // Save User List From Name
                            foreach(var user in userList)
                            {
                                if(user != splitedMsg[2])
                                {
                                    userList.Add(splitedMsg[2]);
                                    Console.WriteLine($"User :{splitedMsg[2].ToString()} Connect ");
                                }
                                else
                                {
                                    return;
                                }
                            }
                            break;
                        // if, Disconnect
                        case "3":
                            Console.WriteLine($"User :{splitedMsg[2].ToString()} Disconnect ");
                            userList.Remove(splitedMsg[2]);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
            foreach (var item in msgList)
            {
                string[] splitedMsg = item.Split(',');
                Console.WriteLine(splitedMsg[1].ToString());

                if (splitedMsg.Length < 11)
                {
                    return;
                }

                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);
                if (parsedMessage.Contains("1"))
                {
                    string[] groupSplit = receiver.Split(',');

                    foreach (var el in groupSplit)
                    {
                        if (string.IsNullOrEmpty(el))
                            continue;
                        string groupLogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), groupSplit[0], el, splitedMsg[1]);
                       //ChangeListView(groupLogMessage, StaticDefine.ADD_CHATTING_LIST);

                        receiverNumber = GetClinetNumber(el);

                        parsedMessage = string.Format("{0}<GroupChattingStart>", receiver);
                        byte[] sendGroupByteData = Encoding.Default.GetBytes(parsedMessage);
                        ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendGroupByteData, 0, sendGroupByteData.Length);
                    }
                    return;
                }
                if (receiver.Contains(","))
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
                        ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendGroupByteData, 0, sendGroupByteData.Length);
                    }
                    return;
                }

                senderNumber = GetClinetNumber(sender);
                receiverNumber = GetClinetNumber(receiver);

                if (senderNumber == -1 || receiverNumber == -1)
                {
                    //File.AppendAllText("ClientNumberErrorLog.txt", sender + receiver);
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
                    ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(userListByteData, 0, userListByteData.Length);
                    return;
                }

                string logMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, receiver, splitedMsg[1]);
              //  ChangeListView(logMessage, StaticDefine.ADD_CHATTING_LIST);

                if (parsedMessage.Contains("<ChattingStart>"))
                {
                    parsedMessage = string.Format("{0}<ChattingStart>", receiver);
                    sendByteData = Encoding.Default.GetBytes(parsedMessage);
                    ClientManager.clientDic[senderNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);

                    parsedMessage = string.Format("{0}<ChattingStart>", sender);
                    sendByteData = Encoding.Default.GetBytes(parsedMessage);
                    ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);

                    return;
                }

                if (parsedMessage.Contains(""))

                    ClientManager.clientDic[receiverNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
            }
        }
        private int GetClinetNumber(string targetClientName)
        {
            foreach (var item in ClientManager.clientDic)
            {
                if (item.Value.clientName == targetClientName)
                {
                    return item.Value.clientNumber;
                }
            }
            return -1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           // dataGridView1.DataSource = null;
           // SetupDataGridView();
        }


        private void SetupDataGridView()
        {
         
           // dataGridView1.PerformLayout();
            //dataGridView1.ColumnCount = 7;

            //chattingLogList.Add("??,???,????");


            //dataGridView1.Columns[0].Name = "ID";
            //dataGridView1.Columns[1].Name = "Name";
            //dataGridView1.Columns[2].Name = "Date";
            //dataGridView1.Columns[3].Name = "Data";
            //dataGridView1.Columns[4].Name = "Controll";
            //dataGridView1.Columns[5].Name = "Device";
            //dataGridView1.Columns[6].Name = "DeviceData";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void serverResetButton_Click(object sender, EventArgs e)
        {
            serverStateLabel.Text = "ServerClose";
            //dataGridView1.Rows.Add("??", "???", "??????", "제발!", "맞냐", "?");
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 1)
            {
                dataGridView1.Rows.Remove(dataGridView1.Rows[0]);
            }
        }
        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
        }
    }
}
