using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using GSMMODEM;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class SMSManagementController : ApiController
    {

        [HttpGet]
        public IHttpActionResult GetSPs()
        {
            return null;
        }




        [HttpGet]
        [Authorize(Users = "admin")]
        public IHttpActionResult SendSMS(string phone, string msg, string selectedCom)
        {
            GsmModem gsm = new GsmModem(selectedCom, SPService.BandRate);
            
            gsm.Open();
            try
            {
                gsm.GetMsgCenterNo();

                var result = new ReturnResult();

//                if (!gsm.SendMsg(phone, msg, out var error))
//                {
//                    result.msg = error;
//                    result.success = false;
//                    result.status = 500;
//                }
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


//        public IHttpActionResult GetSMSAndDel()
//        {
//            
//
//        }
    }
}
