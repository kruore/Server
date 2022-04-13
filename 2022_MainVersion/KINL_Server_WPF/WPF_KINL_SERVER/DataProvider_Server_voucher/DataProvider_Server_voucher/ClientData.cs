using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace DataProvider_Server_voucher
{
    class ClientData
    {
        public static bool isdebug = false;
        public TcpClient tcpClient { get; set; }
        public Byte[] readBuffer { get; set; }
        public StringBuilder currentMsg { get; set; }
        public string clientName { get; set; }
        public int clientNumber { get; set; }
        public enum ClientType { User, Device, IPhone, Watch};
        public ClientType clientType { get; set; }
        public bool isSend { get; set; }

        /// <summary>
        /// TCP Client 를 통한 데이터 획득 및 아이디 넘버링 객체 구현
        /// </summary>
        /// <param name="tcpClient">해당 class를 통한 clientID 구현 </param>

        public ClientData(TcpClient tcpClient)
        {
            currentMsg = new StringBuilder();
            readBuffer = new byte[1024];
            this.isSend = false;
            this.tcpClient = tcpClient;

            char[] splitDivision = new char[2];
            splitDivision[0] = '.';
            splitDivision[1] = ':';

            string[] temp = null;
            string[] temp2 = null;
            string[] temp3 = null;

            temp2 = tcpClient.Client.RemoteEndPoint.ToString().Split(splitDivision[1]);
            this.clientNumber = int.Parse(temp2[1]);

        }
    }
}