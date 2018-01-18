using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class TaskServiceController:ApiController
    {
        private GSMTaskService taskService;

        public TaskServiceController()
        {
            taskService = AppDomain.CurrentDomain.GetData("TaskService") as GSMTaskService;
            
        }

        public IHttpActionResult GetServiceStatus()
        {

            return Json(new ReturnResult()
            {
                success = true,
                data = new {status = taskService.Status}

            });

        }


    }
}
