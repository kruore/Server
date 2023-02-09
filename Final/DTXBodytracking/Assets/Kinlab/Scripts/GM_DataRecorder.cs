using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using FileClient;


namespace _KINLAB
{
    public class GM_DataRecorder : MonoBehaviour
    {
        public static GM_DataRecorder instance = null;
        public static FileSendClient instance_FileSender = new FileSendClient();
        private bool isCategoryPrinted = false;

        private string rootpath = string.Empty;
        [SerializeField]
        private string folder_Path = string.Empty;
        [SerializeField]
        private string folderName = "KINLAB_TEST_DATA";

        private string str_DataCategory = string.Empty;

        public Dictionary<string, Queue<string>> dataDictionary;
        public Dictionary<string, string> dataCategory;
        public bool isRecording=false;
        static string StreamingAssetPathForReal()
        {
#if UNITY_EDITOR
        return Application.persistentDataPath + "/StreamingAssets/";
#elif UNITY_ANDROID
        return Application.persistentDataPath + "!/assets/";
#elif UNITY_IOS
        return Application.persistentDataPath + "/Raw/";
#endif
        }

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
                dataCategory = new Dictionary<string, string>();
                dataDictionary = new Dictionary<string, Queue<string>>();
            }
            else
            {
                Destroy(this.gameObject);
            }
            MakeFolder();
        }

        public void SetFile(string name,string category)
        {
            string checkstring = string.Empty;
            if (!dataCategory.TryGetValue(name, out checkstring))
            {
                dataDictionary.Add(name, new Queue<string>());
                dataCategory.Add(name, category);
            }
            else
            {
                Debug.LogError("dataCategory isn't null");
                //값이 존재함 에러!
            }
        }
        public void Enequeue_Data(string name, string _data)
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            double cur_time = (System.DateTime.UtcNow - epochStart).TotalMilliseconds;

            string stringRelativeTime = string.Format("{0:F3}", Time.time);
            string refined_Data = cur_time.ToString() + "," + stringRelativeTime.ToString() + "," + "," + "," + _data;
            dataDictionary[name].Enqueue(refined_Data);
        }
        public void DataClear(string name)
        {
            dataDictionary[name].Clear();
        }
        public void Enqueue_Data_Log(string name, string event_Log_OneShot)
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            double cur_time = (System.DateTime.UtcNow - epochStart).TotalMilliseconds;

            string stringRelativeTime = string.Format("{0:F3}", Time.time);
            string refined_Data = cur_time.ToString() + "," + stringRelativeTime.ToString() + "," + "," + event_Log_OneShot + ",";
            dataDictionary[name].Enqueue(refined_Data);
        }
        public bool SaveData(string name,BodyRecorder bodyRecorder)
        {
            bool tempb = false;
            try
            {
                
                bodyRecorder.SaveVideo();
                string tempFileName = name + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";
                string file_Location = System.IO.Path.Combine(folder_Path, tempFileName);

                string m_str_DataCategory = string.Empty;

                int totalCountoftheQueue = dataDictionary[name].Count;

                Debug.Log("Saving Data Starts. IOS_LidarSkeleton_ Queue Count : " + totalCountoftheQueue);

                using (StreamWriter streamWriter = File.AppendText(file_Location))
                {
                    streamWriter.WriteLine(dataCategory[name]);
                    while (dataDictionary[name].Count != 0)
                    {
                        for (int i = 0; i < totalCountoftheQueue; i++)
                        {
                            string stringData = dataDictionary[name].Dequeue();

                            if (stringData.Length > 0)
                            {
                                streamWriter.WriteLine(stringData);
                            }
                        }
                    }
                    streamWriter.Close();

                }
                tempb = true;
                instance_FileSender.StartFileSend(file_Location);
    
            }
            catch (Exception e)
            {
                Debug.Log("WriteSteamingData_BatchProcessing ERROR : " + e);
            }
            return tempb;
        }

        private void MakeFolder()
        {
            rootpath = StreamingAssetPathForReal();
#if UNITY_EDITOR
            folder_Path = System.IO.Path.Combine(rootpath, folderName);
#endif
#if UNITY_IOS

            folder_Path = Path.Combine(rootpath, folderName);
#endif
            Debug.Log("MakeFolderPath:"+folder_Path);
            if (!Directory.Exists(folder_Path))
            {
                Directory.CreateDirectory(folder_Path);
                Debug.Log("Created");
            }
        }
        public void SetFolderName(string _folderName)
        {
            folderName = _folderName;
        }
        public void Save(string name)
        {
            BodyRecorder bodyRecorder = gameObject.GetComponent<BodyRecorder>();
            SaveData(name,bodyRecorder);
        }
        public string GetFolderPath()
        {
            return folder_Path;
        }
    }

}
