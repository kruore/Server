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
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 IParam);

        private const int WM_SETREDRAW = 11;
        bool recorddata = false;
        public static Form1 inst;
        Stopwatch stopwatch;
        public string initialis, index, weight;

        public string clientNAME;
        TcpClient client = null;
        Thread ReceiveThread = null;
        NetworkStream netStream = null;

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
        public void Start()
        {
            //Console.WriteLine("Start");
            dataqueue = new Queue<string>();
            string[] port = SerialPort.GetPortNames();
            SetDataText(port.Length.ToString());
            for (int i = 0; i < port.Length; i++)
            {
                Console.WriteLine(port[i]);
            }
            //Console.WriteLine(port.Length);
            //if (stream != null)
            //{
            //    stream.Close();
            //}
            //stream = new SerialPort(port[0], 115200);
            //stream.Open();
            //Console.WriteLine("Open");
            timeOffset = DateTimeOffset.Now;
            rTh = new Thread(EnqueueData);
            rTh.IsBackground = false;
        }

        SerialPort stream;
        Queue<string> dataqueue;
        public string receivedstring;

        long preUnixMilliseconds;
        long UnixMilliseconds;
        DateTimeOffset timeOffset;
        public Thread rTh;
        public Thread rTh1;
        public Thread rTh2;
        bool easurement_timing_budget_check = false;
        private static AutoResetEvent are = new AutoResetEvent(false);
        /// <summary>
        /// Startbutton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton1_Click(object sender, EventArgs e)
        {

            initialis = DatainitialisText.Text;
            index = EquipIndexText.Text;
            weight = WeightText.Text;
            if (DatainitialisText.Text.Equals(String.Empty) || EquipIndexText.Text.Equals(String.Empty) || WeightText.Text.Equals(String.Empty))
            {
                DataTextBox.AppendText("Error:Null!");
                return;
            }
            else
            {
                DataTextBox.AppendText("initialis:" + initialis + "Index:" + index + "weight:" + weight);

            }
            if (dataqueue != null)
            {
                dataqueue.Clear();
            }
            string temp = initialis+"_"+index +"_"+weight;
            Login(temp);

            string[] port = SerialPort.GetPortNames();
            for (int i = 0; i < port.Length; i++)
            {
                Console.WriteLine(port[i]);
            }
            Console.WriteLine(port.Length);
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
                recorddata = true;
                rTh1 = new Thread(FIxedUpdate);
                rTh1.IsBackground = false;
                rTh1.Start();
            }
            else
            {
                SetDataText("Null Port");
            }
        }
        void EnqueueData()
        {
            while (true)
            {
                DataRecorder.instance.SetFileName();
                DataRecorder.instance.WriteSteamingData_Batch();
                rTh.Suspend();
            }
        }
        public void FIxedUpdate()
        {
            while (recorddata)
            {
                TimeBeginPeriod(1);
                stopwatch.Reset();
                stopwatch.Start();
                if (stream != null)
                {
                    DataUpdate();
                }
                stopwatch.Stop();
                //if (stopwatch.ElapsedMilliseconds < 40)
                //{
                //    Thread.SpinWait(40);
                //}
                //else
                //{
                //    Thread.SpinWait(1);
                //}
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
        public void DataUpdate()
        {
            try
            {
                receivedstring = stream.ReadLine();
                if (easurement_timing_budget_check)
                {
                    //receivedstring = DateTime.Now.ToString("yyyyMMddHHmmss.fff") + ",wow";
                    if (receivedstring != null)
                    {
                        //if (receivedstring.IndexOf(",DKU") > -1)
                        //{
                        //    if (DataRecorder.instance.machineName == null)
                        //    {
                        //        DataRecorder.instance.machineName = receivedstring.Substring(receivedstring.IndexOf("DKU"));
                        //        DataRecorder.instance.machineName = DataRecorder.instance.machineName.Remove(DataRecorder.instance.machineName.IndexOf("#"));
                        //        Console.WriteLine(DataRecorder.instance.machineName);
                        //    }
                        //    receivedstring = receivedstring.Remove(receivedstring.IndexOf(",DKU"), 8);
                        //}
                        receivedstring = receivedstring.Remove(0, 1);
                        //receivedstring=receivedstring.Substring(receivedstring.Length - 1);
                        //receivedstring += "\n";
                        timeOffset = DateTimeOffset.Now;
                        //Debug.Log(receivedstring);
                        UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                        DataRecorder.instance.Queue_ex_01.Enqueue(UnixMilliseconds.ToString() + "," + receivedstring);
                        receivedstring = receivedstring.Replace("\r", "");
                        byte[] buf = Encoding.Default.GetBytes($"DEVICE,{UnixMilliseconds.ToString()},4,{UnixMilliseconds.ToString()},{receivedstring};");
                        client.GetStream().Write(buf, 0, buf.Length);
                        string data = preUnixMilliseconds+"delay:" + (UnixMilliseconds - preUnixMilliseconds).ToString() + "count:" + DataRecorder.instance.Queue_ex_01.Count.ToString() + ",time" + DateTime.Now.ToString("HHmmss.fff") + ",data:" + receivedstring;
                        DataTextBox.Invoke(new Action(() => SetDataText(data)));
                        //if(DataTextBox.TextLength)
                        //Console.Write((UnixMilliseconds - preUnixMilliseconds).ToString() + "\n");
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
                        //EnqueueData();
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
        public void DataSending(string data)
        {

        }
        private void metroButton2_Click(object sender, EventArgs e)
        {
            if (recorddata)
            {
                recorddata = false;
                if (rTh.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    rTh.Start();
                }
                else if (rTh.ThreadState == System.Threading.ThreadState.Suspended)
                {
                    rTh.Resume();
                    SetDataText("Resume");
                }
                else if (rTh.ThreadState == System.Threading.ThreadState.Running)
                {
                    while (rTh.ThreadState != System.Threading.ThreadState.Suspended)
                    {
                        Thread.Sleep(1);
                    }
                    rTh.Resume();
                    SetDataText("RunningAndSuspended");
                }
            }
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            return;
        }
        public void SetDataText(string _data)
        {
            DataTextBox.AppendText(_data + "\n");
            //if (DataTextBox.Lines.Length > linenumber)
            //{
            //    LinkedList<string> tempLines = new LinkedList<string>(DataTextBox.Lines);

            //    while ((tempLines.Count - linenumber) > 0)
            //    {
            //        tempLines.RemoveFirst();
            //    }

            //    DataTextBox.Lines = tempLines.ToArray();
            //}
            //SendMessage(this.Handle, WM_SETREDRAW, true, 0);

            ////DataTextBox.SelectionStart = DataTextBox.Text.Length;
            ////DataTextBox.ScrollToCaret();
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

        private void Login(string a)
        {
            try
            {
                string parsedName = "%^&";
                parsedName += a;
                parsedName += "DEVICE;";

                clientNAME = parsedName;

                client = new TcpClient();
                //client.Connect("210.94.216.195", 4545);
                client.Connect("192.168.1.2", 4545);
                byte[] byteData = new byte[parsedName.Length];
                byteData = Encoding.UTF8.GetBytes(parsedName);
                client.GetStream().Write(byteData, 0, byteData.Length);
                ReceiveThread = new Thread(RecieveMessage);
                ReceiveThread.Start();
                MessageBox.Show("서버에 접속됨");
            }

            catch
            {
                //   MessageBox.Show("서버연결에 실패하였습니다.", "Server Error");
                client = null;
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

                    byte[] sendbuffer = new byte[1024];
                    timeOffset = DateTimeOffset.Now;
                    preUnixMilliseconds = UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
                    string sendData = $"{message},{preUnixMilliseconds};";
                    byte[] byteData = new byte[sendData.Length];
                    byteData = Encoding.UTF8.GetBytes(sendData);
                    client.GetStream().Write(byteData, 0, byteData.Length);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }



            //    while (true)
            //    {
            //        try
            //        {



            //            byte[] receiveByte = new byte[client.ReceiveBufferSize];
            //            client.GetStream().ReadAsync(receiveByte, 0, (int)client.ReceiveBufferSize);
            //            receiveMessage = Encoding.UTF8.GetString(receiveByte);

            //            Console.WriteLine(receiveMessage);
            //            string[] receiveMessageArray = receiveMessage.Split('>');
            //            foreach (var item in receiveMessageArray)
            //            {
            //                if (!item.Contains('<'))
            //                    continue;
            //                if (item.Contains("관리자<TEST"))
            //                    continue;

            //                receiveMessageList.Add(item);
            //            }

            //       //     ParsingReceiveMessage(receiveMessageList);
            //        }
            //        catch (Exception e)
            //        {
            //            //MessageBox.Show("서버와의 연결이 끊어졌습니다.", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //            //MessageBox.Show(e.Message);
            //            //MessageBox.Show(e.StackTrace);
            //            //Environment.Exit(1);
            //        }
            //       //Thread.Sleep(500);
            //    }
            //}

        }

    }
}

