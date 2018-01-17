using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SMSPlatform.Controllers
{
    public class MessageHub:Hub
    {



        public override Task OnConnected()
        {
            return new Task(() =>
            {
                Clients.Clients(new List<string>() {Context.ConnectionId}).notify("已连接");
            });
        }

        public void Stop()
        {
            
        }

        public void Start()
        {

        }


    }
}
