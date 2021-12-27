using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace KINL_Server_WPF
{
    public class KINL_Server
    {
        ClientManager _clientManager = null;
        ConcurrentBag<string> chattingLog = null;
        ConcurrentBag<string> AccessLog = null;
        Thread conntectCheckThread = null;

        public bool isOpened = true;

        public KINL_Server()
        {
            // 생성자에서는 클라이언트매니저의 객체를 생성,
            // 채팅로그와 접근로그를 담을 컬렉션 생성
            // 서버 스레드 및 하트비트 스레드 시작

            _clientManager = new ClientManager();
            chattingLog = new ConcurrentBag<string>();
            AccessLog = new ConcurrentBag<string>();
            _clientManager.EventHandler += ClientEvent;
            _clientManager.messageParsingAction += MessageParsing;
            Task serverStart = Task.Run(() =>
            {
                ServerRun();
            });
            

            conntectCheckThread = new Thread(ConnectCheckLoop);
            conntectCheckThread.Start();

        }

        // 하트비트 스레드
        private void ConnectCheckLoop()
        {
            while (true)
            {
                foreach (var item in ClientManager.clientDic)
                {
                    try
                    {
                        string sendStringData = "관리자<TEST>";
                        byte[] sendByteData = new byte[sendStringData.Length];
                        sendByteData = Encoding.Default.GetBytes(sendStringData);

                        item.Value._client.GetStream().Write(sendByteData, 0, sendByteData.Length);
                    }
                    catch (Exception e)
                    {
                        RemoveClient(item.Value);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        // 클라이언트의 접속종료가 감지됐을때 static 예약어로 저장된 clientDic에서
        // 해당클라이언트를 제거하고, 로그를 남깁니다.
        private void RemoveClient(ClientData targetClient)
        {
            ClientData result = null;
            ClientManager.clientDic.TryRemove(targetClient._clientNumber, out result);
            string leaveLog = string.Format("[{0}] {1} Leave Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result._clientUserName);
            AccessLog.Add(leaveLog);
        }

        // 클라이언트에게 메시지를 보내는 첫번째 과정입니다.
        private void MessageParsing(string sender, string message)
        {
            List<string> msgList = new List<string>();

            string[] msgArray = message.Split('>');
            foreach (var item in msgArray)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                msgList.Add(item);
            }
            SendMsgToClient(msgList, sender);

        }

        // 클라이언트에게 메시지를 보내는 두번째 과정입니다.
        private void SendMsgToClient(List<string> msgList, string sender)
        {
            string LogMessage = "";
            string parsedMessage = "";
            string receiver = "";

            int senderNumber = -1;
            int receiverNumber = -1;

            foreach (var item in msgList)
            {
                string[] splitedMsg = item.Split('<');

                receiver = splitedMsg[0];
                parsedMessage = string.Format("{0}<{1}>", sender, splitedMsg[1]);

                senderNumber = GetClinetNumber(sender);
                receiverNumber = GetClinetNumber(receiver);

                if (senderNumber == -1 || receiverNumber == -1)
                {
                    return;
                }

                if (parsedMessage.Contains("<GiveMeUserList>"))
                {
                    string userListStringData = "관리자<";
                    foreach (var el in ClientManager.clientDic)
                    {
                        userListStringData += string.Format("${0}", el.Value._clientUserName);
                    }
                    userListStringData += ">";
                    byte[] userListByteData = new byte[userListStringData.Length];
                    userListByteData = Encoding.Default.GetBytes(userListStringData);
                    ClientManager.clientDic[receiverNumber]._client.GetStream().Write(userListByteData, 0, userListByteData.Length);
                    return;
                }



                LogMessage = string.Format(@"[{0}] [{1}] -> [{2}] , {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, receiver, splitedMsg[1]);

                ClientEvent(LogMessage, StaticDefine.ADD_CHATTING_LOG);

                byte[] sendByteData = Encoding.Default.GetBytes(parsedMessage);

                ClientManager.clientDic[receiverNumber]._client.GetStream().Write(sendByteData, 0, sendByteData.Length);
            }
        }

        // 클라이언트의 이름을 통해 ClientNumber를 얻는 메서드입니다.
        // ClientDictionary는 ClientNumber를 키로 사용하고있어서
        // 클라이언트의 번호를 통해 클라이언트 객체를 반환받을 수 있습니다.
        private int GetClinetNumber(string targetClientName)
        {
            foreach (var item in ClientManager.clientDic)
            {
                if (item.Value._clientUserName == targetClientName)
                {
                    return item.Value._clientNumber;
                }
            }
            return -1;
        }

        // 접근로그와 채팅로그를 저장하는 메서드입니다.
        // StaticDefine은 제가만든 클래스로 정적변수에 번호를 지정해놨습니다.
        private void ClientEvent(string message, int key)
        {
            switch (key)
            {
                //case StaticDefine.ADD_ACCESS_LOG:
                //    {
                //        AccessLog.Add(message);
                //        break;
                //    }
                case StaticDefine.ADD_CHATTING_LOG:
                    {
                        chattingLog.Add(message);
                        break;
                    }
            }
        }

        // 서버를 돌리는 과정입니다.
        public void ServerRun()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 4545));
            listener.Start();

            while (true)
            {
                Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();

                acceptTask.Wait();

                TcpClient newClient = acceptTask.Result;

                _clientManager.AddClient(newClient);
            }
        }
        public void ServerClose()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 4545));
            try
            {
                listener.Server.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Console.WriteLine( "현재 연결된 클라이언트 없음" );
            }
            finally
            {
                listener.Server.Close(0);
                listener.Stop();
            }

        }


        // 기본적으로 서버가 돌아가는 콘솔 로직입니다.
        public void ConSoleVIew()
        {
            while (true)
            {
                Console.WriteLine("=============서버=============");
                Console.WriteLine("1.현재접속인원확인");
                Console.WriteLine("2.접속기록확인");
                Console.WriteLine("3.채팅로그확인");
                Console.WriteLine("0.종료");
                Console.WriteLine("==============================");

                string key = Console.ReadLine();
                int order = 0;


                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        default:
                            {
                                Console.WriteLine("잘못 입력하셨습니다.");
                                break;
                            }
                    }
                }

                else
                {
                    Console.WriteLine("잘못 입력하셨습니다.");
                }
               // Console.Clear();
                Thread.Sleep(50);
            }
        }

        // 채팅로그확인
        private void ShowCattingLog()
        {
            if (chattingLog.Count == 0)
            {
                Console.WriteLine("채팅기록이 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in chattingLog)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        // 접근로그확인
        private void ShowAccessLog()
        {
            if (AccessLog.Count == 0)
            {
                Console.WriteLine("접속기록이 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in AccessLog)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        // 현재접속유저확인
        private void ShowCurrentClient()
        {
            if (ClientManager.clientDic.Count == 0)
            {
                Console.WriteLine("접속자가 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in ClientManager.clientDic)
            {
                Console.WriteLine(item.Value._clientUserName);
            }
            Console.ReadKey();
        }
    }
}
