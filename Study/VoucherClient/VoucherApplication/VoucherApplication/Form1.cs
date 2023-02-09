using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace VoucherApplication
{

    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        ///         /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);
        /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern uint TimeEndPeriod(uint uMilliseconds);

        //Send Message Extern int
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 IParam);

        // Server Controll Value
        bool server_send = false;
        private const int WM_SETREDRAW = 11;
        bool recorddata = false;
        public static Form1 inst;
        Stopwatch stopwatch;
        public string initialis, index, weight;


        // Client Cotroll Value
        public string clientNAME;
        TcpClient client = null;
        Thread ReceiveThread = null;
        NetworkStream netStream = null;
        bool bLogin;


        // Main
        public Form1()
        {
            if (inst == null)
            {
                inst = this;
            }
            InitializeComponent();
            DataRecorder datainstance = new DataRecorder();
            datainstance.Awake();
            stopwatch = new Stopwatch();
            Start();
        }


        #region Program Life Cycle
        public void Start()
        {
            string[] port = SerialPort.GetPortNames();
            SetDataText(port.Length.ToString());
            for (int i = 0; i < port.Length; i++)
            {
                Console.WriteLine(port[i]);
            }
            timeOffset = DateTimeOffset.Now;
            rTh = new Thread(EnqueueData);
            rTh.IsBackground = false;
        }
        private void streamOn()
        {
            string[] port = SerialPort.GetPortNames();
            for (int i = 0; i < port.Length; i++)
            {
                Console.WriteLine(port[i]);
            }
            if (port.Length > 0)
            {
                if (stream != null)
                {
                    return;
                }
                stream = new SerialPort(port[0], 115200);
                stream.Open();
                char[] buffer = new char[1];
                buffer[0] = '0';
                stream.Write(buffer, 0, 1);
                //stream.Write("0");
                Console.WriteLine("Open");
                timeOffset = DateTimeOffset.Now;
                rTh1 = new Thread(FIxedUpdate);
                rTh1.IsBackground = false;
                rTh1.Start();
            }
            else
            {
                SetDataText("Null Port");
            }
        }


        public void FIxedUpdate()
        {
            Console.WriteLine("DATASEND!!!!");
            while (recorddata)
            {
                TimeBeginPeriod(1);
                stopwatch.Reset();
                stopwatch.Start();
                if (stream != null)
                {
                    if (server_send == true)
                    {
                        DataUpdate();
                    }
                }
                stopwatch.Stop();

                TimeEndPeriod(1);
            }
            if (stream != null)
            {
                stream.Close();
                if (rTh.ThreadState == System.Threading.ThreadState.Suspended)
                {
                    Console.WriteLine(rTh.ThreadState.ToString());
                    rTh.Resume();
                }
                else if (rTh.ThreadState != System.Threading.ThreadState.Running)
                {
                    rTh.Start();
                }
            }
        }

        /// <summary>
        ///  UPDATE TIME
        /// </summary>
        public void DataUpdate()
        {
            try
            {
                if (stream != null)
                {
                    receivedstring = stream.ReadLine();
                }
                if (easurement_timing_budget_check)
                {
                    if (receivedstring != null)
                    {
                        if (receivedstring.IndexOf(",DKU") > -1)
                        {
                            if (DataRecorder.instance.machineName == null)
                            {
                                DataRecorder.instance.machineName = receivedstring.Substring(receivedstring.IndexOf("DKU"));
                                DataRecorder.instance.machineName = DataRecorder.instance.machineName.Remove(DataRecorder.instance.machineName.IndexOf("#"));
                                Console.WriteLine(DataRecorder.instance.machineName);
                            }
                            receivedstring = receivedstring.Remove(receivedstring.IndexOf(",DKU"), 8);
                        }
                        receivedstring = receivedstring.Replace("\r", "");
                        receivedstring = receivedstring.Remove(0, 1);
                        timeOffset = DateTimeOffset.Now;
                        UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                        byte[] buf = Encoding.UTF8.GetBytes($"DEVICE,{UnixMilliseconds.ToString()},4,{UnixMilliseconds.ToString()},{receivedstring};");
                        //  Console.WriteLine(buf);
                        client.GetStream().Write(buf, 0, buf.Length);
                        DataRecorder.instance.Queue_ex_01.Enqueue(UnixMilliseconds.ToString() + "," + receivedstring);
                        string data = preUnixMilliseconds + "delay:" + (UnixMilliseconds - preUnixMilliseconds).ToString() + "count:" + DataRecorder.instance.Queue_ex_01.Count.ToString() + ",time" + DateTime.Now.ToString("HHmmss.fff") + ",data:" + receivedstring;
                        DataTextBox.Invoke(new Action(() => SetDataText(data)));
                        preUnixMilliseconds = UnixMilliseconds;
                        stream.BaseStream.Flush();
                    }
                    else
                    {
                        Console.WriteLine("null");
                    }
                    if (DataRecorder.instance.Queue_ex_01.Count > 300)
                    {
                        if (rTh.ThreadState == System.Threading.ThreadState.Unstarted)
                        {
                            rTh.Start();
                        }
                        else if (rTh.ThreadState == System.Threading.ThreadState.Suspended)
                        {
                            rTh.Resume();
                        }
                    }
                }
                else
                {
                    easurement_timing_budget_check = true;
                }
            }
            catch (Exception e)
            {
                //DataTextBox.BeginInvoke(new Action(() => DataTextBox.Text = e.ToString()));
            }
        }


        #endregion


        SerialPort stream;
        public string receivedstring;

        long preUnixMilliseconds;
        long UnixMilliseconds;
        DateTimeOffset timeOffset;
        public Thread rTh;
        public Thread rTh1;
        bool easurement_timing_budget_check = false;
        private static AutoResetEvent are = new AutoResetEvent(false);

        #region Button Click Event
        /// <summary>
        /// Startbutton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void metroButton1_Click(object sender, EventArgs e)
        {
            //streamOn();
            Login();
        }


        #endregion


        #region Data Controll
        void EnqueueData()
        {
            while (true)
            {
                DataRecorder.instance.SetFileName();
                DataRecorder.instance.WriteSteamingData_Batch();
                rTh.Suspend();
            }
        }


        private void metroButton2_Click(object sender, EventArgs e)
        {
            bLogin = false;
            client.Close();
            client = null;

            if (recorddata)
            {
                recorddata = false;
            }
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            return;
        }


        #endregion

        #region UI CONTROLL

        public void SetDataText(string _data)
        {
            DataTextBox.AppendText(_data + "\n");
        }
        private void DataTextBox_TextChanged(object sender, EventArgs e)
        {

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Process.GetCurrentProcess().Kill();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
        #endregion

        #region Client Controll


        /// <summary>
        /// Player Login
        /// </summary>
        private void Login()
        {
            try
            {
                if (!bLogin)
                {
                    string parsedName = "%^&";
                    parsedName += "6_DEVICE;";

                    clientNAME = parsedName;

                    client = new TcpClient();

                    //동국대학교 계산관 IP
                    client.Connect("210.94.216.195", 4545);

                    // 서버 컴퓨터 IP
                    //  client.Connect("210.94.167.45", 4545);

                    byte[] byteData = new byte[parsedName.Length];
                    byteData = Encoding.UTF8.GetBytes(parsedName);
                    client.GetStream().Write(byteData, 0, byteData.Length);
                    ReceiveThread = new Thread(RecieveMessage);
                    ReceiveThread.Start();
                    MessageBox.Show("서버에 접속됨");
                    bLogin = true;
                }
            }
            catch
            {
                MessageBox.Show("서버연결에 실패하였습니다.\n서버를 기동해주세요", "Server Error");
                client = null;
                bLogin = false;
            }
        }
        private void RecieveMessage()
        {
            while (true)
            {
                try
                {
                    netStream = client.GetStream();

                    int BUFFERSIZE = client.ReceiveBufferSize;
                    byte[] buffer = new byte[BUFFERSIZE];
                    int bytes = netStream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytes);
                    string[] splited = message.Split(';');
                    for (int i = 0; i < splited.Length; i++)
                    {
                        if (splited[i].Equals(""))
                        {
                            continue;
                        }
                        else
                        {
                            string[] splited_Data = splited[i].Split(',');
                            if (splited_Data.Length > 0)
                            {
                                if (splited_Data[0].Equals("<PTP>"))
                                {
                                    byte[] sendbuffer = new byte[1024];
                                    timeOffset = DateTimeOffset.Now;
                                    preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                                    string sendString = splited[i];
                                    string sendData = $"{sendString},{preUnixMilliseconds};";
                                    byte[] byteData = new byte[sendData.Length];
                                    byteData = Encoding.UTF8.GetBytes(sendData);
                                    client.GetStream().Write(byteData, 0, byteData.Length);
                                }
                                else if (splited_Data[0].Equals("#2"))
                                {
                                    Console.WriteLine("DATASEND");
                                    server_send = true;
                                    recorddata = true;
                                    streamOn();
                                    if (stream != null)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        MessageBox.Show("기기 스트림이 불안정합니다.\n다시 연결해주세요.");
                                        Console.WriteLine("STREAM END");
                                    }
                                }
                                else if (splited_Data[0].Equals("#3"))
                                {
                                    if (recorddata)
                                    {
                                        recorddata = false;
                                        server_send = false;
                                        if (rTh.ThreadState == System.Threading.ThreadState.Unstarted)
                                        {
                                            rTh.Start();
                                        }
                                        else if (rTh.ThreadState == System.Threading.ThreadState.Suspended)
                                        {
                                            rTh.Resume();
                                            //     SetDataText("Resume");
                                        }
                                        else if (rTh.ThreadState == System.Threading.ThreadState.Running)
                                        {
                                            while (rTh.ThreadState != System.Threading.ThreadState.Suspended)
                                            {
                                                Thread.Sleep(1);
                                            }
                                            rTh.Resume();
                                        }
                                    }
                                    if (stream != null)
                                    {
                                        stream.Close();
                                        stream = null;
                                    }
                                    DataTextBox.BeginInvoke(new Action(() => DataTextBox.Clear()));
                                }
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("서버와의 연결이 끊어졌습니다.", "Server Error");
                    ReceiveThread.Abort();
                }
            }

        }

    }

    #endregion

}

