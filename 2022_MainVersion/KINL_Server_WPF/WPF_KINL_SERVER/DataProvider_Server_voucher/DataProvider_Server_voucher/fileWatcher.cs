using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace DataProvider_Server_voucher
{
    /// <summary>
    /// 파일의 변동을 눈치채고 만약에 상태가 변경된다면 해당 상태를 파악하는 프로그램
    /// </summary>
    public class fileWatcher
    {
        public static void watcher()
        {
            Console.WriteLine("Watcher ACTIVE");
            var watcher = new FileSystemWatcher(@"D:\GitHub\Server\2022_MainVersion\KINL_Server_WPF\WPF_KINL_SERVER\DataProvider_Server_voucher\DataProvider_Server_voucher\bin\Release\KPT_Server_DataFolder");

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.Filter = "*.txt";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        public static void watcher_sub()
        {
            Console.WriteLine("Watcher ACTIVE");
            var watcher = new FileSystemWatcher(@"D:\GitHub\Server\2022_MainVersion\KINL_Server_WPF\WPF_KINL_SERVER\DataProvider_Server_voucher\DataProvider_Server_voucher\bin\Release\KPT_Server_DataFolder");

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged_SubFile;
            watcher.Created += OnCreated_SubFile;
            watcher.Deleted += OnDeleted_SubFile;
            watcher.Renamed += OnRenamed_SubFile;
            watcher.Error += OnError;

            watcher.Filter = "*.txt";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed Name: {e.Name}");
            Console.WriteLine($"Changeds Path: {e.FullPath}");
            Exchanged_FilePath(e.FullPath, e.Name);
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }

        private static void Exchanged_FilePath(string tempFilePath, string tempFileName)
        {
          //  string tempFileNames = tempFileName.Substring(0,tempFilePath.Length-4) + ".csv";
            string dest_file = "D:\\PROJECT\\5_4_Server_code_test\\Package3\\Allout\\";
            string dest_fileName = System.IO.Path.Combine(dest_file, tempFileName);
            if (!Directory.Exists(dest_file))
            {
                Directory.CreateDirectory(dest_file);
            }
            Console.WriteLine("Dic:  " + Directory.Exists(dest_fileName).ToString());
            if (!Directory.Exists(dest_fileName))
            {
                File.Copy(tempFilePath, dest_fileName,true);
            }
        }

        public static void Clear()
        {
            string dest_file = "D:\\PROJECT\\5_4_Server_code_test\\Package3\\Allout\\";
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dest_file);
            // Delete this dir and all subdirs.
            try
            {
                di.Delete(true);
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }



        private static void OnChanged_SubFile(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed Name: {e.Name}");
            Console.WriteLine($"Changeds Path: {e.FullPath}");
        }

        private static void OnCreated_SubFile(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void OnDeleted_SubFile(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed_SubFile(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }
    }
}
