using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DataBaseAccessHelper;
using GSMMODEM;
using Logger;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class MessageController:ApiController
    {
        private SqlHelper helper;
        private GSMPool pool;
        private SMSPlatformLogger logger;
        public MessageController()
        {
            logger = AppDomain.CurrentDomain.GetData("Logger") as SMSPlatformLogger;
            try
            {
                pool = AppDomain.CurrentDomain.GetData("Pool") as GSMPool;
                var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
                helper = new SqlHelper();
                conn.Open();
                helper.SetConnection(conn);
            }
            catch (Exception e)
            {
               logger.Error(e+"");
            }
      
            
        }

        [HttpGet]
        public IHttpActionResult GetSIMPhones()
        {
            try
            {
                return Json(new ReturnResult()
                {
                    success = true,
                    data = pool.PhoneComDic,
                    status = 200,
                });
            }
            catch (Exception e)
            {
                
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = e+"",
#else
                    msg = "服务器内部错误 请联系管理员",
#endif
                    status = 500,
                    success = false,

                });
            }
        
        }

//        [HttpGet]
//        public IHttpActionResult holder()
//        {
//
//        }




        [HttpGet]
        public IHttpActionResult Select2GetSIMPhones()
        {
            try
            {
                return Json(new ReturnResult()
                {
                    success = true,
                    data = pool.PhoneComDic.Select(x=>new {id=x.Value,text = x.Key}),
                    status = 200,
                });
            }
            catch (Exception e)
            {

                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = e + "",
#else
                    msg = "服务器内部错误 请联系管理员",
#endif
                    status = 500,
                    success = false,

                });
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (helper!=null)
            {
                helper.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
