﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Linq;

namespace DataProvider_Server_voucher
{
    public partial class Form1 : Form
    {
        object lockObj = new object();
        object _ptpLock = new object();

        //Connect Checker
        private Dictionary<int, int> deviceAI = new Dictionary<int, int>();
        private Dictionary<int, int> deviceConnection = new Dictionary<int, int>();
        private Dictionary<int, string> deviceMachineName = new Dictionary<int, string>();
        int server_threadDelay = 20;
        //실제 저장장소
        private Dictionary<string, ObservableCollection<string>> DeviceData = new Dictionary<string, ObservableCollection<string>>();
        private Dictionary<string, ObservableCollection<string>> AirpodData = new Dictionary<string, ObservableCollection<string>>();
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

        //분석 AI
        public static Dictionary<string, string> ai_FeedData = new Dictionary<string, string>();

        Task conntectCheckThread = null;

        private Dictionary<string, bool> DeviceSend = new Dictionary<string, bool>();
        private Dictionary<string, bool> Watch_DeviceSend = new Dictionary<string, bool>();
        private Dictionary<string, bool> Airpod_DeviceSend = new Dictionary<string, bool>();


        public Form1()
        {
            InitializeComponent();
            MainServerStart();
            ClientManager.messageParsingAction += MessageParsing;
            ClientManager.ChangeListViewAction += ChangeListView;
            ClientManager.PTP_Synchronized += CheckPTP;
            fileWatcher.watcher();
            fileWatcher.watcher_sub();
        }

        private void PTPEndChecker(string sender)
        {
            string sendStringData = "<PTPEND>;";
            byte[] sendByteData = new byte[sendStringData.Length];
            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
            foreach (var item in ClientManager.clientDic)
            {
                if (item.Value.clientNumber == int.Parse(sender))
                {
                    item.Value.tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                }
            }
        }
        /// <summary>
        /// GET CLIENT NUMBER AND CHECKED PTP
        /// </summary>
        /// <param name="a">ptp 를 진행할 때 필요한 client number</param>
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
                            Thread.Sleep(1);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR");
                            break;
                            //return;
                        }
                        Thread.Sleep(server_threadDelay);
                    }
                }
                PTPEndChecker(a);
                conntectCheckThread = new Task(ConnectCheckLoop);
                conntectCheckThread.Start();
                return;
            }

        }
        /// <summary>
        /// PTP 10개의 데이터를 받아 가장 짧은 딜레이 기준 최적화
        /// </summary>
        /// <param name="sender">client port Number</param>
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
            //확인용 DebugLine
            Console.WriteLine("END");
            listBox4.BeginInvoke((Action)(() =>
            {
                ObservableCollection<string> list = new ObservableCollection<string>();
                if (listBox4.Items.Contains("PTP " + sender))
                {
                    return;
                }
                else
                {
                    listBox4.Items.Add("PTP " + sender);
                }
            }));
            foreach (var item in ClientManager.clientDic)
            {
                Console.WriteLine("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
                Console.WriteLine("USERNUMBER" + item.Value.clientNumber);
                Console.WriteLine("DELAY:" + totalDelay[item.Value.clientNumber.ToString()]);
                Console.WriteLine("SERVERDELAY:" + serverDelays[item.Value.clientNumber.ToString()]);
                Console.WriteLine("CLIENTDELAY:" + clientDelays[item.Value.clientNumber.ToString()]);
            }
        }
        /// <summary>
        /// ECHO(HeartBit) 스레드, 서버의 접속을 확인하고 비접속시 디스커넥트 시킴
        /// </summary>
        private void ConnectCheckLoop()
        {
            while (true)
            {
                if (ClientManager.clientDic.Count > 0)
                {

                    foreach (var item in ClientManager.clientDic)
                    {
                        try
                        {
                            string sendStringData = "<TEST>;";
                            byte[] sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
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
        }
        /// <summary>
        /// 클라이언트 제거
        /// </summary>
        /// <param name="targetClient">해당 클라이언트 데이터 제거</param>
        private void RemoveClient(ClientData targetClient)
        {

            int disconnectDevice;
            ClientData result = null;
            try
            {
                deviceConnection.TryGetValue(targetClient.clientNumber, out disconnectDevice);
                if (ClientManager.clientDic[disconnectDevice].isSend == true)
                {
                    string sendStringDatas = "#3;";
                    byte[] sendByteDatas = new byte[sendStringDatas.Length];
                    sendByteDatas = Encoding.UTF8.GetBytes(sendStringDatas);
                    ClientManager.clientDic[disconnectDevice].isSend = false;
                    ClientManager.clientDic[disconnectDevice].tcpClient.GetStream().Write(sendByteDatas, 0, sendByteDatas.Length);
                    ChangeListView(targetClient.clientNumber.ToString(), StaticDefine.REMOVE_USER_LIST, null, null);
                }
                deviceConnection.Remove(targetClient.clientNumber);
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
            ChangeListView(targetClient.clientNumber.ToString(), StaticDefine.REMOVE_USER_LIST, null, null);
            listBox4.BeginInvoke((Action)(() =>
            {
                listBox4.Items.Add("PTP:" + targetClient.clientName + "-Leave");
            }));
        }
        /// <summary>
        /// 메세지 파싱
        /// </summary>
        /// <param name="sender">보내는 클라이언트 port number</param>
        /// <param name="message">전송 된 데이터</param>
        private void MessageParsing(string sender, string message)
        {
            //// Console.WriteLine(message);
            //testContain += message;
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
                }
            }
        }
        /// <summary>
        /// 파싱 된 메세지 해독 및 전달
        /// </summary>
        /// <param name="msgList"></param>
        /// <param name="sender"></param>
        // MESSAGE PARSSED
        private void SendMsgToClient(string msgList, string sender)
        {
            //try
            //{
            // Console.WriteLine("MSG: "+ msgList);
            listBox3.BeginInvoke((Action)(() =>
            {
                listBox5.Items.Add(msgList);
                listBox5.SelectedIndex = listBox5.Items.Count - 1;
            }));
            string parsedMessage = "";
            string receiver = "";
            string sendStringData = "";
            byte[] sendByteData = new byte[sendStringData.Length];
            msgList = msgList.Replace("^", ",");

            if (msgList.IndexOf("<") == 0)
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
                            for (int i = 1; i < splitedMsg.Length; i++)
                            {
                                long a;
                                if (!long.TryParse(splitedMsg[i], out a))
                                {
                                    Console.WriteLine(splitedMsg + "\n" + msgList);
                                    return;
                                }
                            }
                            if (splitedMsg.Length < 6)
                            {
                                //    Console.WriteLine(splitedMsg);
                                if (splitedMsg.Equals(""))
                                {

                                }
                                else
                                {
                                    timeOffset = DateTimeOffset.Now;
                                    preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                                    sendStringData = $"{splitedMsg[0]},{splitedMsg[1]},{splitedMsg[2]},{splitedMsg[3]},{preUnixMilliseconds};";
                                    sendByteData = new byte[sendStringData.Length];
                                    Console.WriteLine(sendStringData);
                                    sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                    ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                }
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
                }
            }

            else if (msgList.IndexOf("#") == 0)
            {
                string[] splitedMsgs = msgList.Split(',');

                switch (msgList.Substring(0, 2))
                {
                    //Connect
                    // #1 , IOS, DEVICE
                    case "#1":
                        string connectIos = splitedMsgs[1];
                        int connectIosNumber = GetClinetNumber(splitedMsgs[1]);

                        string connectDevice = splitedMsgs[2];
                        int connectDeviceNumber = GetClinetNumber(splitedMsgs[2]);

                        //string connectDevice_exerciseName = splitedMsgs[3];
                        if (connectDeviceNumber == -1)
                        {

                        }
                        else
                        {
                            // gm_db.CheckSchema_FromExercise(GetClinetName(splitedMsgs[2]), splitedMsgs[3]);
                        }
                        //TODO : 이 친구를 운동으로 사용 할 예정

                        // 두 디바이스 커넥트
                        deviceConnection.Add(connectIosNumber, connectDeviceNumber);
                        deviceMachineName.Add(connectDeviceNumber, splitedMsgs[3]);
                        int counters = GM_DB.Instance.CheckTable(splitedMsgs[1], splitedMsgs[3]);
                        Console.WriteLine("counter : " + counters);
                        break;

                    //Data Send
                    case "#2":
                        try
                        {
                            sendStringData = "#2;";
                            sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            int values = 0;
                            deviceConnection.TryGetValue(int.Parse(sender), out values);
                            int connectDeviceNumber_local = values;
                            Console.WriteLine("접속한 클라이언트의 수: " + ClientManager.clientDic.Count);
                            if (ClientManager.clientDic[connectDeviceNumber_local].isSend == false)
                            {
                                ClientManager.clientDic[connectDeviceNumber_local].isSend = true;
                                ClientManager.clientDic[connectDeviceNumber_local].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                Console.WriteLine(ClientManager.clientDic[connectDeviceNumber_local].clientName);
                            }
                        }
                        catch (Exception e)
                        {
                            sendStringData = "#0;";
                            sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            int connectDeviceNumber_local = int.Parse(sender);
                            ClientManager.clientDic[connectDeviceNumber_local].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                            return;
                        }
                        break;

                    //#3 = Data End
                    case "#3":
                        sendStringData = "#3;";
                        sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        try
                        {
                            sendStringData = "#3;";
                            sendByteData = new byte[sendStringData.Length];
                            sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                            int values = 0;
                            string names = string.Empty;
                            deviceConnection.TryGetValue(int.Parse(sender), out values);
                            deviceMachineName.TryGetValue(values, out names);

                            int connectDeviceNumber_local = values;
                            if (ClientManager.clientDic[connectDeviceNumber_local].isSend == true)
                            {
                                ClientManager.clientDic[connectDeviceNumber_local].isSend = false;
                                ClientManager.clientDic[connectDeviceNumber_local].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                Console.WriteLine("DATA SAVE");
                            }
                            //deviceConnection.TryGetValue(int.Parse(sender), out values);
                            Console.WriteLine("1회 호출됨");
                            fileWatcher.Clear();
                            SaveFile(int.Parse(sender));
                            Send_AI(sender,values);
                            // 저장이 완료되면 클라이언트에게 AI분석 요청 의뢰

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Stop Send Data Error");
                            //ClientData tempSender;
                            //ClientManager.clientDic.TryGetValue(int.Parse(sender), out tempSender);
                            //if (tempSender != null)
                            //{
                            //    RemoveClient(tempSender);
                            //}
                            return;
                        }
                        break;
                    // AI Server Send
                    // #4
                    case "#4":
                        Console.WriteLine("AI 분석 의뢰");
                        string AI_name;
                        int AI_clientNumber;
                        try
                        {
                            foreach (var item in ClientManager.clientDic)
                            {
                                if (item.Value.clientName == "AI")
                                {
                                    Console.WriteLine("AI 찾았음!");
                                    AI_name = item.Value.clientName;
                                    AI_clientNumber = GetClinetNumber("AI");
                                    Console.WriteLine(sender.ToString());
                                    Console.WriteLine(GetClinetName(sender));
                                    sendStringData = "#4," + ai_FeedData[GetClinetName(sender)] + "," + sender.ToString() + ";";
                                    Console.WriteLine(sendStringData);
                                    sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                    ClientManager.clientDic[AI_clientNumber].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("AI 없음");
                        }

                        break;

                    case "#5":

                        // #5 , all =  테이블 명
                        // #5 , exercise = 해당 테이블의 SELECT * FROM table_name;
                        // 5개씩만 받고 , + 표시를 누르면 추가 요청을 하는 방식.
                        try
                        {
                            Thread.Sleep(1000);
                            if (splitedMsgs[1] == "All")
                            {
                                string data = GM_DB.Instance.Search_All_Table(GetClinetName(sender));
                                string[] sendByteDatas = data.Split(';');
                                for (int i = 0; i < sendByteDatas.Length - 1; i++)
                                {
                                    sendStringData = "<#5>," + sendByteDatas[i] + ';';
                                    sendByteData = new byte[sendStringData.Length];
                                    sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                    ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                }
                            }
                            else if (splitedMsgs[1] == "DataPath")
                            {
                                string data = GM_DB.Instance.Search_Table(GetClinetName(sender), "DataPath");
                                string[] sendByteDatas = data.Split(',');

                                for (int i = 0; i < sendByteDatas.Length - 1; i++)
                                {
                                    sendStringData = "<#5>," + sendByteDatas[i] + ';';
                                    sendByteData = new byte[sendStringData.Length];
                                    sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                    ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                }
                            }
                            else if (splitedMsgs[1] == "Data")
                            {
                                string data = GM_DB.Instance.Search_Table(GetClinetName(sender), "Data");
                                string[] sendByteDatas = data.Split(',');

                                for (int i = 0; i < sendByteDatas.Length - 1; i++)
                                {
                                    sendStringData = "<#5>," + sendByteDatas[i] + ';';
                                    sendByteData = new byte[sendStringData.Length];
                                    sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                    ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                }
                            }
                            else
                            {
                                int counter_c = GM_DB.Instance.CheckTable(GetClinetName(sender), splitedMsgs[1]);
                                string data = null;
                                if (counter_c == 1)
                                {
                                    data = GM_DB.Instance.Search_Table(GetClinetName(sender), splitedMsgs[1]);

                                    if (data == null)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        string[] sendByteDatas = data.Split(',');
                                    }
                                }
                                if (data != null)
                                {
                                    string[] sendByteDatas = data.Split(';');
                                    for (int i = 0; i < sendByteDatas.Length - 1; i++)
                                    {
                                        sendStringData = "<#5>," + splitedMsgs[1] + "," + sendByteDatas[i] + ';';
                                        Console.WriteLine(sendStringData);
                                        sendByteData = new byte[sendStringData.Length];
                                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                        ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {

                        }
                        sendStringData = "<#6>;";
                        sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                        Console.WriteLine("DB Search");
                        // IOS request DB data
                        break;
                    case "#8":
                        Console.WriteLine(msgList);
                        GM_DB.Instance.Update1RM_DataTable(GetClinetName(splitedMsgs[1]), splitedMsgs[2], splitedMsgs[3], splitedMsgs[4]);

                        //Shoulder Press Return
                        int counter = GM_DB.Instance.CheckTable(GetClinetName(splitedMsgs[1]), splitedMsgs[2]);
                        string datas = null;
                        if (counter == 1)
                        {
                            datas = GM_DB.Instance.Search_Table(GetClinetName(splitedMsgs[1]), splitedMsgs[2]);

                            if (datas == null)
                            {
                                return;
                            }
                            else
                            {
                                string[] sendByteDatas = datas.Split(',');
                            }
                        }
                        if (datas != null)
                        {
                            string[] sendByteDatas = datas.Split(';');
                            for (int i = 0; i < sendByteDatas.Length - 1; i++)
                            {
                                sendStringData = "<#5>," + splitedMsgs[2] + "," + sendByteDatas[i] + ';';
                                Console.WriteLine("DDDDDDDDDDDDDDDDDDDDDDDDDDDD:"+sendStringData);
                                sendByteData = new byte[sendStringData.Length];
                                sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                                ClientManager.clientDic[int.Parse(splitedMsgs[1])].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                            }
                        }
                        sendStringData = "<#6>;";
                        sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);
                        ClientManager.clientDic[int.Parse(splitedMsgs[1])].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
                        Console.WriteLine("DB Search");
                        break;
                    //Close Socket;
                    case "#9":
                        RemoveClient(ClientManager.clientDic[int.Parse(sender)]);
                        break;
                }
            }
            else
            {
                //#으로 포함된 프로토콜이 아닐 경우
                string[] splitedMsg = msgList.Split(',');

                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);
                switch (receiver)
                {
                    case "AI":
                        if (!totalDelay.ContainsKey(sender))
                        {
                            return;
                        }
                        Console.WriteLine("AI COMMM");
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
                            if (splitedMsg.Length == 10)
                            {
                                bool isRight = true;
                                for (int i = 1; i < splitedMsg.Length; i++)
                                {
                                    if (i < 3)
                                    {
                                        if(splitedMsg[i].Length==13)
                                        {
                                            double a;
                                            isRight = double.TryParse(splitedMsg[i], out a);
                                            if (!isRight)
                                            {
                                                Console.WriteLine(splitedMsg[i]);
                                                isRight = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            isRight = true;
                                            break;
                                        }
                                    }
                                    else if (i == 3)
                                    {
                                        if (splitedMsg[i].Length == 1)
                                        {
                                            float b;
                                            isRight = float.TryParse(splitedMsg[i], out b);
                                            if (!isRight)
                                            {
                                                Console.WriteLine(splitedMsg[i]);
                                                isRight = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            isRight = true;
                                            break;
                                        }
                                    }
                                    if (splitedMsg[i].Length <= 5)
                                    {
                                        float b;
                                        isRight = float.TryParse(splitedMsg[i], out b);
                                        if (!isRight)
                                        {
                                            Console.WriteLine(splitedMsg[i]);
                                            isRight = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        isRight = true;
                                        break;
                                    }
                                    //if (splitedMsg[i].Contains("<") && splitedMsg[i].Contains("W"))
                                    //{
                                    //    return;
                                    //}
                                }
                                if (isRight)
                                {
                                    long tempTime1 = Convert.ToInt64(splitedMsg[1]);
                                    long PTPTime1 = totalDelay[sender];
                                    tempTime1 = (tempTime1 + PTPTime1) - (serverDelays[sender] / 2);
                                    StringBuilder aaaa1 = new StringBuilder();
                                    aaaa1 = aaaa1.Append(splitedMsg[0] + ",");
                                    aaaa1.Append(tempTime1);
                                    for (int j = 1; j < splitedMsg.Length; j++)
                                    {
                                        aaaa1.Append("," + splitedMsg[j]);
                                    }
                                    string groupLogMessage2 = aaaa1.ToString();
                                   // Console.WriteLine(aaaa1);
                                    ChangeListView(sender, StaticDefine.DATA_SEND_START, groupLogMessage2, "AIRPOT");
                                    //}
                                }
                                else if (!isRight)
                                {
                                    Console.WriteLine("???????????");
                                }
                            }
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

                                for (int i = 0; i < splitedMsg.Length; i++)
                                {
                                    if (splitedMsg[i].Contains("<") && splitedMsg[i].Contains("AI"))
                                    {
                                        return;
                                    }
                                }
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
                                for (int j = 1; j < splitedMsg.Length; j++)
                                {
                                    aaaa2.Append("," + splitedMsg[j]);
                                }
                                //    Console.WriteLine(aaaa2);
                                string groupLogMessage3 = aaaa2.ToString();
                                //Console.WriteLine(groupLogMessage3);
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
            else if (clientName.Contains("AI"))
            {
                if (protocool == StaticDefine.ADD_USER)
                {
                    listBox3.BeginInvoke((Action)(() =>
                    {
                        listBox3.Items.Add("AI: " + clientNumber + "Connect");
                    }));
                }
                else if (protocool == StaticDefine.REMOVE_USER_LIST)
                {
                    listBox3.BeginInvoke((Action)(() =>
                    {
                        listBox3.Items.Add("AI : " + clientNumber + "Disconnect");
                    }));
                }
            }

            else
            {
                if (protocool == StaticDefine.ADD_USER)
                {
                    Watch_DeviceSend.Add(clientNumber, false);
                    Airpod_DeviceSend.Add(clientNumber, false);
                    ObservableCollection<string> watchList = new ObservableCollection<string>();
                    ObservableCollection<string> AirpodList = new ObservableCollection<string>();
                    WatchData.Add(clientNumber, watchList);
                    AirpodData.Add(clientNumber, AirpodList);
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
                        string sendStringData = "<TEST>";
                        byte[] sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.UTF8.GetBytes(sendStringData);

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
                            if (Airpod_DeviceSend[clientNumber])
                            {
                                //Console.WriteLine(clientNumber);
                                AirpodData[clientNumber].Add(decodeData);
                                // Console.WriteLine("DATAAirpod");
                            }
                            else
                            {
                                listBox1.BeginInvoke((Action)(() =>
                                {
                                    listBox1.Items.Add("AIRPOT:" + clientNumber + "Send");
                                }));
                                Airpod_DeviceSend[clientNumber] = true;
                            }
                        }
                        break;
                }
            }
        }


        //FILE SAVED
        private void SaveFile(int sender)
        {
            int clientPort = 0;
            string deivceMachineName = string.Empty;
            try
            {
                deviceConnection.TryGetValue(sender, out clientPort);
                deviceMachineName.TryGetValue(clientPort, out deivceMachineName);
            }
            catch (Exception ex)
            {
            }
            string SaveIOS = GetClinetName(sender.ToString());
            string SaveDevice = GetClinetName(clientPort.ToString());

            if (SaveDevice.Contains("DEVICE"))
            {
                for (int i = 0; i < DeviceData[clientPort.ToString()].Count; i++)
                {
                    GM_DataRecorder.instance.Enqueue_Data(clientPort.ToString(), DeviceData[clientPort.ToString()][i].ToString());
                }
                if (DeviceData[clientPort.ToString()].Count > 0)
                {
                    foreach (var clientNames in ClientManager.clientDic.Values)
                    {
                        if (clientNames.clientNumber == clientPort)
                        {
                            GM_DataRecorder.instance.WriteSteamingData_Batch_Device(clientPort.ToString(), SaveIOS, deivceMachineName);
                        }
                    }
                    DeviceData[clientPort.ToString()].Clear();
                }
            }
            if (SaveIOS.Contains("IOS"))
            {
                // Console.WriteLine("IOS");
                if (AirpodData[sender.ToString()].Count > 1)
                {

                    for (int i = 0; i < AirpodData[sender.ToString()].Count; i++)
                    {
                        GM_DataRecorder.instance.Enqueue_Data_A(sender.ToString(), AirpodData[sender.ToString()][i].ToString());
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
                if (AirpodData[sender.ToString()].Count > 0)
                {
                    foreach (var clientNames in ClientManager.clientDic.Values)
                    {
                        if (clientNames.clientNumber == sender)
                        {
                            GM_DataRecorder.instance.WriteSteamingData_Batch_Airpod(sender.ToString(), clientNames.clientName);
                        }
                    }
                    AirpodData[sender.ToString()].Clear();
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
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        public void Send_AI(string sender, int values)
        {
            string sendStringData = "#4;";
            var sendByteData = Encoding.UTF8.GetBytes(sendStringData);
            ClientManager.clientDic[int.Parse(sender)].tcpClient.GetStream().Write(sendByteData, 0, sendByteData.Length);
            Console.WriteLine("#4 Send");
            Console.WriteLine("SENDER : " + sender);
            deviceConnection.Remove(int.Parse(sender));
            deviceMachineName.Remove(values);
        }
    }
}
