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
using Newtonsoft.Json;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class MessageController : ApiController
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
                logger.Error(e + "");
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
                    msg = e + "",
#else
                    msg = "服务器内部错误 请联系管理员",
#endif
                    status = 500,
                    success = false,

                });
            }

        }

        [HttpGet]
        public IHttpActionResult CreateSMSRecord(string json)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            
            try
            {
                var model = JsonConvert.DeserializeObject<WizardUpdateDataModel>(json);
                var service = new SMSSendQueueService(helper);
                RecordContainerModel container = new RecordContainerModel();
                container.ContainerName = model.containerData.containerName;
                container.CreateTime = DateTime.Now;
                container.SIMsPhone = model.containerData.simPhones;

                var containerid = service.AddContainer(container);

               
                foreach (var contactorID in model.selectedContactors)
                {
                    var cmodel =
                        helper.SelectDataTable("select * from where ID = " + contactorID)
                            .Select()
                            .Select(x => (contactorModel) new contactorModel().SetData(x))
                            .SingleOrDefault();

                    SMSSendQueueModel smsmodel = new SMSSendQueueModel();
                    smsmodel.ContainerID = containerid;
                    smsmodel.CreateTime = container.CreateTime;
                    smsmodel.SMSContent
                }
                tran.Commit();
                return Json(new ReturnResult()
                {
                    msg = "创建成功",
                    status = 200,
                    success = true
                });
            }
            catch (Exception e)
            {
                tran.Rollback();
                return Json(new ReturnResult()
                {

#if DEBUG
                    msg = e+"",
#else
                    msg = "服务器内部错误 请联系管理员",
#endif
                    success = false,
                    status=500,
                });
            }
        }
        



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
