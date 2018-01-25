using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSMMODEM;

namespace SMSPlatform.Controllers
{
    public class MessageHandler
    {
        private GsmModem modem;

        public MessageHandler(GsmModem modem)
        {
            this.modem = modem;
        }



    }
}
