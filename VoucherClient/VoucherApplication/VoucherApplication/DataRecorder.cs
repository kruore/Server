using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
namespace VoucherApplication
{
    class DataRecorder
    {
        public static DataRecorder instance = null;

        private string rootpath = string.Empty;
        private string folder_Path = string.Empty;

        private string folderName = "KINLAB_DataLog";
        private string fileName = string.Empty;
        public string machineName = null;
        private string str_DataCategory = string.Empty;

        StringBuilder sb = new StringBuilder();


        private double startTime = 0.0f;
        private double currentTime = 0.0f;

        public enum Warning_Type
        {
            Ex_Start, Ex_End
        }

        public Queue<string> Queue_ex_01;

        private bool isCategoryPrinted;


        private bool bStartRecord = false;

        //-------------------------------

        public void Awake()
        {
            instance = this;
            MakeFolder();
            SetFileName();
            Start();
            Console.WriteLine("Awake");
            //rTh = new Thread(WriteSteamingData_Batch);
        }

        private void Start()
        {
            Queue_ex_01 = new Queue<string>();
            //startTime = currentTime = Time.time;
            startTime = currentTime = DateTime.Now.Ticks;


            bStartRecord = true;
            //StartCoroutine(Coru_Record_Data_60Hz());
        }



        private void MakeFolder()
        {
            rootpath = Directory.GetCurrentDirectory();

            folder_Path = System.IO.Path.Combine(rootpath, folderName);

            Directory.CreateDirectory(folder_Path);
        }

        public void SetFileName()
        {
            string fileNameFormat = string.Empty;
            fileName = "Voucher_"+ Form1.inst.index + "_" + Form1.inst.initialis + "_" +Form1.inst.weight+ "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public void Enequeue_Data(string _data)
        {
            //Debug.Log(DateTime.Now.Ticks.ToString());

            //currentTime = Time.time;
            currentTime = DateTime.Now.Ticks;

            //Debug.Log((currentTime - startTime).ToString());
            DateTimeOffset timeOffset = DateTimeOffset.Now;
            //Debug.Log(receivedstring);
            long UnixMilliseconds = timeOffset.ToUnixTimeMilliseconds();
            double timestemp = (currentTime - startTime) / 10000000.0d;

            string stringRelativeTime = string.Format("{0:F4}", timestemp);

            string refined_Data = UnixMilliseconds + "," + stringRelativeTime + "," + _data + ",";


            Queue_ex_01.Enqueue(refined_Data);

        }

        public void WriteSteamingData_Batch()
        {

            WriteSteamingData_Batch(ref Queue_ex_01);
        }

        public bool WriteSteamingData_Batch(ref Queue<string> _Queue_ex)
        {
            bool tempb = false;

            string tempFileName = machineName + "_" + fileName + ".txt";
            try
            {
                //string tempFileName = machineName + "_" + fileName + ".txt";
                string file_Location = System.IO.Path.Combine(folder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = _Queue_ex.Count;

                FileInfo fileInfo = new FileInfo(file_Location);
                if (fileInfo.Exists)
                {

                    return false;

                }
                using (StreamWriter streamWriter = File.CreateText(file_Location))
                {
                    streamWriter.Write("Unixtime,Distancemm,Distancecm,Weight,Count,DistanceADC,WeightADC,\n");
                    if (_Queue_ex.Count > 301)
                    {
                        for (int i = 0; i < 300; i++)
                        {
                            string stringData = _Queue_ex.Dequeue();

                            if (stringData.Length > 0)
                            {
                                if (!isCategoryPrinted)
                                {

                                    //streamWriter.WriteLine(str_DataCategory);
                                    isCategoryPrinted = true;
                                }
                                streamWriter.Write(stringData);
                                Console.WriteLine("icount:" + i + "queuecount" + _Queue_ex.Count + "::" + stringData);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; _Queue_ex.Count>0; i++)
                        {
                            string stringData = _Queue_ex.Dequeue();

                            if (stringData.Length > 0)
                            {
                                if (!isCategoryPrinted)
                                {

                                    //streamWriter.WriteLine(str_DataCategory);
                                    isCategoryPrinted = true;
                                }
                                streamWriter.Write(stringData);
                                Console.WriteLine("icount:" + i + "queuecount" + _Queue_ex.Count + "::" + stringData);
                            }
                        }
                    } 

                    streamWriter.Close();
                }

                tempb = true;
                Console.WriteLine("FileUpload" + fileName);
                //StartCoroutine(CheckSavingDataCompleted());
            }
            catch (Exception e)
            {
                Console.WriteLine("FileUploadFail: {0}", e);

                //TCPTestClient.instance.Send_Message_Index("\n" + "WriteSteamingData_BatchProcessing ERROR : " + e, 0);
            }

            // 업로드 할 파일
            //var filename = @"D:\nowonbuntistory.png";
            // 서버에 접속한다.
            //var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4545);
            //// FileInfo 생성
            //var file = new FileInfo(System.IO.Path.Combine(folder_Path, tempFileName));
            //// 파일이 존재하는지
            //if (file.Exists)
            //{
            //    // 바이너리 버퍼
            //    var binary = new byte[file.Length];
            //    // 파일 IO 생성
            //    using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            //    {
            //        // 소켓 생성
            //        using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            //        {
            //            try
            //            {
            //                // 접속
            //                client.Connect(ipep);
            //                // 파일을 IO로 읽어온다.
            //                stream.Read(binary, 0, binary.Length);
            //                // 상태 0 - 파일 이름 크기를 보낸다.
            //                client.Send(new byte[] { 0 });
            //                // 송신 - 파일 이름 크기 Bigendian
            //                client.Send(BitConverter.GetBytes(file.Name.Length));
            //                // 상태 1 - 파일 이름 보낸다.
            //                client.Send(new byte[] { 1 });
            //                // 송신 - 파일 이름
            //                client.Send(Encoding.UTF8.GetBytes(file.Name));
            //                // 상태 2 - 파일 크기를 보낸다.
            //                client.Send(new byte[] { 2 });
            //                // 송신 - 파일 크기 Bigendian
            //                client.Send(BitConverter.GetBytes(binary.Length));
            //                // 상태 3 - 파일를 보낸다.
            //                client.Send(new byte[] { 3 });
            //                // 송신 - 파일
            //                client.Send(binary);
            //            }
            //            catch (Exception e)
            //            {
            //                Console.WriteLine(e);
            //                return tempb;

            //            }
            //        }
            //    }
            //}
            return tempb;
        }



        public void Write_Warning(Warning_Type _warning, string _additionalMessage)
        {
            string warning_Message = string.Empty;

            switch (_warning)
            {
                case Warning_Type.Ex_Start:
                    warning_Message = "[Record Starts.:" + _additionalMessage + "]";
                    //TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " : " + warning_Message, 0);
                    break;

                case Warning_Type.Ex_End:
                    warning_Message = "[Record Stops.:]";
                    //TCPTestClient.instance.Send_Message_Index("\n" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + " : " + warning_Message, 0);
                    break;


            }


            Enequeue_Data(warning_Message);
        }
        virtual public void Record_Data(string _data)
        {

            sb.Clear();
            //sb.AppendFormat("{0:F4}", IMU_Sensor_0_Acc.x).Append(',');

            //sb.Append("||");

            if (sb.Length > 0 && sb[sb.Length - 1] == ',')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            Enequeue_Data(_data);

        }
        private void BubbleSort(ref List<int> _dataArray)
        {
            int temp_bs;

            for (int i = 0; i < (_dataArray.Count - 1); i++)
            {
                for (int j = 0; j < (_dataArray.Count - 1) - i; j++)
                {
                    if (_dataArray[j] > _dataArray[j + 1])
                    {
                        temp_bs = _dataArray[j];
                        _dataArray[j] = _dataArray[j + 1];
                        _dataArray[j + 1] = temp_bs;
                    }

                }
            }
        }

    }
}
