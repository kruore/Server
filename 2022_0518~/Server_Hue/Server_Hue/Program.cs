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
        applewatch = 1, airpot = 2, device = 3, connectPacket, ptp
    }

    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : PacketSession
    {
        DataRecorder dataRecorder = new DataRecorder();

        ConcurrentDictionary<KeyValuePair<ushort, ushort>, ConcurrentQueue<string>> datas = new ConcurrentDictionary<KeyValuePair<ushort, ushort>, ConcurrentQueue<string>>();
        ConcurrentQueue<string> appleWatchData = new ConcurrentQueue<string>();
        ConcurrentQueue<string> airpodData = new ConcurrentQueue<string>();
        ConcurrentQueue<string> deviceData = new ConcurrentQueue<string>();
        int count = 0;
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
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
                    appleWatchData.Enqueue($"{time.ToString("N0")},{gyrox.ToString("N3")},{gyroy.ToString("N3")},{gyroz.ToString("N3")},{accx.ToString("N3")},{accy.ToString("N3")},{accz.ToString("N3")},{heartRate}");
                    if (appleWatchData.Count > 300)
                    {
                        if (datas.TryAdd(checker, appleWatchData))
                        {
                            if (datas.TryGetValue(checker, out var value))
                            {
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
                    airpodData.Enqueue($"{time_a.ToString("N0")},{gyrox_a.ToString("N3")},{gyroy_a.ToString("N3")},{gyroz_a.ToString("N3")},{accx_a.ToString("N3")},{accy_a.ToString("N3")},{accz_a.ToString("N3")}");
                    if (airpodData.Count > 300)
                    {
                        if (datas.TryAdd(checker, airpodData))
                        {
                            if (datas.TryGetValue(checker, out var value))
                            {
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
                    deviceData.Enqueue($"{time_d.ToString("N0")},{gyrox_d.ToString("N3")},{gyroy_d.ToString("N3")},{gyroz_d.ToString("N3")},{accx_d.ToString("N3")},{accy_d.ToString("N3")},{accz_d.ToString("N3")}");
                    //    Console.WriteLine($"{time_d.ToString("N0")},{gyrox_d.ToString("N3")},{gyroy_d.ToString("N3")},{gyroz_d.ToString("N3")},{accx_d.ToString("N3")},{accy_d.ToString("N3")},{accz_d.ToString("N3")}");
                    //   Console.WriteLine(deviceData.Count);
                    if (deviceData.Count > 300)
                    {
                        if (datas.TryAdd(checker, deviceData))
                        {
                            if (datas.TryGetValue(checker, out var value))
                            {
                                count++;
                                dataRecorder.PrintData(value,count);
                                deviceData.Clear();
                                datas.TryRemove(checker, out value);
                            }
                        }
                    }
                    break;
                case ((int)packTypes.connectPacket):
                    break;
                case ((int)packTypes.ptp):
                    break;
            }

        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Trasnferred bytes : {numOfBytes}");
        }
        //public override int OnRecv(ArraySegment<byte> buffer)
        //{
        //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        //   // double data = BitConverter.GetBytes(buffer.Array,0);
        //    Console.WriteLine($"[From Client]{recvData}");
        //  //  Console.WriteLine($"BUFFER LENGTH {data}");
        //    return buffer.Count;
        //}
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
