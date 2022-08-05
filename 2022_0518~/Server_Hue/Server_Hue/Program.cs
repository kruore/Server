using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace Server_Hue
{
    enum packTypes
    {
        applewatch = 1, airpot = 2, device = 3, connectPacket = 4 , ptp = 5
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

    class GameSession : PacketSession
    {
        PTPSession ptp_session = new PTPSession();
        DataRecorder dataRecorder = new DataRecorder();
        ConcurrentQueue<KeyValuePair<ushort, double>> ptpData = new ConcurrentQueue<KeyValuePair<ushort, double>>();
        ConcurrentDictionary<KeyValuePair<ushort, ushort>, ConcurrentQueue<string>> datas = new ConcurrentDictionary<KeyValuePair<ushort, ushort>, ConcurrentQueue<string>>();
        ConcurrentQueue<string> appleWatchData = new ConcurrentQueue<string>();
        ConcurrentQueue<string> airpodData = new ConcurrentQueue<string>();
        ConcurrentQueue<string> deviceData = new ConcurrentQueue<string>();
        int[] fileIndex = new int[3] { 0, 0, 0 };
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            ptp_session.PTP_Start(this);
            //PTP Start()
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            int len = 0;
            bool success = true;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
            len += 2;
            ushort packetID = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
            len += 2;
            ushort packetType = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
            KeyValuePair<ushort, ushort> checker = new KeyValuePair<ushort, ushort>(packetID, packetType);
            switch (packetType)
            {
                case ((int)packTypes.applewatch):
                    len += 2;
                    double time = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
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
                    appleWatchData.Enqueue($"{time.ToString("F0")},{gyrox.ToString("N3")},{gyroy.ToString("N3")},{gyroz.ToString("N3")},{accx.ToString("N3")},{accy.ToString("N3")},{accz.ToString("N3")},{heartRate}");
                    if (appleWatchData.Count > 300)
                    {
                        if (datas.TryAdd(checker, appleWatchData))
                        {
                            if (datas.TryGetValue(checker, out var value))
                            {
                                fileIndex[0]++;
                                dataRecorder.PrintData(value, fileIndex[0], packetID, packetType);
                                deviceData.Clear();
                                datas.TryRemove(checker, out value);
                            }
                        }
                    }
                    break;
                case ((int)packTypes.airpot):
                    len += 2;
                    double time_a = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
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
                    airpodData.Enqueue($"{time_a.ToString("F0")},{gyrox_a.ToString("N3")},{gyroy_a.ToString("N3")},{gyroz_a.ToString("N3")},{accx_a.ToString("N3")},{accy_a.ToString("N3")},{accz_a.ToString("N3")}");
                    if (airpodData.Count > 300)
                    {
                        if (datas.TryAdd(checker, airpodData))
                        {
                            if (datas.TryGetValue(checker, out var value))
                            {
                                fileIndex[1]++;
                                dataRecorder.PrintData(value, fileIndex[1], packetID, packetType);
                                deviceData.Clear();
                                datas.TryRemove(checker, out value);
                            }
                        }
                    }
                    break;
                case ((int)packTypes.device):
                    len += 2;
                    double time_d = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
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
                    deviceData.Enqueue($"{time_d.ToString("F0")},{gyrox_d.ToString("N3")},{gyroy_d.ToString("N3")},{gyroz_d.ToString("N3")},{accx_d.ToString("N3")},{accy_d.ToString("N3")},{accz_d.ToString("N3")}");
                    //    Console.WriteLine($"{time_d.ToString("N0")},{gyrox_d.ToString("N3")},{gyroy_d.ToString("N3")},{gyroz_d.ToString("N3")},{accx_d.ToString("N3")},{accy_d.ToString("N3")},{accz_d.ToString("N3")}");
                    //   Console.WriteLine(deviceData.Count);
                    if (deviceData.Count > 300)
                    {
                        if (datas.TryAdd(checker, deviceData))
                        {
                            if (datas.TryGetValue(checker, out var value))
                            {
                                fileIndex[2]++;
                                dataRecorder.PrintData(value, fileIndex[2], packetID, packetType);
                                deviceData.Clear();
                                datas.TryRemove(checker, out value);
                            }
                        }
                    }
                    break;
                case ((int)packTypes.connectPacket):
                    break;
                case ((int)packTypes.ptp):
                    //만약에 PTP를 진행한다면..
                    // SIZE, ID, PACKET_PTP,PTPCOUNT,DOUBLE,DOUBLE,DOUBLE,DOUBLE
                    double[] time_ptp = new double[4];
                    len += 2;
                    ushort count = BitConverter.ToUInt16(buffer.Array, buffer.Offset + len);
                    len += 8;
                    time_ptp[0] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
                    len += 8;
                    time_ptp[1] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
                    len += 8;
                    time_ptp[2] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);
                    len += 8;
                    time_ptp[3] = BitConverter.ToDouble(buffer.Array, buffer.Offset + len);

                    if (count == 4)
                    {
                        KeyValuePair<ushort, double> PTP_checker = new KeyValuePair<ushort,double>();
                        for (ushort i = 0; i < 4; i++)
                        {
                            PTP_checker = new KeyValuePair<ushort, double>(i, time_ptp[i]);
                            ptpData.Enqueue(PTP_checker);
                        }
                    }
                    break;
            }

        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Trasnferred bytes : {numOfBytes}");
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($" OnDisconnected {endPoint}");
        }


    }

    class program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {

            Socket socket;
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // IPAddress ipAddr = ipHost.AddressList[0];
            IPAddress iPAddr = IPAddress.Any;
            IPEndPoint ipEnd = new IPEndPoint(iPAddr, 4545);
            Console.WriteLine(ipEnd);

            listener.Init(ipEnd, () => { return new GameSession(); }, 10);

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
