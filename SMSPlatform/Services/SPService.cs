using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Services
{
    public class SPService
    {

        public static int BandRate = 115200;

        public IEnumerable<string> GetSPNames()
        {
            return SerialPort.GetPortNames();
        }

        
        
    }
}
