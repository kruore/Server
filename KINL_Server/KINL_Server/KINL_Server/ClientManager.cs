using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace KINL_Server
{
    class ClientManager
    {

        public event Action<string, int> EventHandler = null;
    }

}
