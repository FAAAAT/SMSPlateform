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
        public IHttpActionResult GetSMSQueue(int? containerId, string toName, string toPhone,
            DateTime? beginTime, DateTime? endTime, string smsContent)
        {
            try
            {
                SMSSendQueueService service = new SMSSendQueueService(helper);
                var datas = service.GetSMSSend(containerId, toName, toPhone, 0, beginTime, endTime, smsContent);
                return Json(new ReturnResult()
                {
                    success = true,
                    data = datas,
                    status = 200
                });

            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),
#else 
                    msg = ex.toString(),
#endif
                    success = false,
                    status = 500
                });
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
        public IHttpActionResult CreateSMSQueue(string json)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {
                var model = JsonConvert.DeserializeObject<WizardUpdateDataModel>(json);
                var service = new SMSSendQueueService(helper);
                RecordContainerModel container = new RecordContainerModel();
                container.ContainerName = model.containerData.containerName;
                container.CreateTime = DateTime.Now;
                container.SIMsPhone = model.containerData.simPhones;

                var containerid = service.AddContainer(container);
                var templateService = new TemplateService(helper);
                TemplateModel template = templateService.GetTemplate(model.templateID);


                foreach (var contactorID in model.selectedContactors)
                {
                    var cmodel =
                        helper.SelectDataTable("select * from Contactor where ID = " + contactorID)
                            .Select()
                            .Select(x => (contactorModel)new contactorModel().SetData(x))
                            .SingleOrDefault();

                    SMSSendQueueModel smsmodel = new SMSSendQueueModel();
                    smsmodel.ContainerID = containerid;
                    smsmodel.CreateTime = container.CreateTime;
                    smsmodel.SMSContent = templateService.TemplateReplace(template.TemplateContent,
                        new Dictionary<string, string>()
                        {
                            {"姓名", cmodel.ContactorName},
                            {"日期", DateTime.Now.ToString("yyyy年MM月dd日")},

                        });
                    smsmodel.ToContactorID = contactorID;
                    smsmodel.ToName = cmodel.ContactorName;
                    smsmodel.ToPhoneNumber = cmodel.PhoneNumber;
                    smsmodel.Status = 0;
                    smsmodel.CreateTime = DateTime.Now;
                    service.AddSMSSendQueue(smsmodel);
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
                    msg = e + "",
#else
                    msg = "服务器内部错误 请联系管理员",
#endif
                    success = false,
                    status = 500,
                });
            }
        }

        [HttpGet]
        public IHttpActionResult GetSMSQueue(int id)
        {
            try
            {
                SMSSendQueueService service = new SMSSendQueueService(helper);
                var data = service.GetSendQueueModel(id);
                return Json(new ReturnResult()
                {
                    data = data,
                    success = true,
                    status = 200

                });

            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),
#else
                    msg = "内部错误",
#endif
                    success = false,
                    status = 500,
                });
            }
        }

        [HttpGet]
        public IHttpActionResult UpdateSMSQueue([FromUri] SMSSendQueueModel model)
        {
            try
            {
                SMSSendQueueService service = new SMSSendQueueService(helper);
                service.UpdateSMS(model);
                return Json(new ReturnResult()
                {
                    msg = "更新成功",
                    success = true,
                    status = 200
                });

            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),
#else 
                    msg = "内部错误",
#endif
                    success=false,
                    status = 500
                });
            }
        }

        [HttpGet]
        public IHttpActionResult DeleteSMSQueue(int id)
        {
            try
            {
                SMSSendQueueService service = new SMSSendQueueService(helper);
                service.DeleteSMS(id);
                return Json(new ReturnResult()
                {
                    msg = "删除成功",
                    success = true,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),

#else 
                    msg="内部错误",
#endif
                    success = false,
                    status = 500,
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
                    data = pool.PhoneComDic.Select(x => new { id = x.Value, text = x.Key }),
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
            if (helper != null)
            {
                helper.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
