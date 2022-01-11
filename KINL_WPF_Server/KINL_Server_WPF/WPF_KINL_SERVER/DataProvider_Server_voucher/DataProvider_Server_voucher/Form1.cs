using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace DataProvider_Server_voucher
{
    public partial class Form1 : Form
    {
        object lockObj = new object();
        private ObservableCollection<string> DeviceData = new ObservableCollection<string>();
        private ObservableCollection<string> AirPotData = new ObservableCollection<string>();
        private ObservableCollection<string> WatchData = new ObservableCollection<string>();


        Task conntectCheckThread = null;

        private bool DeviceSend = false;
        private bool AirPot_DeviceSend = false;
        private bool Watch_DeviceSend = false;

        public Form1()
        {
            InitializeComponent();
            MainServerStart();
            ClientManager.messageParsingAction += MessageParsing;
            ClientManager.ChangeListViewAction += ChangeListView;
            conntectCheckThread = new Task(ConnectCheckLoop);
            conntectCheckThread.Start();
        }
        private void ConnectCheckLoop()
        {
            while (true)
            {
                foreach (var item in ClientManager.clientDic)
                {
                    Console.WriteLine(ClientManager.clientDic.Count);
                    try
                    {
                        string sendStringData = "<TEST>";
                        byte[] sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        item.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                    }
                    catch (Exception e)
                    {
                        RemoveClient(item.Value);
                        Console.WriteLine(item.Value);
                        SaveFile();
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
            ChangeListView(result.clientName, StaticDefine.REMOVE_USER_LIST, null);
            Console.WriteLine(leaveLog);
        }

        private void MessageParsing(string sender, string message)
        {
            lock (lockObj)
            {
                List<string> msgList = new List<string>();
                string[] msgArray = message.Split(';');
                foreach (var item in msgArray)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    msgList.Add(item);
                    SendMsgToClient(item, sender);
                    //  Console.WriteLine(item.ToString());
                }
                //SendMsgToClient(msgList, sender);
                //for (int i = 0; i < msgList.Count; i++)
                //{


                //}
            }
        }
        private void SendMsgToClient(string msgList, string sender)
        {
            string parsedMessage = "";
            string receiver = "";

            //%^& DEVICE = Connect
            //DEVCIE,TIME,DATA
            string[] splitedMsg = msgList.Split(',');
            //if(splitedMsg.Length < 3)
            //{
            //    return;
            //}
            receiver = splitedMsg[0];
            parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);
            if (receiver.Contains("DEVICE"))
            {
                string groupLogMessage = string.Format(@"[{0}],[{1}],[{2}],[{3}].[{4}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), splitedMsg[0], splitedMsg[1], splitedMsg[2], splitedMsg[3]);
               // Console.WriteLine(groupLogMessage);
                ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage);

                return;
            }
            if (receiver.Contains("AIRPOT"))
            {
                string groupLogMessage = string.Format(@"[{0}],[{1}],[{2}],[{3}],[{4}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), splitedMsg[0], splitedMsg[1], splitedMsg[2], splitedMsg[3]);
               // Console.WriteLine(groupLogMessage);
                ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage);
                return;
            }
            if (receiver.Contains("WATCH"))
            {
                string groupLogMessage = string.Format(@"[{0}],[{1}],[{2}],[{3}],[{4}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), splitedMsg[0], splitedMsg[1], splitedMsg[2], splitedMsg[3]);
              //  Console.WriteLine(groupLogMessage);
                ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage);
                return;
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

        private void ChangeListView(string a, int b, string c)
        {
            switch (a)
            {
                case "IOS":
                    if (b == StaticDefine.ADD_USER)
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            listBox3.Items.Add(a + "Connect");
                        }));
                    }
                    else if (b == StaticDefine.REMOVE_USER_LIST)
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            listBox3.Items.Add(a + "Disconnect");
                        }));
                    }
                    break;
                case "DEVICE":
                    if (b == StaticDefine.ADD_USER)
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            listBox3.Items.Add(a + "Connect");
                        }));
                    }
                    else if (b == StaticDefine.REMOVE_USER_LIST)
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            listBox3.Items.Add(a + "Disconnect");
                        }));
                    }
                    else if(b == StaticDefine.DATA_SEND_START)
                    {
                        if (DeviceSend)
                        {
                            DeviceData.Add(c);
                         //   Console.WriteLine("DATAADD");
                        }
                        else
                        {
                            listBox3.BeginInvoke((Action)(() =>
                            {
                                listBox3.Items.Add(a + "Send");
                            }));
                            DeviceSend = true;
                        }
                    }
                    break;
                case "WATCH":
                    if (b == StaticDefine.ADD_USER)
                    {
                        listBox2.BeginInvoke((Action)(() =>
                        {
                            listBox2.Items.Add(a + "Connect");
                        }));
                    }
                    else if(b == StaticDefine.REMOVE_USER_LIST)
                    {
                        listBox2.BeginInvoke((Action)(() =>
                        {
                            listBox2.Items.Add(a + "Disconnect");
                        }));
                    }
                    else if(b == StaticDefine.DATA_SEND_START)
                    {
                        if (Watch_DeviceSend)
                        {
                            WatchData.Add(c);
                           // Console.WriteLine("DATAWATCH");
                        }
                        else
                        {
                            listBox2.BeginInvoke((Action)(() =>
                            {
                                listBox2.Items.Add(a + "Send");
                            }));
                            Watch_DeviceSend = true;
                        }
                    }
                    break;

                case "AIRPOT":
                    if (b == StaticDefine.ADD_USER)
                    {
                        listBox1.BeginInvoke((Action)(() =>
                        {
                            listBox1.Items.Add(a + "Connect");
                        }));
                    }
                    else if(b == StaticDefine.REMOVE_USER_LIST)
                    {
                        listBox1.BeginInvoke((Action)(() =>
                        {
                            listBox1.Items.Add(a + "Disconnect");
                        }));
                    }
                    else if(b == StaticDefine.DATA_SEND_START)
                    {
                        if (AirPot_DeviceSend)
                        {
                            AirPotData.Add(c);
                           // Console.WriteLine("DATAAIRPOT");
                        }
                        else
                        {
                            listBox1.BeginInvoke((Action)(() =>
                            {
                                listBox1.Items.Add(a + "Send");
                            }));
                            AirPot_DeviceSend = true;
                        }
                    }
                    break;
            }
        }
        private void SaveFile()
        {
            //Console.WriteLine($"SAVED : {DeviceData.Count}");
            //Console.WriteLine($"Client :{item.Value.clientName}");
            //GM_DataRecorder.instance.MakeFolder(item.Value.clientName);
            for (int i = 0; i < DeviceData.Count; i++)
            {
                GM_DataRecorder.instance.Enqueue_Data(DeviceData[i].ToString());
            }
            for (int i = 0; i < AirPotData.Count; i++)
            {
                GM_DataRecorder.instance.Enqueue_Data_A(AirPotData[i].ToString());
            }
            for (int i = 0; i < WatchData.Count; i++)
            {
                GM_DataRecorder.instance.Enqueue_Data_W(WatchData[i].ToString());
            }
            if (DeviceData.Count > 0)
            {
                GM_DataRecorder.instance.WriteSteamingData_Batch_Device();
            }
            if (WatchData.Count > 0)
            {
                GM_DataRecorder.instance.WriteSteamingData_Batch_Watch();
            }
            if (AirPotData.Count > 0)
            {
                GM_DataRecorder.instance.WriteSteamingData_Batch_AirPot();
            }
            DeviceData.Clear();
            WatchData.Clear();
            AirPotData.Clear();
        }

            private void MainServerStart()
        {
            MainServer a = new MainServer();
            GM_DataRecorder b = new GM_DataRecorder();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
    }
}
