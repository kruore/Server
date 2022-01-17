using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataProvider_Server_voucher
{
    public partial class Form1 : Form
    {
        object lockObj = new object();
        private Dictionary<int, ObservableCollection<string>> DeviceData = new Dictionary<int, ObservableCollection<string>>();
        private Dictionary<int, ObservableCollection<string>> AirPotData = new Dictionary<int, ObservableCollection<string>>();
        private Dictionary<int, ObservableCollection<string>> WatchData = new Dictionary<int, ObservableCollection<string>>();
        List<string> list = new List<string>();
        Dictionary<string, long> totalDelay = new Dictionary<string, long>();
        Dictionary<string, long> serverDelays = new Dictionary<string, long>();
        Dictionary<string, long> clientDelays = new Dictionary<string, long>();


        List<string> msgList = new List<string>();
        //UNIX PTP
        long preUnixMilliseconds;
        long UnixMilliseconds;
        DateTimeOffset timeOffset;



        Task conntectCheckThread = null;
        Task conntectPTPThread = null;


        private bool DeviceSend = false;
        private bool AirPot_DeviceSend = false;
        private bool Watch_DeviceSend = false;

        private bool PTP_Checker = true;

        public Form1()
        {
            InitializeComponent();
            MainServerStart();
            ClientManager.messageParsingAction += MessageParsing;
            ClientManager.ChangeListViewAction += ChangeListView;
            ClientManager.PTP_Synchronized += CheckPTP;

        }

        private void CheckPTP(string a)
        {
            int i = 0;

            //  true = start
            while (list.Count < 10)
            {
                // 람다 처리 하는게 있다 확인해보자
                foreach (var item in ClientManager.clientDic)
                {
                    if (item.Value.clientNumber == item.Key)
                    {
                        try
                        {
                            timeOffset = DateTimeOffset.Now;
                            preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                            string sendStringData = $"<PTP>,{item.Key.ToString()},{preUnixMilliseconds}";
                            byte[] sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            item.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                        }
                        catch (Exception e)
                        {
                            string sendStringData = $"<PTP>,{item.Key.ToString()},{preUnixMilliseconds}";
                            //Console.WriteLine(sendStringData);
                        }
                        Thread.Sleep(10);
                        i++;
                    }
                }
            }
            PTP_Checker = false;
            Console.WriteLine("END");
            conntectCheckThread = new Task(ConnectCheckLoop);
            conntectCheckThread.Start();
            list.Clear();
            return;
        }
        private void ConnectCheckLoop()
        {
            while (true)
            {
                foreach (var item in ClientManager.clientDic)
                {
                    try
                    {
                        string sendStringData = "<TEST>";
                        byte[] sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        item.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                        Console.WriteLine(sendStringData);
                    }
                    catch (Exception e)
                    {
                        RemoveClient(item.Value);
                        //Console.WriteLine(item.Value);
                        SaveFile(item.Value.clientNumber, item.Value.clientName);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void RemoveClient(ClientData targetClient)
        {
            ClientData result = null;
            totalDelay.Remove(targetClient.clientNumber.ToString());
            clientDelays.Remove(targetClient.clientNumber.ToString());
            serverDelays.Remove(targetClient.clientNumber.ToString());
            //ChangeListView(leaveLog, StaticDefine.ADD_ACCESS_LIST);
            ClientManager.clientDic.TryRemove(targetClient.clientNumber, out result);
            ChangeListView(result.clientName, targetClient.clientNumber, StaticDefine.REMOVE_USER_LIST, null);
            string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.clientName);
            Console.WriteLine(leaveLog);
        }

        private void MessageParsing(string sender, string message)
        {
            lock (lockObj)
            {
                try
                {
                    if (message.Contains("<PTP>"))
                    {
                        string[] ptpMsg = message.Split(',');
                        if (totalDelay.ContainsKey(ptpMsg[1]))
                        {
                            return;
                        }
                        else
                        {
                            if (ptpMsg.Length < 6)
                            {
                                Console.WriteLine(ptpMsg);

                                timeOffset = DateTimeOffset.Now;
                                preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                                if (ptpMsg[3].Contains(";"))
                                {
                                    ptpMsg[3] = ptpMsg[3].TrimEnd(ptpMsg[3][ptpMsg[3].Length - 1]);
                                }
                                string sendStringData = $"{ptpMsg[0]},{ptpMsg[1]},{ptpMsg[2]},{ptpMsg[3]},{preUnixMilliseconds}";
                                byte[] sendByteData = new byte[sendStringData.Length];
                                sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                ClientManager.clientDic[int.Parse(ptpMsg[1])].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                Console.WriteLine(sendStringData);
                            }
                            if (ptpMsg.Length >= 6 && ptpMsg.Length < 7)
                            {
                                Console.WriteLine(ptpMsg[0]);
                                list.Add(message);
                                if (list.Count == 10)
                                {
                                    CalculatePTP();
                                }
                                return;
                            }
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR" + ex);
                }

                string[] msgArray = message.Split(';');
                foreach (var item in msgArray)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    if (!item.Contains(";"))
                    {
                        msgList.Add(item + ";");
                    }
                    else
                    {
                        msgList.Add(item);
                    }
                    SendMsgToClient(item, sender);
                    //Console.WriteLine(item.ToString());
                }
            }
        }
        //PTP

        private void CalculatePTP()
        {
            string UserNum = "";
            long sumServerDelay = 0;
            long sumClientDelay = 0;
            long totalServerDelay = 0;
            for (int i = 0; i < list.Count; i++)
            {
                string[] a = list[i].ToString().Split(',');
                if (a[5].Contains(";"))
                {
                    a[5] = a[5].TrimEnd(a[5][a[5].Length - 1]);
                }
                long server00 = Convert.ToInt64(a[2]);
                long client00 = Convert.ToInt64(a[3]);
                long server01 = Convert.ToInt64(a[4]);
                long client01 = Convert.ToInt64(a[5]);

                long serverDelay = server01 - server00;
                long clientDelay = client01 - client00;
                long serverAndClientDelay = server01 - client01;

                sumServerDelay += serverDelay;
                sumClientDelay += clientDelay;
                totalServerDelay += serverAndClientDelay;
                UserNum = a[1];
            }
            long SD = sumServerDelay / 10;
            long CD = sumClientDelay / 10;
            long SCD = totalServerDelay / 10;
            serverDelays.Add(UserNum, SD);
            clientDelays.Add(UserNum, CD);
            totalDelay.Add(UserNum, SCD);

            //확인용
            foreach (var item in ClientManager.clientDic)
            {
                Console.WriteLine("DELAY:" + totalDelay[item.Value.clientNumber.ToString()]);
                Console.WriteLine("SERVERDELAY:" + serverDelays[item.Value.clientNumber.ToString()]);
                Console.WriteLine("CLIENTDELAY:" + clientDelays[item.Value.clientNumber.ToString()]);
            }
        }
        private void SendMsgToClient(string msgList, string sender)
        {
            try
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
                //parsedMessage = string.Format("SEND CLIENT:{0}", sender);
                //Console.WriteLine(parsedMessage);
                if (receiver.Contains("DEVICE"))
                {
                    string groupLogMessage = "";
                    if (splitedMsg.Length > 8 && splitedMsg.Length < 11)
                    {
                        long tempTime = Convert.ToInt64(splitedMsg[1]);
                        string receiverSender = receiver.Substring(0, receiver.Length - 6);
                        int clientNumber = GetClinetNumber(receiver);
                        Console.WriteLine(clientNumber);
                        string data = "";
                        data = splitedMsg[8].TrimEnd(splitedMsg[8][splitedMsg[8].Length - 1]);
                        var offsetTime = tempTime - totalDelay[clientNumber.ToString()] + ((serverDelays[clientNumber.ToString()] / 2) + (clientDelays[clientNumber.ToString()] / 2));
                        Console.WriteLine(data);
                        groupLogMessage = string.Format($"{splitedMsg[0]},{splitedMsg[1]},{offsetTime.ToString()},{splitedMsg[2]},{splitedMsg[3]}.{splitedMsg[4]},{splitedMsg[5]},{splitedMsg[6]},{splitedMsg[7]},{data};");
                        ChangeListView(receiver, clientNumber, StaticDefine.DATA_SEND_START, groupLogMessage);
                        Console.WriteLine(groupLogMessage);
                    }
                    return;
                }
                else if (receiver.Contains("AIRPOT"))
                {

                }
                else if (receiver.Contains("WATCH"))
                {

                }
                else if (receiver.Contains("<TEST>"))
                {
                    return;
                }
                else
                {
                    Console.WriteLine($"Doesn't correct protocol : {msgList}");
                    return;
                }

                //if (receiver.Contains("DEVICE"))
                //{
                //    string groupLogMessage = string.Format(@"[{0}],[{1}],[{2}],[{3}].[{4}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), splitedMsg[0], splitedMsg[1], splitedMsg[2], splitedMsg[3]);
                //    // Console.WriteLine(groupLogMessage);
                //    ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage);

                //    return;
                //}
                //if (receiver.Contains("AIRPOT"))
                //{
                //    string groupLogMessage = string.Format(@"[{0}],[{1}],[{2}],[{3}],[{4}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), splitedMsg[0], splitedMsg[1], splitedMsg[2], splitedMsg[3]);
                //    // Console.WriteLine(groupLogMessage);
                //    ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage);
                //    return;
                //}
                //if (receiver.Contains("WATCH"))
                //{
                //    string groupLogMessage = string.Format(@"[{0}],[{1}],[{2}],[{3}],[{4}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), splitedMsg[0], splitedMsg[1], splitedMsg[2], splitedMsg[3]);
                //    //  Console.WriteLine(groupLogMessage);
                //    ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage);
                //    return;
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(msgList);
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
        private string GetClinetName(string targetClientName)
        {
            foreach (var item in ClientManager.clientDic)
            {
                if (item.Value.clientName == targetClientName)
                {
                    return item.Value.clientName;
                }
            }
            return "";
        }

        private void ChangeListView(string a, int clientNumber, int protocool, string c)
        {
            try
            {
                if (a.Contains("DEVICE"))
                {
                    if (protocool == StaticDefine.ADD_USER)
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            ObservableCollection<string> lists = new ObservableCollection<string>();
                            listBox3.Items.Add(a + "Connect");
                            if (!DeviceData.ContainsKey(clientNumber))
                            {
                                DeviceData.Add(clientNumber, lists);
                            }
                        }));
                    }
                    else if (protocool == StaticDefine.REMOVE_USER_LIST)
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            listBox3.Items.Add(a + "Disconnect");
                        }));
                    }
                    else if (protocool == StaticDefine.DATA_SEND_START)
                    {
                        if (DeviceSend)
                        {
                            DeviceData[clientNumber].Add(c);
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
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void SaveFile(int clientNumber, string clientName)
        {
            //SAVE
            for (int i = 0; i < DeviceData[clientNumber].Count; i++)
            {
                GM_DataRecorder.instance.Enqueue_Data(clientNumber, DeviceData[clientNumber][i].ToString());
            }
            if (DeviceData[clientNumber].Count > 0)
            {
                GM_DataRecorder.instance.WriteSteamingData_Batch_Device(clientNumber, clientName);
            }
            //if (WatchData[clientNumber].Count > 0)
            //{
            //    for (int i = 0; i < AirPotData[clientNumber].Count; i++)
            //    {
            //        GM_DataRecorder.instance.Enqueue_Data_A(AirPotData[clientNumber].ToString());
            //    }
            //    GM_DataRecorder.instance.WriteSteamingData_Batch_Watch();
            //}
            //if (AirPotData[clientNumber].Count > 0)
            //{
            //    for (int i = 0; i < WatchData.Count; i++)
            //    {
            //        GM_DataRecorder.instance.Enqueue_Data_W(WatchData[clientNumber].ToString());
            //    }
            //    GM_DataRecorder.instance.WriteSteamingData_Batch_AirPot();
            //}
            if (DeviceData.ContainsKey(clientNumber))
            {
                DeviceData[clientNumber].Clear();
                DeviceData.Remove(clientNumber);
            }
            //if (WatchData.ContainsKey(clientNumber))
            //{
            //    WatchData[clientNumber].Clear();
            //    WatchData.Remove(clientNumber);
            //}
            //if (AirPotData.ContainsKey(clientNumber))
            //{
            //    AirPotData[clientNumber].Clear();
            //    AirPotData.Remove(clientNumber);
            //}
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
            Console.WriteLine(sender.ToString());
            SaveFile(GetClinetNumber(sender.ToString()), GetClinetName(sender.ToString()));
        }
    }
}
