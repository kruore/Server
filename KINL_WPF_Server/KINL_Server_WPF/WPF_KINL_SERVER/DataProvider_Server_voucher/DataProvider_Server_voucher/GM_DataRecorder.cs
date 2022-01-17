using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
public class GM_DataRecorder
{
    public static GM_DataRecorder instance = null;

    private string rootpath = string.Empty;
    private string mainfolder_Path = string.Empty;

    private string mainFolderName = "KPT_Server_DataFolder";

    private string str_DataCategory = string.Empty;
    object _lock = new object();

    public Dictionary<int, Queue<string>> Queue_Device = new Dictionary<int, Queue<string>>();
    public Queue<string> Queue_AirPot = new Queue<string>();
    public Queue<string> Queue_Watch = new Queue<string>();

    public GM_DataRecorder()
    {
        if (instance == null)
        {
            instance = this;
        }
        mainfolder_Path = rootpath = Directory.GetCurrentDirectory();
        mainfolder_Path = MakeFolder(mainFolderName);
    }
    public void Enqueue_Data(int clientNumber, string _Queue)
    {
        lock (_lock)
        {
            if (!Queue_Device.ContainsKey(clientNumber))
            {
                Queue<string> a = new Queue<string>();
                Queue_Device.Add(clientNumber, a);
            }
            Queue_Device[clientNumber].Enqueue(_Queue);
        }
    }
    public void Enqueue_Data_A(string _Queue)
    {
        lock (_lock)
        {
            Queue_AirPot.Enqueue(_Queue);
        }
    }
    public void Enqueue_Data_W(string _Queue)
    {
        lock (_lock)
        {
            Queue_Watch.Enqueue(_Queue);
        }
    }
    bool isCategoryPrinted_SW = false;
    public bool WriteSteamingData_Batch_Device(int clientNumber, string clientName)
    {
        bool tempb = false;

        try
        {
            string tempFileName = clientName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);

            string m_str_DataCategory = string.Empty;

            int totalCountoftheQueue = Queue_Device[clientNumber].Count;
            Console.WriteLine("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

            using (StreamWriter streamWriter = File.AppendText(file_Location))
            {
                while (Queue_Device.Count != 0)
                {
                    for (int i = 0; i < totalCountoftheQueue; i++)
                    {
                        string stringData = Queue_Device[clientNumber].Dequeue();

                        if (stringData.Length > 0)
                        {
                            Console.WriteLine(stringData.Length);
                            if (!isCategoryPrinted_SW)
                            {
                                Console.WriteLine(isCategoryPrinted_SW);
                                str_DataCategory =
                                    "DeviceName,realTimeStamp,PTPtimeStamp,Data";
                                streamWriter.WriteLine(str_DataCategory);
                                isCategoryPrinted_SW = true;
                            }
                            streamWriter.WriteLine(stringData);
                        }
                    }
                }
            }
            tempb = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return tempb;
    }
    public bool WriteSteamingData_Batch_Watch()
    {
        bool tempb = false;

        try
        {
            string tempFileName = "WATCH" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);

            string m_str_DataCategory = string.Empty;

            int totalCountoftheQueue = Queue_Watch.Count;

            //Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

            using (StreamWriter streamWriter = File.AppendText(file_Location))
            {
                while (Queue_Watch.Count != 0)
                {
                    for (int i = 0; i < totalCountoftheQueue; i++)
                    {
                        string stringData = Queue_Watch.Dequeue();

                        if (stringData.Length > 0)
                        {
                            if (!isCategoryPrinted_SW)
                            {
                                str_DataCategory =
                                    "TimeStamp,"
                                    + "Data";
                                streamWriter.WriteLine(str_DataCategory);
                                isCategoryPrinted_SW = true;
                            }
                            streamWriter.WriteLine(stringData);
                        }
                    }
                }
            }
            tempb = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return tempb;
    }
    public bool WriteSteamingData_Batch_AirPot()
    {
        bool tempb = false;

        try
        {
            string tempFileName = "AIRPOT" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            string file_Location = System.IO.Path.Combine(mainfolder_Path, tempFileName);

            string m_str_DataCategory = string.Empty;

            int totalCountoftheQueue = Queue_AirPot.Count;

            //Debug.Log("Saving Data Starts. Queue Count : " + totalCountoftheQueue);

            using (StreamWriter streamWriter = File.AppendText(file_Location))
            {
                while (Queue_AirPot.Count != 0)
                {
                    for (int i = 0; i < totalCountoftheQueue; i++)
                    {
                        string stringData = Queue_AirPot.Dequeue();

                        if (stringData.Length > 0)
                        {
                            if (!isCategoryPrinted_SW)
                            {
                                str_DataCategory =
                                    "TimeStamp,"
                                    + "Data";
                                streamWriter.WriteLine(str_DataCategory);
                                isCategoryPrinted_SW = true;
                            }
                            streamWriter.WriteLine(stringData);
                        }
                    }
                }
            }
            tempb = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return tempb;
    }
    private void OnDisable()
    {
        DisposeFileReader();
        //Directory.SetCurrentDirectory(rootpath);
    }
    private StreamReader fileReader = null;

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
}