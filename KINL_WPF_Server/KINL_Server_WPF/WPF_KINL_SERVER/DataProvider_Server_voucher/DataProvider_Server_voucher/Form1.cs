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
        private ObservableCollection<string> DeviceData = new ObservableCollection<string>();
        private ObservableCollection<string> AirPotData = new ObservableCollection<string>();
        private ObservableCollection<string> WatchData = new ObservableCollection<string>();
        List<string> list = new List<string>();
        Dictionary<string, long> totalDelay = new Dictionary<string, long>();
        Dictionary<string, long> serverDelays = new Dictionary<string, long>();
        Dictionary<string, long> clientDelays = new Dictionary<string, long>();
        Dictionary<string, string> msgDic = new Dictionary<string, string>();

        List<string> msgList = new List<string>();
        //UNIX PTP
        long preUnixMilliseconds;
        long UnixMilliseconds;
        DateTimeOffset timeOffset;

        public static string clientNames;

        Task conntectCheckThread = null;
        Task conntectPTPThread = null;
        Task fileCheck = null;



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
                            string sendStringData = $"ERROR<PTP>,{item.Key.ToString()},{preUnixMilliseconds}";
                            // Console.WriteLine(sendStringData);
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
                        //     Console.WriteLine(sendStringData);
                    }
                    catch (Exception e)
                    {
                        SaveFile(item.Value.clientNumber);
                        RemoveClient(item.Value);
                        //Console.WriteLine(item.Value);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void RemoveClient(ClientData targetClient)
        {
            ClientData result = null;
            Console.WriteLine(clientNames);
            totalDelay.Remove(targetClient.clientNumber.ToString());
            clientDelays.Remove(targetClient.clientNumber.ToString());
            serverDelays.Remove(targetClient.clientNumber.ToString());
            ClientManager.clientDic.TryRemove(targetClient.clientNumber, out result);
            ChangeListView(result.clientName, StaticDefine.REMOVE_USER_LIST, null);
            //  string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.clientName);
            //ChangeListView(leaveLog, StaticDefine.ADD_ACCESS_LIST);
            //  Console.WriteLine(leaveLog);
        }

        private void MessageParsing(string sender, string message)
        {
            lock (lockObj)
            {
                //
                //Console.WriteLine(message);
                if (!msgDic.ContainsKey(GetClinetNumber(sender).ToString()))
                {
                    msgDic.Add(GetClinetNumber(sender).ToString(), message);
                }
                else
                {
                    msgDic[GetClinetNumber(sender).ToString()] += message;
                }
                string[] msgArray = new string[0];
                if (msgDic[GetClinetNumber(sender).ToString()].LastIndexOf(';') == msgDic[GetClinetNumber(sender).ToString()].Length)
                {
                    msgArray = msgDic[GetClinetNumber(sender).ToString()].Split(';');
                    msgDic[GetClinetNumber(sender).ToString()] = "";
                }
                else
                {
                    string[] temparray = msgDic[GetClinetNumber(sender).ToString()].Split(';');
                    msgArray = new string[temparray.Length - 1];
                    for (int i = 0; i < temparray.Length - 1; i++)
                    {
                        msgArray[i] = temparray[i];
                    }
                    msgDic[GetClinetNumber(sender).ToString()] = temparray[temparray.Length - 1];
                }
                //Console.WriteLine("msgDic" + msgDic[GetClinetNumber(sender).ToString()]);
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
                    // Console.WriteLine(item.ToString());
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
            long minServerDelay = 0;
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
                if (i == 0)
                {
                    minServerDelay = serverDelay;
                }

                if (minServerDelay > serverDelay)
                {
                    minServerDelay = serverDelay;
                }

                sumServerDelay += serverDelay;
                sumClientDelay += clientDelay;
                totalServerDelay += serverAndClientDelay;
                UserNum = a[1];
            }
            long SD = minServerDelay;
            long CD = sumClientDelay / 10;
            long SCD = totalServerDelay / 10;

            serverDelays.Add(UserNum, SD);
            clientDelays.Add(UserNum, CD);
            totalDelay.Add(UserNum, SCD);

            //확인용
            foreach (var item in ClientManager.clientDic)
            {
                Console.WriteLine("USERNUMBER" + UserNum);
                Console.WriteLine("DELAY:" + totalDelay[item.Value.clientNumber.ToString()]);
                Console.WriteLine("SERVERDELAY:" + serverDelays[item.Value.clientNumber.ToString()]);
                Console.WriteLine("CLIENTDELAY:" + clientDelays[item.Value.clientNumber.ToString()]);
            }
        }
        private void SendMsgToClient(string msgList, string sender)
        {
            //try
            //{
            string parsedMessage = "";
            string receiver = "";

            msgList= msgList.Replace("^", ",");

            //%^& DEVICE = Connect
            //DEVCIE,TIME,DATA
            string[] splitedMsg = msgList.Split(',');
            //if(splitedMsg.Length < 3)
            //{
            //    return;
            //}
            receiver = splitedMsg[0];
            parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);
            try
            {
                if (msgList.Contains("<PTP>"))
                {
                    string[] ptpMsg = msgList.Split(',');
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
                            list.Add(msgList);
                            if (list.Count == 10)
                            {
                                CalculatePTP();
                            }
                            return;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR" + ex);
            }

            switch (receiver)
            {
                case "DEVICE":
                    long tempTime = Convert.ToInt64(splitedMsg[1]);
                    long PTPTime = totalDelay[GetClinetNumber(sender).ToString()];
                    tempTime = (tempTime + PTPTime) + (serverDelays[GetClinetNumber(sender).ToString()] / 2); ;
                    StringBuilder aaaa = new StringBuilder();
                    aaaa = aaaa.Append(splitedMsg[0] + ",");
                    aaaa.Append(tempTime);
                    for (int i = 1; i < splitedMsg.Length; i++)
                    {
                        aaaa.Append("," + splitedMsg[i]);
                    }
                    string groupLogMessage = aaaa.ToString();
                    ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage);
                    return;
                case "AIRPOT":
                    long tempTime1 = Convert.ToInt64(splitedMsg[1]);
                    long PTPTime1 = totalDelay[GetClinetNumber(sender).ToString()];
                    tempTime1 = (tempTime1 + PTPTime1) - (serverDelays[GetClinetNumber(sender).ToString()] / 2);
                    StringBuilder aaaa1 = new StringBuilder();
                    aaaa1 = aaaa1.Append(splitedMsg[0] + ",");
                    aaaa1.Append(tempTime1);
                    for (int i = 1; i < splitedMsg.Length; i++)
                    {
                        aaaa1.Append("," + splitedMsg[i]);
                    }
                    string groupLogMessage2 = aaaa1.ToString();
                    ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage2);
                    return;
                case "WATCH":
                    long tempTime2 = Convert.ToInt64(splitedMsg[1]);

                    long PTPTime2 = totalDelay[GetClinetNumber(sender).ToString()];
                    long aaa = 0;

                    if (!totalDelay.TryGetValue(GetClinetNumber(sender).ToString(), out aaa))
                    {
                        foreach (var name in totalDelay)
                        {
                            Console.WriteLine($"{GetClinetNumber(name.Key)},{name.Value},{sender}");
                        }
                    }
                    tempTime2 = (tempTime2 + PTPTime2) - (serverDelays[GetClinetNumber(sender).ToString()] / 2);
                    StringBuilder aaaa2 = new StringBuilder();
                    aaaa2 = aaaa2.Append(splitedMsg[0] + ",");
                    aaaa2.Append(tempTime2);
                    for (int i = 1; i < splitedMsg.Length; i++)
                    {
                        aaaa2.Append("," + splitedMsg[i]);
                    }
                    Console.WriteLine(aaaa2);
                    string groupLogMessage3 = aaaa2.ToString();
                    //  Console.WriteLine(groupLogMessage);
                    ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage3);
                    return;
                case "<TEST>":
                    return;
                default:
                    Console.WriteLine("ERROR DEFAULT" + msgList);
                    break;
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
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(msgList);

            //}
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
        private string GetClinetName()
        {
            foreach (var item in ClientManager.clientDic)
            {
                if (item.Value.clientName.ToString().Contains("DEVICE"))
                {
                    return item.Value.clientName;
                }
            }
            return "";
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
                    else if (b == StaticDefine.DATA_SEND_START)
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
                    else if (b == StaticDefine.REMOVE_USER_LIST)
                    {
                        listBox2.BeginInvoke((Action)(() =>
                        {
                            listBox2.Items.Add(a + "Disconnect");
                        }));
                    }
                    else if (b == StaticDefine.DATA_SEND_START)
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
                    else if (b == StaticDefine.REMOVE_USER_LIST)
                    {
                        listBox1.BeginInvoke((Action)(() =>
                        {
                            listBox1.Items.Add(a + "Disconnect");
                        }));
                    }
                    else if (b == StaticDefine.DATA_SEND_START)
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
        private void SaveFile(int abc)
        {
            string userToken = clientNames;
            Console.WriteLine("USER: " + userToken);
            Console.WriteLine("TOKENNUMBER"+ GetClinetNumber(userToken));
            //Console.WriteLine($"SAVED : {DeviceData.Count}");
            //Console.WriteLine($"Client :{item.Value.clientName}");
            //GM_DataRecorder.instance.MakeFolder(item.Value.clientName);
            if (GetClinetNumber(userToken) == abc)
            {

                for (int i = 0; i < DeviceData.Count; i++)
                {
                    GM_DataRecorder.instance.Enqueue_Data(DeviceData[i].ToString());
                }
                if (DeviceData.Count > 0)
                {
                    GM_DataRecorder.instance.WriteSteamingData_Batch_Device(userToken);
                }
                DeviceData.Clear();
            }
            else
            {
                for (int i = 0; i < AirPotData.Count; i++)
                {
                    GM_DataRecorder.instance.Enqueue_Data_A(AirPotData[i].ToString());
                }
                for (int i = 0; i < WatchData.Count; i++)
                {
                    GM_DataRecorder.instance.Enqueue_Data_W(WatchData[i].ToString());
                }

                if (WatchData.Count > 0)
                {
                    GM_DataRecorder.instance.WriteSteamingData_Batch_Watch(userToken);
                }
                if (AirPotData.Count > 0)
                {
                    GM_DataRecorder.instance.WriteSteamingData_Batch_AirPot(userToken);
                }

                WatchData.Clear();
                AirPotData.Clear();
            }
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
            //SaveFile();
        }
    }
}
