using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
namespace DataProvider_Server_voucher
{
    public class GM_DataRecorder
    {

        /// <summary>
        ///  싱글톤 데이터 프로그램
        /// </summary>
        public static GM_DataRecorder instance = null;

        // 파일 세이브 관련 처리
        private string rootpath = string.Empty;
        private string mainfolder_Path = string.Empty;

        string UnixTimeMillisecondsTime = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        private string mainFolderName = "KPT_Server_DataFolder";

        // 다중 접속 스레드 관련 처리
        private string str_DataCategory = string.Empty;
        object _lock = new object();


        //데이터 관련 처리
        public Dictionary<string, Queue<string>> Queue_Device = new Dictionary<string, Queue<string>>();
        public Dictionary<string, Queue<string>> Queue_Airpod = new Dictionary<string, Queue<string>>();
        public Dictionary<string, Queue<string>> Queue_Watch = new Dictionary<string, Queue<string>>();

        /// <summary>
        /// 데이터 레코드 인스턴스
        /// </summary>
        public GM_DataRecorder()
        {
            if (instance == null)
            {
                instance = this;
            }
            mainfolder_Path = rootpath = Directory.GetCurrentDirectory();
            mainfolder_Path = MakeFolder(mainFolderName);
        }

        #region DataControll
        public void Enqueue_Data(string clientName, string _Queue)
        {
            lock (_lock)
            {
                Queue<string> list = new Queue<string>();
                if (!Queue_Device.ContainsKey(clientName))
                {
                    Queue_Device.Add(clientName, list);
                }
                Queue_Device[clientName].Enqueue(_Queue);
            }
        }
        public void Enqueue_Data_A(string clientName, string _Queue)
        {
            lock (_lock)
            {
                Queue<string> list = new Queue<string>();
                if (!Queue_Airpod.ContainsKey(clientName))
                {
                    Queue_Airpod.Add(clientName, list);
                }
                Queue_Airpod[clientName].Enqueue(_Queue);
            }
        }
        public void Enqueue_Data_W(string clientName, string _Queue)
        {
            lock (_lock)
            {
                Queue<string> list = new Queue<string>();
                if (!Queue_Watch.ContainsKey(clientName))
                {
                    Queue_Watch.Add(clientName, list);
                }
                Queue_Watch[clientName].Enqueue(_Queue);
            }
        }
        #endregion



        #region Data Batch End File Saved
        public bool WriteSteamingData_Batch_Device(string clientNumber, string clientName, string deviceName)
        {
            bool isCategoryPrinted_DV = false;
            bool tempb = false;

            try
            {
                string DateTimes = DateTime.Now.ToString("yyyyMMddHHmmss");
                isCategoryPrinted_DV = false;
                //    string tempFileName = $"{DateTimes}_{clientName}_DEVICE.txt";
                string tempFileName = $"{DateTimes}_07_6_05DEVICE_DEVICE.txt";
                string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);
                GM_DB.Instance.UpdateDataPath(clientName, DateTimes, tempFileName, file_Location);
                string m_str_DataCategory = string.Empty;
                List<string> weigthlist = new List<string>();
                Dictionary<int, int> counter = new Dictionary<int, int>();
                List<int> dataList = new List<int>();
                int totalCountoftheQueue = Queue_Device.Count;
                int largest = 0;

                //Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    while (Queue_Device[clientNumber].Count != 0)
                    {
                        string stringData = Queue_Device[clientNumber].Dequeue();
                        string[] splitData = stringData.Split(',');
                        weigthlist.Add(splitData[7]);

                        if (Queue_Device[clientNumber].Count == 0)
                        {
                            for (int i = 0; i < weigthlist.Count; i++)
                            {
                                if (weigthlist[i] == "0")
                                {
                                    weigthlist.RemoveAt(i);
                                }
                                else
                                {
                                    int data = int.Parse(weigthlist[i]);
                                    if (!counter.ContainsKey(data))
                                    {
                                        Console.WriteLine("key 추가됨:" + data);
                                        counter.Add(data, 0);
                                    }
                                    else
                                    {
                                        counter[data]++;
                                    }
                                }
                            }
                            if (counter.Count > 0)
                            {
                                foreach (KeyValuePair<int, int> largestData in counter.ToList())
                                {
                                    if (largest != 0)
                                    {
                                        if ((counter[largest]) < largestData.Value)
                                        {
                                            largest = largestData.Key;
                                        }
                                    }
                                    else
                                    {
                                        largest = largestData.Key;
                                    }
                                }
                            }
                            /// <param name="_idx">schema 명</param>
                            /// <param name="_datedata">날짜</param>
                            /// <param name="_weight">무게</param>
                            /// <param name="_count">횟수</param>
                            /// <param name="_time">운동시간</param>
                            /// <param name="_machineindex">머신의 index</param>
                            /// <param name="_exerciseclass">운동종류</param>
                            /// <param name="_mucleclass">운동에 쓰이는 근육</param>
                            Console.WriteLine("현재 무게는 "+largest);
                            GM_DB.Instance.UpdateDataset(clientName, splitData[1], int.Parse(splitData[7]), int.Parse(splitData[8]), 6);
                            GM_DB.Instance.UpdateMachineSet(clientName, deviceName, DateTimes, UnixTimeMillisecondsTime, largest, int.Parse(splitData[8]), 0);
                            Console.WriteLine("클라이언트 IOS 의 넘버는 : " + clientNumber);
                            if (Form1.ai_FeedData.ContainsKey(clientName))
                            {
                                Form1.ai_FeedData[clientName] = stringData + "," + largest + "," + UnixTimeMillisecondsTime + "," + deviceName;
                                Console.WriteLine("AI 분석 데이터:" + Form1.ai_FeedData);
                                Console.WriteLine("무게는 : " + largest);
                            }
                            else
                            {
                                Form1.ai_FeedData.Add(clientName, stringData + "," + largest + "," + UnixTimeMillisecondsTime + "," + deviceName);
                            }
                        }
                        if (stringData.Length > 0 && !isCategoryPrinted_DV)
                        {
                            str_DataCategory =
                               "DeviceName,"
                               + "PTPTime,"
                               + "UnixTime,"
                               + "protocool,"
                               + "CurrentDeviceTime,"
                               + "DistanceMM,"
                               + "DistanceCM,"
                               + "Weight,"
                               + "Count,"
                               + "DistanceADC,"
                               + "WeightADC"
                               + "DeviceName(Current)";
                            streamWriter.WriteLine(str_DataCategory);
                            isCategoryPrinted_DV = true;
                        }
                        streamWriter.WriteLine(stringData);

                    }
                    streamWriter.Close();
                }
                tempb = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(";;;;;;;;;;" + e);
            }
            return tempb;
        }
        public bool WriteSteamingData_Batch_Watch(string clientNumber, string clientName)
        {
            bool isCategoryPrinted_W = false;
            bool tempb = false;
           // string tempFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{clientName}_WATCH.txt";
            string tempFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_07_6_05DEVICE_WATCH.txt";
            try
            {
                isCategoryPrinted_W = false;
                string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = Queue_Watch[clientNumber].Count;

                //Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    while (Queue_Watch[clientNumber].Count != 0)
                    {
                        string stringData = Queue_Watch[clientNumber].Dequeue();

                        if (stringData.Length > 0)
                        {
                            if (!isCategoryPrinted_W)
                            {
                                str_DataCategory =
                                    "Device,"
                                    + "PTPTime,"
                                    + "UnixTime,"
                                    + "Protocool,"
                                    + "CurrentDeviceTime,"

                                    + "GyroX,"
                                    + "GyroY,"
                                    + "GyroZ,"
                                    + "AccX,"
                                    + "AccY,"
                                    + "AccZ,"
                                    + "HeartRate"
                                    ;
                                streamWriter.WriteLine(str_DataCategory);
                                isCategoryPrinted_W = true;
                            }
                            streamWriter.WriteLine(stringData);
                        }
                    }
                    streamWriter.Close();
                }
                tempb = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("1111111111" + e);
            }

            return tempb;
        }
        public bool WriteSteamingData_Batch_Airpod(string clientNumber, string clientName)
        {
            bool isCategoryPrinted_A = false;
            bool tempb = false;
            //string tempFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{clientName}_AIRPOD.txt";
            string tempFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_07_6_05DEVICE_AIRPOD.txt";
            try
            {
                isCategoryPrinted_A = false;
                string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = Queue_Airpod[clientNumber].Count;

                //Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    while (Queue_Airpod[clientNumber].Count != 0)
                    {

                        string stringData = Queue_Airpod[clientNumber].Dequeue();

                        if (stringData.Length > 0)
                        {
                            if (!isCategoryPrinted_A)
                            {
                                str_DataCategory =
                                    "Device,"
                                    + "PTPTime,"
                                    + "UnixTime,"
                                    + "Protocool,"
                                    + "CurrentDeviceTime,"
                                    + "GyroX,"
                                    + "GyroY,"
                                    + "GyroZ,"
                                    + "AccX,"
                                    + "AccY,"
                                    + "AccZ"
                                    ;
                                streamWriter.WriteLine(str_DataCategory);
                                isCategoryPrinted_A = true;
                            }
                            streamWriter.WriteLine(stringData);
                        }
                    }
                    streamWriter.Close();
                }
                tempb = true;

            }
            catch (Exception e)
            {
                Console.WriteLine("1111112222222222222" + e);
            }
            return tempb;
        }
        #endregion


        #region Program Controll

        private void OnDisable()
        {
            DisposeFileReader();
            //Directory.SetCurrentDirectory(rootpath);
        }

        #endregion

        // 스트림 라이터 프로그램 읽기


        #region Folder Utill

        //폴더 생성
        public string MakeFolder(string _WantedfolderName)
        {
            string finalFolderPath = string.Empty;

            finalFolderPath = System.IO.Path.Combine(mainfolder_Path, _WantedfolderName);
            // 만약 파일 패스에 존재하지 않는다면 디렉토리를 생성한다.
            Console.WriteLine(finalFolderPath);

            if (!Directory.Exists(finalFolderPath))
            {
                Directory.CreateDirectory(finalFolderPath);
            }
            return finalFolderPath;

        }
        #endregion



        private StreamReader fileReader = null;
        /// <summary>
        /// CSV READ
        /// </summary>
        /// <param name="_AccountName">카운터는 </param>
        public void Start_Load_csvData(string _AccountName)
        {
            string file_Location = System.IO.Path.Combine(mainfolder_Path, _AccountName);
            string[] fileNames = Directory.GetFiles(file_Location);
            fileReader = new StreamReader(fileNames[fileNames.Length - 1]);
        }

        private void DisposeFileReader()
        {
            if (fileReader != null)
            {
                fileReader.Dispose();
                fileReader = null;
            }
        }
    }
}