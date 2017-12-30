using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class ReturnResult
    {
        public string msg { get; set; }
        public bool success { get; set; }
        public int status { get; set; } = 200;
        public int total { get; set; }
        public object data { get; set; }

    }
}
