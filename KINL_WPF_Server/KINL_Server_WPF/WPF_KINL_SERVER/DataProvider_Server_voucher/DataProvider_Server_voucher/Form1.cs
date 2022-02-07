using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
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

        //메세지 정리1644221081875
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
                if (!PTPlist.ContainsKey(a.ToString()))
                {
                    List<string> tempList = new List<string>();
                    PTPlist.Add(a.ToString(), tempList);
                    while (PTPlist[a.ToString()].Count < 10)
                    {
                        try
                        {
                            timeOffset = DateTimeOffset.Now;
                            preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                            string sendStringData = $"<PTP>,{a.ToString()},{preUnixMilliseconds};";
                            byte[] sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            if (ClientManager.clientDic.ContainsKey(int.Parse(a)))
                            {
                                ClientManager.clientDic[int.Parse(a)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                            }
                            //foreach (var att in ClientManager.clientDic)
                            //{
                            //    if (att.Value.clientNumber == GetClinetNumber(a))
                            //    {
                            //        att.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                            //    }
                            //}
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR");
                            break;
                            //return;
                        }
                        Thread.Sleep(30);
                    }
                }
                //else
                //{
                //    while (PTPlist[a.ToString()].Count < 10)
                //    {
                //        try
                //        {
                //            timeOffset = DateTimeOffset.Now;
                //            preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                //            string sendStringData = $"<PTP>,{a.ToString()},{preUnixMilliseconds}";
                //            byte[] sendByteData = new byte[sendStringData.Length];
                //            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                //            //foreach (var att in ClientManager.clientDic)
                //            //{
                //            //    if (att.Value.clientNumber == a)
                //            //    {
                //            //        att.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                //            //    }
                //            //}
                //        }
                //        catch (Exception e)
                //        {
                //            Console.WriteLine("ERROR");
                //            break;
                //            //return;
                //        }
                //        Thread.Sleep(10);
                //    }
                //}
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

            for (int i = 0; i < PTPlist[sender.ToString()].Count; i++)
            {
                string[] a = PTPlist[sender.ToString()][i].Split(',');

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

            serverDelays.Add(sender, SD);
            clientDelays.Add(sender, CD);
            totalDelay.Add(sender, SCD);
            //확인용
            foreach (var item in ClientManager.clientDic)
            {
                Console.WriteLine("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
                Console.WriteLine("USERNUMBER" + item.Value.clientNumber);
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
            try
            {
                ChangeListView(targetClient.clientNumber.ToString(), StaticDefine.REMOVE_USER_LIST, null, null);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                Console.WriteLine("EEEEEEEEEEEEEEEEEEEEEEE");
            }
            ClientManager.clientDic.TryRemove(targetClient.clientNumber, out result);
            PTPlist.Remove(targetClient.clientNumber.ToString());
            totalDelay.Remove(targetClient.clientNumber.ToString());
            clientDelays.Remove(targetClient.clientNumber.ToString());
            serverDelays.Remove(targetClient.clientNumber.ToString());
           // string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.clientName);
            //ChangeListView(leaveLog, StaticDefine.ADD_ACCESS_LIST);
           // Console.WriteLine(leaveLog);
        }

        private void MessageParsing(string sender, string message)
        {
            lock (lockObj)
            {
                if (!msgDic.ContainsKey(sender))
                {
                    msgDic.Add(sender, message);
                }
                else
                {
                    msgDic[sender] += message;
                }
                string[] msgArray = new string[0];
                if (msgDic[sender].LastIndexOf(';') == msgDic[sender].Length)
                {
                    msgArray = msgDic[sender].Split(';');
                    msgDic[sender] = "";
                }
                else
                {
                    string[] temparray = msgDic[sender].Split(';');
                    msgArray = new string[temparray.Length - 1];
                    for (int i = 0; i < temparray.Length - 1; i++)
                    {
                        msgArray[i] = temparray[i];
                    }
                    msgDic[sender] = temparray[temparray.Length - 1];
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
            string sendStringData = "";
            byte[] sendByteData = new byte[sendStringData.Length];
            msgList = msgList.Replace("^", ",");
            if (msgList.Contains("<PTP>"))
            {
                Console.WriteLine(msgList);
            }


            if (msgList.Substring(0, 1) == "#")
            {
                string[] splitedMsgs = msgList.Split(',');
                //#1,iosName,DeviceName
                // 동기화
                //#1,1(ios),2(device)= true
                //#2,DeviceData Recived(Start)
                //#3
                //#4 Save= IOS send , server Recive
                //#5,DeviceName - Discnnect = IOS
                //server = ios,device disconnect
                switch (msgList.Substring(0, 2))
                {
                    case "#1":
                        sendStringData = "#2";
                        sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        try
                        {

                            Console.WriteLine("Device Connected");
                            string connectIos = splitedMsgs[1];
                            int connectIosNumber = GetClinetNumber(splitedMsgs[2]);

                            string connectDevice = splitedMsgs[2];
                            int connectDeviceNumber = GetClinetNumber(splitedMsgs[3]);

                            ClientManager.clientDic[connectIosNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                            ClientManager.clientDic[connectDeviceNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Connection Error");
                            return;
                        }
                        break;

                    case "#2":
                        //#2,DeviceData Recived(Start)
                        sendStringData = "#2";
                        sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        try
                        {
                            string connectDevice = splitedMsgs[1];
                            int connectDeviceNumber_local = GetClinetNumber(splitedMsgs[1]);

                            ClientManager.clientDic[connectDeviceNumber_local].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("RESEND ERROR");
                            return;
                        }
                        break;

                    //#3 = Data End
                    case "#3":

                        sendStringData = "#3";
                        sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        try
                        {
                            string connectDevices = splitedMsgs[1];
                            int connectDeviceNumbers = GetClinetNumber(splitedMsgs[1]);
                            ClientManager.clientDic[connectDeviceNumbers].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Stop Send Data Error");
                            return;
                        }
                        break;

                    // Save
                    case "#4":
                        SaveFile(int.Parse(sender));
                        SaveFile(GetClinetNumber(splitedMsgs[1]));
                        break;

                    case "#5":
                        // ERROR = disconnected device or ios callback
                        break;

                    //Close Socket;
                    case "#9":
                        RemoveClient(ClientManager.clientDic[int.Parse(sender)]);
                        break;
                }
                return;
            }
            else
            {
                string[] splitedMsg = msgList.Split(',');

                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);
                switch (receiver)
                {
                    case "<PTP>":
                        if (totalDelay.ContainsKey(sender))
                        {
                            return;
                        }
                        else
                        {
                            if (splitedMsg.Length < 6)
                            {
                                //    Console.WriteLine(splitedMsg);

                                timeOffset = DateTimeOffset.Now;
                                preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                                if (splitedMsg[3].Contains(";"))
                                {
                                    splitedMsg[3] = splitedMsg[3].TrimEnd(splitedMsg[3][splitedMsg[3].Length - 1]);
                                    Console.WriteLine(splitedMsg[3]);
                                }
                                sendStringData = $"{splitedMsg[0]},{splitedMsg[1]},{splitedMsg[2]},{splitedMsg[3]},{preUnixMilliseconds};";
                                sendByteData = new byte[sendStringData.Length];
                                sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                            }
                            else if (splitedMsg.Length >= 6 && splitedMsg.Length < 7)
                            {
                                if (splitedMsg[2].Contains("<PTP>") || splitedMsg[3].Contains("<PTP>"))
                                {
                                    return;
                                }
                                PTPlist[sender].Add(msgList);
                                if (PTPlist[sender].Count == 10)
                                {
                                    CalculatePTP(sender);
                                }
                                return;
                            }
                        }
                        break;
                    case "DEVICE":
                        if (!totalDelay.ContainsKey(sender))
                        {
                            return;
                        }
                        long tempTime = Convert.ToInt64(splitedMsg[1]);
                        long PTPTime = totalDelay[sender];
                        tempTime = (tempTime + PTPTime) + (serverDelays[sender] / 2); ;
                        StringBuilder aaaa = new StringBuilder();
                        aaaa = aaaa.Append(splitedMsg[0] + ",");
                        aaaa.Append(tempTime);
                        for (int i = 1; i < splitedMsg.Length; i++)
                        {
                            aaaa.Append("," + splitedMsg[i]);
                        }
                        string groupLogMessage = aaaa.ToString();
                        ChangeListView(sender, StaticDefine.DATA_SEND_START, groupLogMessage, "DEVICE");
                        return;
                    case "AIRPOT":
                        if (!totalDelay.ContainsKey(sender))
                        {
                            return;
                        }
                        try
                        {
                            //if (splitedMsg.Length == 11)
                            //{
                            long tempTime1 = Convert.ToInt64(splitedMsg[1]);
                            long PTPTime1 = totalDelay[sender];
                            tempTime1 = (tempTime1 + PTPTime1) - (serverDelays[sender] / 2);
                            StringBuilder aaaa1 = new StringBuilder();
                            aaaa1 = aaaa1.Append(splitedMsg[0] + ",");
                            aaaa1.Append(tempTime1);
                            for (int i = 1; i < splitedMsg.Length; i++)
                            {
                                aaaa1.Append("," + splitedMsg[i]);
                            }
                            string groupLogMessage2 = aaaa1.ToString();
                            //   Console.WriteLine(aaaa1);
                            ChangeListView(sender, StaticDefine.DATA_SEND_START, groupLogMessage2, "AIRPOT");
                            //}
                        }
                        catch (Exception ex)
                        {
                            return;
                        }
                        return;
                    case "WATCH":
                        if (!totalDelay.ContainsKey(sender))
                        {
                            return;
                        }
                        try
                        {
                            if (splitedMsg.Length == 11)
                            {
                                long tempTime2 = Convert.ToInt64(splitedMsg[1]);
                                long PTPTime2 = totalDelay[sender];
                                long aaa = 0;

                                if (!totalDelay.TryGetValue(sender, out aaa))
                                {
                                    foreach (var name in totalDelay)
                                    {
                                        Console.WriteLine($"{GetClinetNumber(name.Key)},{name.Value},{sender}");
                                    }
                                }
                                tempTime2 = (tempTime2 + PTPTime2) - (serverDelays[sender] / 2);
                                StringBuilder aaaa2 = new StringBuilder();
                                aaaa2 = aaaa2.Append(splitedMsg[0] + ",");
                                aaaa2.Append(tempTime2);
                                for (int i = 1; i < splitedMsg.Length; i++)
                                {
                                    aaaa2.Append("," + splitedMsg[i]);
                                }
                                //    Console.WriteLine(aaaa2);
                                string groupLogMessage3 = aaaa2.ToString();
                                //  Console.WriteLine(groupLogMessage);
                                ChangeListView(sender, StaticDefine.DATA_SEND_START, groupLogMessage3, "WATCH");
                            }
                        }
                        catch (Exception ex)
                        {
                            return;
                        }
                        return;
                    case "<TEST>":
                        if (!totalDelay.ContainsKey(sender))
                        {
                            return;
                        }
                        return;
                    default:
                        if (!totalDelay.ContainsKey(sender))
                        {
                            return;
                        }
                        Console.WriteLine("ERROR DEFAULT" + msgList);
                        break;
                }
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

        private string GetClinetName(string clinetPortNumber)
        {
            foreach (var item in ClientManager.clientDic)
            {
                if (item.Value.clientNumber == int.Parse(clinetPortNumber))
                {
                    return item.Value.clientName;
                }
            }
            return "";
        }


        // VIEW CHANGED
        private void ChangeListView(string clientNumber, int protocool, string decodeData, string deviceType)
        {
            string clientName = GetClinetName(clientNumber);
            // string clientNumbers = clientNumber;
            string iosNumbers = "";
            // Console.WriteLine(clientName);
            if (clientName.Contains("DEVICE"))
            {
                if (protocool == StaticDefine.ADD_USER)
                {
                    listBox3.BeginInvoke((Action)(() =>
                    {
                        ObservableCollection<string> list = new ObservableCollection<string>();
                        DeviceData.Add(clientNumber, list);
                        DeviceSend.Add(clientNumber, false);
                        listBox3.Items.Add("DEVICE : " + clientNumber + "Connect");
                    }));
                }
                else if (protocool == StaticDefine.REMOVE_USER_LIST)
                {
                    listBox3.BeginInvoke((Action)(() =>
                    {
                        listBox3.Items.Add("DEVICE : " + clientNumber + "Disconnect");
                    }));
                }
                else if (protocool == StaticDefine.DATA_SEND_START)
                {
                    if (DeviceSend[clientNumber] == true)
                    {
                        DeviceData[clientNumber].Add(decodeData);
                        //   Console.WriteLine("DATAADD");
                    }
                    else
                    {
                        listBox3.BeginInvoke((Action)(() =>
                        {
                            listBox3.Items.Add("DEVICE:" + clientNumber + "Send");
                        }));
                        DeviceSend[clientNumber] = true;
                    }
                }
            }

            else
            {
                if (protocool == StaticDefine.ADD_USER)
                {
                    Watch_DeviceSend.Add(clientNumber, false);
                    AirPot_DeviceSend.Add(clientNumber, false);
                    ObservableCollection<string> watchList = new ObservableCollection<string>();
                    ObservableCollection<string> airpotList = new ObservableCollection<string>();
                    WatchData.Add(clientNumber, watchList);
                    AirPotData.Add(clientNumber, airpotList);
                    listBox3.BeginInvoke((Action)(() =>
                    {
                        Console.WriteLine(clientNumber);
                        listBox3.Items.Add("IOS" + clientName + "Connect");

                    }));
                }
                else if (protocool == StaticDefine.REMOVE_USER_LIST)
                {
                    listBox3.BeginInvoke((Action)(() =>
                    {
                        listBox3.Items.Add("IOS:" + clientNumber + "Disconnect");
                    }));
                }

                switch (deviceType)
                {

                    case "WATCH":
                        if (protocool == StaticDefine.ADD_USER)
                        {
                            listBox2.BeginInvoke((Action)(() =>
                            {
                                listBox2.Items.Add("IOS:" + clientNumber + "Connect");
                            }));
                        }
                        else if (protocool == StaticDefine.REMOVE_USER_LIST)
                        {
                            listBox2.BeginInvoke((Action)(() =>
                            {
                                listBox2.Items.Add("IOS:" + clientNumber + "Disconnect");
                            }));
                        }
                        else if (protocool == StaticDefine.DATA_SEND_START)
                        {
                            if (Watch_DeviceSend[clientNumber])
                            {
                                //  Console.WriteLine(senderPort);
                                WatchData[clientNumber].Add(decodeData);
                                // Console.WriteLine("DATAWATCH");
                            }
                            else
                            {
                                listBox2.BeginInvoke((Action)(() =>
                                {
                                    listBox2.Items.Add("IOS:" + clientNumber + "Send");
                                }));
                                Watch_DeviceSend[clientNumber] = true;
                            }
                        }
                        break;

                    case "AIRPOT":
                        if (protocool == StaticDefine.ADD_USER)
                        {
                            listBox1.BeginInvoke((Action)(() =>
                            {
                                listBox1.Items.Add("AIRPOT:" + clientNumber + "Connect");
                            }));
                        }
                        else if (protocool == StaticDefine.REMOVE_USER_LIST)
                        {
                            listBox1.BeginInvoke((Action)(() =>
                            {
                                listBox1.Items.Add("AIRPOT:" + clientNumber + "Disconnect");
                            }));
                        }
                        else if (protocool == StaticDefine.DATA_SEND_START)
                        {
                            if (AirPot_DeviceSend[clientNumber])
                            {
                                //Console.WriteLine(clientNumber);
                                AirPotData[clientNumber].Add(decodeData);
                                // Console.WriteLine("DATAAIRPOT");
                            }
                            else
                            {
                                listBox1.BeginInvoke((Action)(() =>
                                {
                                    listBox1.Items.Add("AIRPOT:" + clientNumber + "Send");
                                }));
                                AirPot_DeviceSend[clientNumber] = true;
                            }
                        }
                        break;
                }
            }
        }


        //FILE SAVED

        private void SaveFile(int sender)
        {
            string SaveConfig = GetClinetName(sender.ToString());
            if (SaveConfig.Contains("DEVICE"))
            {
                for (int i = 0; i < DeviceData[sender.ToString()].Count; i++)
                {
                    GM_DataRecorder.instance.Enqueue_Data(sender.ToString(), DeviceData[sender.ToString()][i].ToString());
                }
                if (DeviceData[sender.ToString()].Count > 0)
                {
                    foreach (var clientNames in ClientManager.clientDic.Values)
                    {
                        if (clientNames.clientNumber == sender)
                        {
                            GM_DataRecorder.instance.WriteSteamingData_Batch_Device(sender.ToString(), clientNames.clientName);
                        }
                    }
                    DeviceData[sender.ToString()].Clear();
                }
            }
            else if (SaveConfig.Contains("IOS"))
            {
                Console.WriteLine("IOS");
                if (AirPotData[sender.ToString()].Count > 1)
                {

                    for (int i = 0; i < AirPotData[sender.ToString()].Count; i++)
                    {
                        GM_DataRecorder.instance.Enqueue_Data_A(sender.ToString(), AirPotData[sender.ToString()][i].ToString());
                    }
                }
                if (WatchData[sender.ToString()].Count > 1)
                {
                    for (int i = 0; i < WatchData[sender.ToString()].Count; i++)
                    {
                        GM_DataRecorder.instance.Enqueue_Data_W(sender.ToString(), WatchData[sender.ToString()][i].ToString());
                    }
                }

                if (WatchData[sender.ToString()].Count > 0)
                {
                    foreach (var clientNames in ClientManager.clientDic.Values)
                    {
                        if (clientNames.clientNumber == sender)
                        {
                            GM_DataRecorder.instance.WriteSteamingData_Batch_Watch(sender.ToString(), clientNames.clientName);
                        }
                    }

                    WatchData[sender.ToString()].Clear();
                }
                if (AirPotData[sender.ToString()].Count > 0)
                {
                    foreach (var clientNames in ClientManager.clientDic.Values)
                    {
                        if (clientNames.clientNumber == sender)
                        {
                            GM_DataRecorder.instance.WriteSteamingData_Batch_AirPot(sender.ToString(), clientNames.clientName);
                        }
                    }
                    AirPotData[sender.ToString()].Clear();
                }
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

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
