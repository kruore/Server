using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KINL_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            MainServer a =new MainServer();
            a.ConsoleView();
        }
    }
}
