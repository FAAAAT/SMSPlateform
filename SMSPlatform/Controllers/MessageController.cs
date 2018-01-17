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
        public IHttpActionResult GetRecordContainer(string containerName)
        {
            try
            {
                var whereStr = " where 1=1 ";
                if (!string.IsNullOrWhiteSpace(containerName))
                {
                    whereStr += $" and ContainerName like '%{containerName}%'";
                }
                var datas = helper.SelectDataTable("select * from RecordContainer " + whereStr).Select().Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel);

                return Json(new ReturnResult()
                {
                    data = datas,
                    success = true,
                    status = 200,
                });

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = e.ToString(),
#else
                    msg = "内部错误，请联系管理员",

#endif
                    status = 500,
                    success = false,
                });
            }


        }
        [HttpGet]
        public IHttpActionResult DeleteRecordContainer(int id)
        {


            var recordCount = helper.SelectScalar<int>($"select count(1) from SMSSendRecord where ContainerID = {id}");

            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {

                helper.Update("RecordContainer", new Dictionary<string, object>() {{"Status", 3}}, $" ID={id} ",
                    new List<SqlParameter>());
                helper.Delete("SMSSendQueue", $"ContainerID ={id}");
                tran.Commit();
                return Json(new ReturnResult()
                {
                    msg = "删除成功",
                    success = true,
                    status = 200,
                });
            }
            catch (Exception e)
            {
                tran.Rollback();
                logger.Error(e.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = e.ToString(),
#else
                    msg = "内部错误,请联系管理员",
#endif
                    success = false,
                    status = 500,
                });
            }
            finally
            {
                helper.ClearTransaction();
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
                    data = pool.PhoneComDic.Select(x=>new {text=x.Value,id=x.Value,disabled = false,selected = false}),
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
                container.Status = 0;

                var containerid = service.AddContainer(container);
                var templateService = new TemplateService(helper);
                TemplateModel template = templateService.GetTemplate(model.templateID);


                foreach (var contactorID in model.selectedContactors)
                {
                    var cmodel =
                        helper.SelectDataTable("select * from Contactor where ID = " + contactorID)
                            .Select()
                            .Select(x => (contactorModel) new contactorModel().SetData(x))
                            .SingleOrDefault();

                    SMSSendQueueModel smsmodel = new SMSSendQueueModel();
                    smsmodel.ContainerID = containerid;
                    smsmodel.CreateTime = container.CreateTime;
                    smsmodel.SMSContent = templateService.TemplateReplace(template.TemplateContent,
                        new Dictionary<string, string>()
                        {
                            {"姓名", cmodel.ContactorName},
                            {"日期", DateTime.Now.ToString("yyyy年MM月dd日")},
                            {"通知内容", model.actualContent}
                        });
                    smsmodel.TemplateID = template.ID.Value;
                    smsmodel.TemplateContent = template.TemplateContent;
                    smsmodel.InnerTemplateContent = model.actualContent;
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
            finally
            {
                helper.ClearTransaction();
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
                    success = false,
                    status = 500
                });
            }
        }

        [HttpGet]
        public IHttpActionResult UpdateHallContainer(string json)
        {
            var model = JsonConvert.DeserializeObject<WizardUpdateDataModel>(json);
            if (!model.ID.HasValue)
            {
                return Json(new ReturnResult()
                {
                    msg = "修改任务需要ID",
                    success = false,
                    status = 500
                });

            }
            int containerCount = helper.SelectScalar<int>($"select count(1) from recordcontainer where ID = {model.ID}");
            if (containerCount == 0)
            {
                return Json(new ReturnResult()
                {
                    msg = "未找到要修改的任务",
                    success = false,
                    status = 500
                });

            }

            var containerExists = helper.SelectDataTable("select * from RecordContainer where ID = " + model.ID).Select().Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel).SingleOrDefault();
            if (containerExists != null && containerExists.Status != 0)
            {
                return Json(new ReturnResult()
                {
                    msg = "要修改的任务必须处于等待发送状态",
                    success = false,
                    status = 500,
                });
            }





            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {
                var service = new SMSSendQueueService(helper);
                RecordContainerModel container = new RecordContainerModel();
                container.ContainerName = model.containerData.containerName;
                container.CreateTime = DateTime.Now;
                container.SIMsPhone = model.containerData.simPhones;
                container.Status = 0;

                var dic = new Dictionary<string, object>();
                container.GetValues(dic);
                helper.Update("RecordContainer", dic, $" ID={model.ID} ", new List<SqlParameter>());

                helper.Delete("SMSSendQueue", $"ContainerID=" + model.ID);

                var templateService = new TemplateService(helper);
                TemplateModel template = templateService.GetTemplate(model.templateID);


                foreach (var contactorID in model.selectedContactors)
                {
                    var cmodel =
                        helper.SelectDataTable("select * from Contactor where ID = " + contactorID)
                            .Select()
                            .Select(x => (contactorModel) new contactorModel().SetData(x))
                            .SingleOrDefault();

                    SMSSendQueueModel smsmodel = new SMSSendQueueModel();
                    smsmodel.ContainerID = model.ID;
                    smsmodel.CreateTime = container.CreateTime;
                    smsmodel.SMSContent = templateService.TemplateReplace(template.TemplateContent,
                        new Dictionary<string, string>()
                        {
                            {"姓名", cmodel.ContactorName},
                            {"日期", DateTime.Now.ToString("yyyy年MM月dd日")},
                            {"通知内容", model.actualContent}
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
            finally
            {
                helper.ClearTransaction();
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

        [HttpGet]
        public IHttpActionResult GetContainerDetail(int containerId)
        {



            try
            {
                var container = helper.SelectDataTable(
                    $"select * from RecordContainer where ID = {containerId}").Select().Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel).SingleOrDefault();
                IEnumerable<SMSSendQueueModel> sendQueues = null;
                if (container != null)
                {
                    sendQueues = helper.SelectDataTable($"select * from SMSSendQueue where ContainerID = {container.ID}").Select().Select(x => new SMSSendQueueModel().SetData(x) as SMSSendQueueModel);
                }

               

                return Json(new ReturnResult()
                {
                    data = new {container = container,sendQueues = sendQueues}
                    ,success = true
                    ,status = 200
                });

            }
            catch (Exception e)
            {
                logger.Error(e + "");
                return Json(
                    new ReturnResult()
                    {
#if DEBUG
                        msg = e.ToString(),
#else
                        msg = "内部错误 请联系管理员",
#endif
                        success =  false,
                        status = 500,
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
