using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoucherApplication
{
    public class User
    {
        public string userName { get; set; }

        public User(string name)
        {
            this.userName = name;
        }
    }
}
