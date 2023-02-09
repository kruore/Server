using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
namespace FileClient
{
    class client:MonoBehaviour
    {
        public void Start()
        {
            Main();
            Debug.Log("FileSended!");
        }
        private void MakeFolder()
        {

            var rootPath = Application.persistentDataPath + "/StreamingAssets/";
            var folderName = "CCW";
            var folder_Path = System.IO.Path.Combine(rootPath, folderName);
            Debug.Log("MakeFolderPath:" + folder_Path);
            if (!Directory.Exists(folder_Path))
            {
                Directory.CreateDirectory(folder_Path);
                Debug.Log("Created");
            }
        }

        // 실행 함수
        void Main()
        {
            MakeFolder();
            var rootPath = Application.persistentDataPath + "/StreamingAssets/CCW";
            var folderName = "jumpingaccvideofix.mp4";
            var folder_Path = System.IO.Path.Combine(rootPath, folderName);
            // 업로드 할 파일
            var filename = folder_Path;
            // 서버에 접속한다.
            var ipep = new IPEndPoint(IPAddress.Parse("210.94.216.195"), 4646);
            Debug.Log("Connected!");
            // FileInfo 생성
            var file = new FileInfo(folder_Path);
            Debug.Log(file.Exists);
            Debug.Log(file.Directory);
            // 파일이 존재하는지
            if (file.Exists)
            {
                Debug.Log("File Exists");
                // 바이너리 버퍼
                var binary = new byte[file.Length];
                // 파일 IO 생성
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    // 소켓 생성
                    using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        // 접속
                        client.Connect(ipep);
                        // 파일을 IO로 읽어온다.
                        stream.Read(binary, 0, binary.Length);
                        // 상태 0 - 파일 이름 크기를 보낸다.
                        client.Send(new byte[] { 0 });
                        // 송신 - 파일 이름 크기 Bigendian
                        client.Send(BitConverter.GetBytes(file.Name.Length));
                        // 상태 1 - 파일 이름 보낸다.
                        client.Send(new byte[] { 1 });
                        // 송신 - 파일 이름 
                        client.Send(Encoding.UTF8.GetBytes(file.Name));
                        // 상태 2 - 파일 크기를 보낸다.
                        client.Send(new byte[] { 2 });
                        // 송신 - 파일 크기 Bigendian
                        client.Send(BitConverter.GetBytes(binary.Length));
                        // 상태 3 - 파일를 보낸다.
                        client.Send(new byte[] { 3 });
                        // 송신 - 파일
                        client.Send(binary);
                    }
                }
            }
            else
            {
                // 콘솔 출력
                Console.WriteLine("The file is not exists.");
            }
            // 아무 키나 누르면 종료
            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }
    }
}