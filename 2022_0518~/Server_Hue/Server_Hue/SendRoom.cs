using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_Hue
{
    internal class SendRoom
    {

        private string roomId;
        private bool isDeviceConnect;
        private bool isIosConnect;
        private bool isReady;
        SendRoom(string _roomId, bool isDevice, bool isIOS)
        {
            roomId= _roomId;
            isDeviceConnect= isDevice;
            isIosConnect= isIOS;

            if(isDevice && isIOS)
            {
                isReady = true;
            }
        }

        void WaittingOtherDevice()
        {
            if(!isDeviceConnect)
            {

            }
        }
    }
}
