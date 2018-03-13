using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class MessageHub : Hub
    {

        private GSMTaskService taskService = AppDomain.CurrentDomain.GetData("TaskService") as GSMTaskService;

        public override Task OnConnected()
        {
            var task = new Task(() =>
            {
                Clients.Clients(new List<string>() {Context.ConnectionId}).notify("已连接");
            });
            task.Start();
            return task; 
        }


      

        public int Stop()
        {
            taskService.Stop();
            return (int)taskService.Status;
        }

        public int Start()
        {
            taskService.Start();
            return (int) taskService.Status;
        }

        public void StartOne(string phone)
        {
            taskService.Start(phone);
        }

    }
}
