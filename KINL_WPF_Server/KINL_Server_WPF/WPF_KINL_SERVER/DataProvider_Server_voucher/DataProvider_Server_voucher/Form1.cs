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
        object _ptpLock = new object();

        //실제 저장장소
        private Dictionary<string, ObservableCollection<string>> DeviceData = new Dictionary<string, ObservableCollection<string>>();
        private Dictionary<string, ObservableCollection<string>> AirPotData = new Dictionary<string, ObservableCollection<string>>();
        private Dictionary<string, ObservableCollection<string>> WatchData = new Dictionary<string, ObservableCollection<string>>();

        //PTP 할때 사용
        Dictionary<string, List<string>> PTPlist = new Dictionary<string, List<string>>();

        //PTP 완료된 딜레이 체크
        Dictionary<string, long> totalDelay = new Dictionary<string, long>();
        Dictionary<string, long> serverDelays = new Dictionary<string, long>();
        Dictionary<string, long> clientDelays = new Dictionary<string, long>();

        //메세지 간이 저장 공간
        Dictionary<string, string> msgDic = new Dictionary<string, string>();

        //메세지 정리
        List<string> msgList = new List<string>();

        //UNIX PTP
        long preUnixMilliseconds;
        long UnixMilliseconds;
        DateTimeOffset timeOffset;

        //디바이스 네임
        public static string clientNames;

        Task conntectCheckThread = null;
        Task conntectPTPThread = null;
        Task fileCheck = null;

        private Dictionary<string, bool> DeviceSend = new Dictionary<string, bool>();
        private Dictionary<string, bool> Watch_DeviceSend = new Dictionary<string, bool>();
        private Dictionary<string, bool> AirPot_DeviceSend = new Dictionary<string, bool>();

        private bool PTP_Checker = true;

        public Form1()
        {
            InitializeComponent();
            MainServerStart();
            ClientManager.messageParsingAction += MessageParsing;
            ClientManager.ChangeListViewAction += ChangeListView;
            ClientManager.PTP_Synchronized += CheckPTP;

        }
        //GET CLIENT NUMBER AND CHECKED PTP
        private void CheckPTP(string a)
        {
            lock (_ptpLock)
            {
                if (!PTPlist.ContainsKey(GetClinetNumber(a).ToString()))
                {
                    List<string> tempList = new List<string>();
                    PTPlist.Add(GetClinetNumber(a).ToString(), tempList);
                    while (PTPlist[GetClinetNumber(a).ToString()].Count < 10)
                    {
                        try
                        {
                            timeOffset = DateTimeOffset.Now;
                            preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                            string sendStringData = $"<PTP>,{GetClinetNumber(a).ToString()},{preUnixMilliseconds}";
                            byte[] sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            foreach (var att in ClientManager.clientDic)
                            {
                                if (att.Value.clientNumber == GetClinetNumber(a))
                                {
                                    att.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR");
                            break;
                            //return;
                        }
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    while (PTPlist[GetClinetNumber(a).ToString()].Count < 10)
                    {
                        try
                        {
                            timeOffset = DateTimeOffset.Now;
                            preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                            string sendStringData = $"<PTP>,{GetClinetNumber(a).ToString()},{preUnixMilliseconds}";
                            byte[] sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            foreach (var att in ClientManager.clientDic)
                            {
                                if (att.Value.clientNumber == GetClinetNumber(a))
                                {
                                    att.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR");
                            break;
                            //return;
                        }
                        Thread.Sleep(10);
                    }
                }
                Console.WriteLine("END");
                conntectCheckThread = new Task(ConnectCheckLoop);
                conntectCheckThread.Start();
                return;
            }

        }
        private void CalculatePTP(string sender)
        {
            string UserNum = "";
            long sumServerDelay = 0;
            long sumClientDelay = 0;
            long totalServerDelay = 0;
            long minServerDelay = 0;

            for (int i = 0; i < PTPlist[GetClinetNumber(sender).ToString()].Count; i++)
            {
                string[] a = PTPlist[GetClinetNumber(sender).ToString()][i].Split(',');

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
                    }
                    catch (Exception e)
                    {
                        SaveFile(item.Value.clientNumber);
                        RemoveClient(item.Value);
                    }
                }
                Thread.Sleep(2000);
            }
        }

        private void RemoveClient(ClientData targetClient)
        {
            ClientData result = null;
            Console.WriteLine(clientNames);
            PTPlist.Remove(targetClient.clientNumber.ToString());
            totalDelay.Remove(targetClient.clientNumber.ToString());
            clientDelays.Remove(targetClient.clientNumber.ToString());
            serverDelays.Remove(targetClient.clientNumber.ToString());
            ClientManager.clientDic.TryRemove(targetClient.clientNumber, out result);
            ChangeListView(result.clientName, StaticDefine.REMOVE_USER_LIST, null,null);
            //  string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.clientName);
            //ChangeListView(leaveLog, StaticDefine.ADD_ACCESS_LIST);
            //  Console.WriteLine(leaveLog);
        }

        private void MessageParsing(string sender, string message)
        {
            lock (lockObj)
            {
                if (!msgDic.ContainsKey(GetClinetNumber(sender).ToString()))
                {
                    msgDic.Add(GetClinetNumber(sender).ToString(), message);
                    Console.WriteLine($"KEY 추가됨 : {GetClinetNumber(sender).ToString()}");
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



        // MESSAGE PARSSED
        private void SendMsgToClient(string msgList, string sender)
        {
            //try
            //{
            string parsedMessage = "";
            string receiver = "";

            msgList = msgList.Replace("^", ",");
            if (msgList.Contains("<PTP>"))
            {
                Console.WriteLine(msgList);
            }
            //%^& DEVICE = Connect
            //DEVCIE,TIME,DATA
            string[] splitedMsg = msgList.Split(',');
            //if(splitedMsg.Length < 3)
            //{
            //    return;
            //}
            receiver = splitedMsg[0];
            parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);
            switch (receiver)
            {
                case "<PTP>":
                    if (totalDelay.ContainsKey(GetClinetNumber(sender).ToString()))
                    {
                        return;
                    }
                    else
                    {
                        if (splitedMsg.Length < 6)
                        {
                            Console.WriteLine(splitedMsg);

                            timeOffset = DateTimeOffset.Now;
                            preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                            if (splitedMsg[3].Contains(";"))
                            {
                                splitedMsg[3] = splitedMsg[3].TrimEnd(splitedMsg[3][splitedMsg[3].Length - 1]);
                            }
                            string sendStringData = $"{splitedMsg[0]},{splitedMsg[1]},{splitedMsg[2]},{splitedMsg[3]},{preUnixMilliseconds}";
                            byte[] sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            ClientManager.clientDic[GetClinetNumber(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                            Console.WriteLine(sendStringData);
                        }
                        if (splitedMsg.Length >= 6 && splitedMsg.Length < 7)
                        {
                            if (splitedMsg[2].Contains("<PTP>") || splitedMsg[3].Contains("<PTP>"))
                            {
                                return;
                            }
                            PTPlist[GetClinetNumber(sender).ToString()].Add(msgList);
                            if (PTPlist[GetClinetNumber(sender).ToString()].Count == 10)
                            {
                                CalculatePTP(sender);
                            }
                            return;
                        }
                    }
                    break;
                case "DEVICE":
                    if (!totalDelay.ContainsKey(GetClinetNumber(sender).ToString()))
                    {
                        return;
                    }
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
                    ChangeListView(sender, StaticDefine.DATA_SEND_START, groupLogMessage,GetClinetNumber(sender).ToString());
                    return;
                case "AIRPOT":
                    if (!totalDelay.ContainsKey(GetClinetNumber(sender).ToString()))
                    {
                        return;
                    }
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
                    ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage2, GetClinetNumber(sender).ToString());
                    return;
                case "WATCH":
                    if (!totalDelay.ContainsKey(GetClinetNumber(sender).ToString()))
                    {
                        return;
                    }
                    try
                    {
                        if (splitedMsg.Length == 11)
                        {
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
                            ChangeListView(receiver, StaticDefine.DATA_SEND_START, groupLogMessage3, GetClinetNumber(sender).ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    return;
                case "<TEST>":
                    if (!totalDelay.ContainsKey(GetClinetNumber(sender).ToString()))
                    {
                        return;
                    }
                    return;
                default:
                    if (!totalDelay.ContainsKey(GetClinetNumber(sender).ToString()))
                    {
                        return;
                    }
                    Console.WriteLine("ERROR DEFAULT" + msgList);
                    break;
            }
        }

        // UTILL

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


        // VIEW CHANGED
        private void ChangeListView(string a, int b, string c,string senderPort)
        {
            string clientNumbers = GetClinetNumber(a).ToString();
            string iosNumbers = "";
            switch (a)
            {
                case "IOS":
                    if (b == StaticDefine.ADD_USER)
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            Console.WriteLine(clientNumbers);
                            listBox3.Items.Add(a + "Connect");
                            Watch_DeviceSend.Add(clientNumbers, false);
                            AirPot_DeviceSend.Add(clientNumbers, false);
                            ObservableCollection<string> watchList = new ObservableCollection<string>();
                            ObservableCollection<string> airpotList = new ObservableCollection<string>();
                            WatchData.Add(clientNumbers, watchList);
                            AirPotData.Add(clientNumbers, airpotList);
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
                            DeviceSend.Add(clientNumbers, false);
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
                        if (DeviceSend[GetClinetNumber(a).ToString()] == true)
                        {
                            DeviceData[GetClinetNumber(a).ToString()].Add(c);
                            //   Console.WriteLine("DATAADD");
                        }
                        else
                        {
                            listBox3.BeginInvoke((Action)(() =>
                            {
                                listBox3.Items.Add(a + "Send");
                            }));
                            DeviceSend[clientNumbers] = true;
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
                        if (Watch_DeviceSend[senderPort])
                        {
                            Console.WriteLine(senderPort);
                            WatchData[senderPort].Add(c);
                            // Console.WriteLine("DATAWATCH");
                        }
                        else
                        {
                            listBox2.BeginInvoke((Action)(() =>
                            {
                                listBox2.Items.Add(a + "Send");
                            }));
                            Watch_DeviceSend[senderPort] = true;
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
                        if (AirPot_DeviceSend[senderPort])
                        {
                            AirPotData[senderPort].Add(c);
                            // Console.WriteLine("DATAAIRPOT");
                        }
                        else
                        {
                            listBox1.BeginInvoke((Action)(() =>
                            {
                                listBox1.Items.Add(a + "Send");
                            }));
                            AirPot_DeviceSend[senderPort] = true;
                        }
                    }
                    break;
            }
        }


        //FILE SAVED

        private void SaveFile(int abc)
        {
            string userToken = clientNames;
            Console.WriteLine("USER: " + userToken);
            Console.WriteLine("TOKENNUMBER" + GetClinetNumber(userToken));
            //Console.WriteLine($"SAVED : {DeviceData.Count}");
            //Console.WriteLine($"Client :{item.Value.clientName}");
            //GM_DataRecorder.instance.MakeFolder(item.Value.clientName);
            if (GetClinetNumber(userToken) == abc)
            {
                for (int i = 0; i < DeviceData[GetClinetNumber(userToken).ToString()].Count; i++)
                {
                    GM_DataRecorder.instance.Enqueue_Data(userToken, DeviceData[userToken].ToString());
                }
                if (DeviceData.Count > 0)
                {
                    GM_DataRecorder.instance.WriteSteamingData_Batch_Device(userToken, userToken);
                }
                DeviceData.Clear();
            }
            else
            {
                if (AirPotData[abc.ToString()].Count > 1)
                {

                    for (int i = 0; i < AirPotData[abc.ToString()].Count; i++)
                    {
                        GM_DataRecorder.instance.Enqueue_Data_A(abc.ToString(), AirPotData[abc.ToString()][i].ToString());
                    }
                }
                if (WatchData[abc.ToString()].Count > 1)
                {
                    for (int i = 0; i < WatchData[abc.ToString()].Count; i++)
                    {
                        GM_DataRecorder.instance.Enqueue_Data_W(abc.ToString(), WatchData[abc.ToString()][i].ToString());
                    }
                }

                if (WatchData.Count > 0)
                {
                    GM_DataRecorder.instance.WriteSteamingData_Batch_Watch(abc.ToString(), abc.ToString());
                }
                if (AirPotData.Count > 0)
                {
                    GM_DataRecorder.instance.WriteSteamingData_Batch_AirPot(abc.ToString(), abc.ToString());
                }

                WatchData.Clear();
                AirPotData.Clear();
            }
        }


        //INIT
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
