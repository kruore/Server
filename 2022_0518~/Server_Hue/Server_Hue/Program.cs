using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Collections.Concurrent;

namespace Server_Hue
{
    enum packTypes
    {
        AI_report = 0, applewatch = 1, airpot = 2, device = 3, connectPacket = 101, ptp = 5, AI = 6, DummyData = 7
    }
    class PTP_Packet
    {
        public ushort size;
        public ushort packetId;
        public ushort packetType;
        public ushort counts;
        public double time00;
        public double time01;
        public double time02;
        public double time03;
    }
    class Login_Packet
    {
        public ushort size;
        public ushort packetId;
        public ushort packetType;
    }

    class FeedBack_Packet
    {
        public ushort size;
        public ushort packetId;
        public ushort packetType;
        public ushort feedBack;
    }

    class GameSession : PacketSession
    {
        PTPSession ptp_session = new PTPSession();
        DataRecorder dataRecorder = new DataRecorder();
        double sessionDelay = 0;
        bool isFinish = false;
        ConcurrentQueue<double[]> ptpData = new ConcurrentQueue<double[]>();
        ConcurrentDictionary<KeyValuePair<ushort, ushort>, ConcurrentQueue<string>> datas = new ConcurrentDictionary<KeyValuePair<ushort, ushort>, ConcurrentQueue<string>>();
        ConcurrentQueue<string> appleWatchData = new ConcurrentQueue<string>();
        ConcurrentQueue<string> airpodData = new ConcurrentQueue<string>();
        ConcurrentQueue<string> deviceData = new ConcurrentQueue<string>();
        System.Timers.Timer timer;

        public ushort SessionID { get; set; }
        public GameRoom Room { get; set; }

        int[] fileIndex = new int[3] { 0, 0, 0 };
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            string ip = ((IPEndPoint)(endPoint)).Address.ToString();
            Thread.Sleep(10);
            //  Room.Enter(this, SessionID);
            if (ip != "127.0.0.1")
            {
                while (ptpData.Count < 20)
                {
                    Thread.Sleep(10);
                    ptp_session.PTP_Start(this);
                }
                sessionDelay = ptp_session.CalculatePTP(this, ptpData) - 20;
                isFinish = true;
                SessionID = 1;
                //Room.FeedBack(0, 22);
                Console.WriteLine($"PTP FINISH : DELAY  = {sessionDelay}");
            }
            else if (ip == "127.0.0.1")
            {
                SessionID = 0;
            }
            ptp_session.LoginPacket(this);
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            int len = 0;
            bool success = true;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
            len += 2;
            ushort packetID = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);

            switch (packetID)
            {
                case 1:
                    len += 2;
                    ushort packetType = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
                    KeyValuePair<ushort, ushort> checker = new KeyValuePair<ushort, ushort>(packetID, packetType);
                    switch (packetType)
                    {
                        case ((int)packTypes.applewatch):
                            len += 2;
                            double time = BitConverter.ToDouble(buffer.Array, buffer.Offset + len) - sessionDelay;
                            len += 8;
                            float gyrox = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float gyroy = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float gyroz = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accx = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accy = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accz = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            Int32 heartRate = BitConverter.ToInt16(buffer.Array, buffer.Offset + len);
                            if (isFinish)
                            {
                                appleWatchData.Enqueue($"{time.ToString("F0")},{gyrox.ToString("N3")},{gyroy.ToString("N3")},{gyroz.ToString("N3")},{accx.ToString("N3")},{accy.ToString("N3")},{accz.ToString("N3")},{heartRate}");
                                //if (appleWatchData.Count > 300)
                                //{
                                //    if (datas.TryAdd(checker, appleWatchData))
                                //    {
                                //        if (datas.TryGetValue(checker, out var value))
                                //        {
                                //            fileIndex[0]++;
                                //            dataRecorder.PrintData(value, fileIndex[0], packetID, packetType);
                                //            appleWatchData.Clear();
                                //            datas.TryRemove(checker, out value);
                                //        }
                                //    }
                                //}
                            }
                            break;
                        case ((int)packTypes.airpot):
                            len += 2;
                            double time_a = BitConverter.ToDouble(buffer.Array, buffer.Offset + len) - sessionDelay;
                            len += 8;
                            float gyrox_a = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float gyroy_a = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float gyroz_a = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accx_a = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accy_a = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accz_a = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            if (isFinish)
                            {
                                airpodData.Enqueue($"{time_a.ToString("F0")},{gyrox_a.ToString("N3")},{gyroy_a.ToString("N3")},{gyroz_a.ToString("N3")},{accx_a.ToString("N3")},{accy_a.ToString("N3")},{accz_a.ToString("N3")}");
                                //if (airpodData.Count > 50)
                                //{
                                //    if (datas.TryAdd(checker, airpodData))
                                //    {
                                //        if (datas.TryGetValue(checker, out var value))
                                //        {
                                //            fileIndex[1]++;
                                //            dataRecorder.PrintData(value, fileIndex[1], packetID, packetType);
                                //            airpodData.Clear();
                                //            datas.TryRemove(checker, out value);
                                //            Room.FeedBack(0, 101);
                                //        }
                                //    }
                                //}
                            }
                            break;
                        case ((int)packTypes.device):
                            len += 2;
                            double time_d = BitConverter.ToDouble(buffer.Array, buffer.Offset + len) - sessionDelay;
                            len += 8;
                            float gyrox_d = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float gyroy_d = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float gyroz_d = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accx_d = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accy_d = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            len += 4;
                            float accz_d = BitConverter.ToSingle(buffer.Array, buffer.Offset + len);
                            if (isFinish)
                            {
                                deviceData.Enqueue($"{time_d.ToString("F0")},{gyrox_d.ToString("N3")},{gyroy_d.ToString("N3")},{gyroz_d.ToString("N3")},{accx_d.ToString("N3")},{accy_d.ToString("N3")},{accz_d.ToString("N3")}");
                                //    if (deviceData.Count > 300)
                                //    {
                                //        if (datas.TryAdd(checker, deviceData))
                                //        {
                                //            if (datas.TryGetValue(checker, out var value))
                                //            {
                                //                fileIndex[2]++;
                                //                dataRecorder.PrintData(value, fileIndex[2], packetID, packetType);
                                //                deviceData.Clear();
                                //                datas.TryRemove(checker, out value);
                                //                Random i = new Random();
                                //                Program.Room.FeedBack(1,(ushort)i.Next(4));
                                //            }
                                //        }
                                //    }
                            }
                            break;
                        case ((int)packTypes.connectPacket):
                            Console.WriteLine("IPhone Connect");
                            Program.Room.Enter(this, 1);
                            timer = new System.Timers.Timer(3000);
                            timer.Elapsed += OnTimedEvent;
                            timer.AutoReset = true;
                            timer.Enabled = true;
                            break;
                        case ((int)packTypes.ptp):
                            //만약에 PTP를 진행한다면..
                            // SIZE, ID, PACKET_PTP,PTPCOUNT,DOUBLE,DOUBLE,DOUBLE,DOUBLE
                            double[] time_ptp = new double[4];
                            len += 2;
                            ushort count = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
                            len += 2;
                            time_ptp[0] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
                            len += 8;
                            time_ptp[1] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
                            len += 8;
                            time_ptp[2] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
                            len += 8;
                            time_ptp[3] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
                            len += 8;
                            if (count == 4)
                            {
                                ptpData.Enqueue(time_ptp);
                            }
                            else
                            {
                                ptp_session.PTP_Recv(this, time_ptp[0], time_ptp[1]);
                            }
                            break;
                        case ((int)packTypes.AI):
                            break;
                        case ((int)packTypes.AI_report):
                            {
                                //          Room.FeedBack(0, 22);
                                len += 2;
                                ushort report = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
                                Console.WriteLine($"AI RETURN{report}");
                                //SessionManager.instance.Find(0).Send();
                            }
                            break;
                        case ((int)packTypes.DummyData):
                            {
                                len += 2;
                                ushort report = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
                                Room.FeedBack(1, report);
                                Console.WriteLine($"AI RETURN{report}");
                                //SessionManager.instance.Find(0).Send();


                            }
                            break;
                    }
                    break;


                case 0:
                    Console.WriteLine("AI REPORT");
                    len += 2;
                    ushort packetType_AI = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
                    switch (packetType_AI)
                    {
                        case ((int)packTypes.connectPacket):
                            Console.WriteLine("AI Connect");
                            len += 2;
                            ushort feedback = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
                            Program.Room.Enter(this, 0);
                            Program.Room.FeedBack(1, feedback);
                            //  Thread.Sleep(10);
                            //   Room.FeedBack(packetID, 0);
                            break;
                        case (int)packTypes.AI_report:
                            Console.WriteLine("USER _ FEEDBACK SEND");
                            break;
                    }
                    break;
            }

        }
        public override void OnSend(int numOfBytes)
        {
            //SessionManager.instance.Find(SessionID);
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.instance.Remove(this);
            if (Room != null)
            {
                Room.Leave(this);
                Room = null;
                Console.WriteLine($"방탈출 {SessionID}");
                timer.Dispose();
            }
            Console.WriteLine($" OnDisconnected {endPoint}");
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            OnRequestSaveAndFeedBack(1);
            Console.WriteLine("");
        }
        private void OnRequestSaveAndFeedBack(ushort packetID)
        {
            for (ushort i = 1; i < 4; i++)
            {
                fileIndex[i - 1]++;
                switch (i)
                {
                    case 1:
                        dataRecorder.PrintData(appleWatchData, packetID, i);
                        appleWatchData.Clear();
                        break;
                    case 2:
                        dataRecorder.PrintData(airpodData, packetID, i);
                        airpodData.Clear();
                        break;
                    case 3:
                        dataRecorder.PrintData(deviceData, packetID, i);
                        deviceData.Clear();
                        break;
                }
            }
            Program.Room.FeedBack(0, (ushort)101);
        }
    }
    class Program
    {
        static Listener listener = new Listener();
        public static GameRoom Room = new GameRoom();
        public static EndPoint ipPoint;
        static void Main(string[] args)
        {

            Socket socket;
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // IPAddress ipAddr = ipHost.AddressList[0];
            IPAddress iPAddr = IPAddress.Any;
            IPEndPoint ipEnd = new IPEndPoint(iPAddr, 4545);
            Console.WriteLine(ipEnd);
            ipPoint = ipEnd;

            listener.Init(ipEnd, () => { return SessionManager.instance.Generate(); });

            try
            {
                Console.WriteLine("Listening...");
                while (true)
                {
                    ;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }

        }


    }
}
