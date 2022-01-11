using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WPF_KINL_Server
{
    class StaticDefine
    {
        public const int ADD_USER = 1;
        public const int ADD_DATA_LIST = 2;
        public const int REMOVE_USER_LIST = 3;
        public const int DATA_SEND_START = 4;
        public const int DATA_SEND_STOP = 5;
    }

    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Server());
        }
    }
}
