using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DataBaseAccessHelper;
using GSMMODEM;
using Microsoft.Owin.Security.Provider;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class SMSManagementController : ApiController
    {

        [HttpGet]
        public IHttpActionResult GetSPs()
        {
            SPService service = new SPService();
            return Json(new ReturnResult()
            {
                success = true,
                data = service.GetSPNames().Select(x => new { key = x, value = x }),

            });
        }




        [HttpGet]
        [LymiAuthorize(Roles = "admin")]
        public IHttpActionResult SendSMS(string phone, string msg, string selectedCom)
        {
            GsmModem gsm = new GsmModem(selectedCom, SPService.BandRate);

            gsm.Open();
            try
            {
                gsm.GetMsgCenterNo();

                var result = new ReturnResult();

                if (!gsm.SendMsg(phone, msg, out string
                error, out
                int count))
                {
                    result.msg = error;
                    result.success = false;
                    result.status = 500;
                }
                return Json(result);

            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                    msg = e.ToString(),
                    status = 500,
                    success = false,
                });
            }
            finally
            {
                gsm.Close();
            }
        }


        [HttpGet]
        [LymiAuthorize(Roles = "admin")]
        public IHttpActionResult StartContainer(int containerId)
        {
            SqlConnection connection = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            connection.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(connection);
            try
            {
                helper.Update("RecordContainer", new Dictionary<string, object>()
                    {
                        {"Status", 5}
                    }, $" ID ={containerId}",
                    new List<SqlParameter>());
                GSMTaskService taskService = AppDomain.CurrentDomain.GetData("TaskService") as GSMTaskService;
                taskService.Start();
                return Json(new ReturnResult()
                {
                    success = true,
                    status = 200,
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status = 500,
                });
            }
            finally
            {
                helper.Dispose();
            }


        }



        [HttpGet]
        [LymiAuthorize(Roles = "admin")]
        public IHttpActionResult PauseContainer(int containerId)
        {

            SqlConnection connection = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            connection.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(connection);
            try
            {
                helper.Update("RecordContainer", new Dictionary<string, object>()
                    {
                        {"Status", 6}
                    }, $" ID ={containerId}",
                    new List<SqlParameter>());
                GSMTaskService taskService = AppDomain.CurrentDomain.GetData("TaskService") as GSMTaskService;
                taskService.Start();
                return Json(new ReturnResult()
                {
                    success = true,
                    status = 200,
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status = 500,
                });
            }
            finally
            {
                helper.Dispose();
            }



        }


        [HttpGet]
        public IHttpActionResult GetStatus()
        {
            try
            {
                GSMTaskService taskService = AppDomain.CurrentDomain.GetData("TaskService") as GSMTaskService;
                return Json(new ReturnResult()
                {
                    success = true,
                    data = taskService.Status,
                    status = 200,
                });
            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    data = e.ToString(),
                    status = 500,
                });
            }

        }





        //        public IHttpActionResult GetSMSAndDel()
        //        {
        //            
        //
        //        }
    }
}
