﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Formatting;
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
        private GSMTaskService taskService;

        public MessageController()
        {
            logger = AppDomain.CurrentDomain.GetData("Logger") as SMSPlatformLogger;
            try
            {
                pool = AppDomain.CurrentDomain.GetData("Pool") as GSMPool;
                taskService = AppDomain.CurrentDomain.GetData("TaskService") as GSMTaskService;
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
        public IHttpActionResult GetRecordContainer(string containerName, DateTime? beginTime, DateTime? endTime,
            int? pageSize, int? pageIndex)
        {
            var dcs = new FormDataCollection(this.Request.RequestUri);
            var status = dcs["status[]"]?.Split(',');
            var queryStatus = dcs["queryStatus[]"]?.Split(',');

            try
            {
                var whereStr = " where 1=1 ";
                if (!string.IsNullOrWhiteSpace(containerName))
                {
                    whereStr += $" and ContainerName like '%{containerName}%'";
                }

                if (beginTime.HasValue)
                {
                    whereStr += $" and CreateTime >= '{beginTime.Value:yyyy-MM-dd}'";
                }

                if (endTime.HasValue)
                {
                    whereStr += $" and CreateTime < '{endTime.Value.AddDays(1):yyyy-MM-dd}'";
                }

                if (status != null && status.Length != 0)
                {
                    whereStr += $" and Status in ({string.Join(",", status)})";
                }

                if (queryStatus != null && queryStatus.Length != 0)
                {
                    whereStr += $" and Status in ({string.Join(",", queryStatus)})";
                }




                var datas =
                    helper.SelectDataTable("select * from RecordContainer " + whereStr)
                        .Select()
                        .Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel);
                var total = datas.Count();
                if (pageSize.HasValue && pageIndex.HasValue)
                {
                    datas = datas.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);
                }
                return Json(new ReturnResult()
                {
                    data = datas,
                    success = true,
                    status = 200,
                    total = total
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

                helper.Update("RecordContainer", new Dictionary<string, object>() { { "Status", 4 } }, $" ID={id} ",
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
            DateTime? beginTime, DateTime? endTime, string smsContent, int? pageIndex, int? pageSize)
        {
            try
            {
                SMSSendQueueService service = new SMSSendQueueService(helper);
                var datas = service.GetSMSSend(containerId, toName, toPhone, 0, beginTime, endTime, smsContent);
                int total = datas.Count();
                if (pageIndex.HasValue && pageSize.HasValue)
                {
                    datas = datas.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);

                }
                return Json(new ReturnResult()
                {
                    success = true,
                    data = datas,
                    status = 200
                    ,
                    total = total
                });

            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),
#else 
                    msg = ex.ToString(),
#endif
                    success = false,
                    status = 500
                });
            }
        }


        [HttpGet]
        public IHttpActionResult GetSMSRecord(int? containerId, string toName, string toPhone,
            DateTime? beginTime, DateTime? endTime, string smsContent, int? pageIndex, int? pageSize)
        {
            try
            {
                SMSSendQueueService service = new SMSSendQueueService(helper);
                var datas = service.GetSMSRecord(containerId, toName, toPhone, 0, beginTime, endTime, smsContent);
                int total = datas.Count();
                if (pageIndex.HasValue && pageSize.HasValue)
                {
                    datas = datas.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);

                }
                return Json(new ReturnResult()
                {
                    success = true,
                    data = datas,
                    status = 200
                    ,
                    total = total
                });

            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),
#else 
                    msg = ex.ToString(),
#endif
                    success = false,
                    status = 500
                });
            }
        }


        [HttpGet]
        public IHttpActionResult GetSIMPhones()
        {
            var task = Task.Run(() => { });
            task.ContinueWith((x) =>
            {
                if (x.Exception == null)
                {
                    int i = 1;
                }
            });
            try
            {
                var date = DateTime.Now;
                var models =
                    helper.SelectDataTable($"select * from MonthlyFeeRecord")
                        .Select()
                        .Select(x => new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel);


                return Json(new ReturnResult()
                {
                    success = true,
                    data = pool.PhoneComDic.Select(x =>



                    {

                        var model = models.SingleOrDefault(y => x.Value == y.PhoneNumber);
                        var setting =
                            helper.SelectDataTable($"select * from SystemSettings where PhoneNumber = '{x.Value}'")
                                .Select()
                                .SingleOrDefault();
                        return new
                        {
                            com = x.Key,
                            text = x.Value + "  资费情况:" + (model == null
                                       ? setting == null ? "暂无数据" : "0/" + setting["MonthTotalCountLimit"]
                                       : (model.SendCount + "/" +
                                          model.MonthLimitRecord)),
                            id = x.Value,
                            disabled = false,
                            selected = false
                        };
                    }),
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
        public IHttpActionResult SetPhoneNumberByComPort(string comport, string phone)
        {
            var modem = pool[comport];
            if (modem != null)
            {
                if (modem.SetPhoneNum(phone))
                {
                    return Json(new ReturnResult()
                    {
                        msg = "号码更新成功",
                        success = true,
                        status = 200
                    });
                }
                else
                {
                    return Json(new ReturnResult()
                    {
                        msg = "号码有误或SIM卡错误。请先确定号码是否正确，如无误请联管理员",
                        success = false,
                        status = 500,
                    });
                }
            }
            else
            {
                return Json(new ReturnResult()
                {
                    msg = "您选择的COM端口不存在",
                    success = false,
                    status = 500,

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
                foreach (var item in container.SIMsPhone)
                {
                    taskService.Start(item);
                }
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

            var containerExists =
                helper.SelectDataTable("select * from RecordContainer where ID = " + model.ID)
                    .Select()
                    .Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel)
                    .SingleOrDefault();
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
                            .Select(x => (contactorModel)new contactorModel().SetData(x))
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
                    smsmodel.TemplateID = model.templateID;
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
                        $"select * from RecordContainer where ID = {containerId}")
                    .Select()
                    .Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel)
                    .SingleOrDefault();
                IEnumerable<SMSSendQueueModel> sendQueues = null;
                if (container != null)
                {
                    sendQueues =
                        helper.SelectDataTable($"select * from SMSSendQueue where ContainerID = {container.ID}")
                            .Select()
                            .Select(x => new SMSSendQueueModel().SetData(x) as SMSSendQueueModel);
                }



                return Json(new ReturnResult()
                {
                    data = new { container = container, sendQueues = sendQueues }
                    ,
                    success = true
                    ,
                    status = 200
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
                        success = false,
                        status = 500,
                    });
            }


        }

        [HttpGet]
        public IHttpActionResult ReSend(int containerId)
        {
            //            var recordModel = helper.SelectDataTable($"select * from SMSSendRecord where ID = {smsRecordId}").Select().Select(x=>new SMSSendQueueModel().SetData(x) as SMSSendQueueModel).SingleOrDefault();
            //            if (recordModel == null)
            //            {
            //                return Json(new ReturnResult()
            //                {
            //                    msg = "you can't resend a sms that not exists!",
            //                    success = false,
            //                    status = 500
            //                });
            //            }

            var container =
                helper.SelectDataTable($"select * from recordcontainer where ID = {containerId} ")
                    .Select()
                    .Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel)
                    .SingleOrDefault();
            var errorSMSs =
                helper.SelectDataTable($"select * from SMSSendRecord where Status = 3 and containerID = {containerId}")
                    .Select()
                    .Select(x => new SMSSendRecordModel().SetData(x) as SMSSendRecordModel);
            if (container == null)
            {
                return Json(new ReturnResult()
                {
                    msg = "未找到对应任务!",
                    success = false,
                    status = 500
                });
            }
            if (!errorSMSs.Any())
            {
                return Json(new ReturnResult()
                {
                    msg = "可重发任务为空!",
                    success = false,
                    status = 500
                });
            }
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {
//                helper.Delete("smssendrecord", $"containerID = {containerId}");
               
                var containerDic = new Dictionary<string, object>();
                container.GetValues(containerDic);
                containerDic.Remove("ID");
                containerDic["Status"] = 0;

//                helper.Update("recordContainer", new Dictionary<string, object>() { { "status", 0 } },
//                    $" ID = {container.ID}", new List<SqlParameter>());
                
                var newContainerID = helper.Insert("recordContainer", containerDic, "OUTPUT inserted.ID");

                foreach (var item in errorSMSs)
                {
                    var dic = new Dictionary<string, object>();
                    item.Status = 0;
                    item.GetValues(dic);
                    
                    dic.Remove("ID");
                    dic["ContainerID"] = newContainerID;
                    helper.Insert("SMSSendQueue", dic);
                }

                tran.Commit();

                return Json(new ReturnResult()
                {
                    msg = "重新启用成功",
                    success = true,
                    status = 200,
                });
            }
            catch (Exception e)
            {
                tran.Rollback();
                return Json(new ReturnResult()
                {
                    msg = e + "",
                    success = false,
                    status = 500,

                });
            }





        }

        [HttpGet]
        public IHttpActionResult CalcMessageCount(int containerId)
        {
            try
            {
                var containerModel =
                    helper.SelectDataTable($"select * from RecordContainer where ID = {containerId}")
                        .Select()
                        .Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel)
                        .SingleOrDefault();

                if (containerModel == null)
                {
                    throw new Exception("ContainerID未找到");
                }


                var smss = helper.SelectDataTable(
                        $"select * from SMSSendQueue where ContainerID = {containerModel.ID}")
                    .Select()
                    .Select(x => new SMSSendQueueModel().SetData(x) as SMSSendQueueModel);


                var selectedPhones = containerModel.SIMsPhone;

                var count = smss.Select(x => GsmModem.CalcMsgLong(x.SMSContent, "0000000000"))
                    .Aggregate(0, (o, n) => o + n);
                var date = DateTime.Now;

                var monthlyFeeModels = helper.SelectDataTable(
                        $"select * from MonthlyFeeRecord where PhoneNumber in ('{string.Join("','", selectedPhones)}') and Month = '{date.Month}' and Year = '{date.Year}'")
                    .Select()
                    .Select(x => new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel);

                var monthlyExcept = selectedPhones.Except(monthlyFeeModels.Select(x => x.PhoneNumber));

                var settings = monthlyExcept.Any()
                    ? helper.SelectDataTable(
                            $"select * from SystemSettings where PhoneNumber in ('{string.Join("','", monthlyExcept)}') ")
                        .Select()
                        .Select(x => new SystemSettingsModel().SetData(x) as SystemSettingsModel)
                    : new List<SystemSettingsModel>();

                var totalRest =
                    monthlyFeeModels.Where(x => x.MonthLimitRecord != 0)
                        .Select(x => x.MonthLimitRecord - x.SendCount)
                        .Aggregate(0, (o, n) => o + n) +
                    settings.Aggregate(0, (o, n) => o + n.MonthTotalCountLimit.Value);


                return Json(new ReturnResult()
                {
                    success = true,
                    data = new { totalRest, count },
                    status = 200
                });
            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                    success = false,
#if DEBUG
                    msg = e.ToString(),

#else
                    msg = "内部错误 请联系管理员",
#endif

                    status = 500,
                });
            }


        }

        [HttpPost]
        public IHttpActionResult CalcMessageAndWordCount(CalcModel model)
        {
            try
            {
                if (model.TemplateID<=0)
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg = "请选择模板"
                    });
                }


                TemplateService service = new TemplateService(helper);

                var template = service.GetTemplate(model.TemplateID);

                var replacedContent = service.TemplateReplace(template.TemplateContent, new Dictionary<string, string>()
            {
                {"姓名","姓名星" },
                { "通知内容",model.Content},
                {"日期", DateTime.Now.ToString("yyyy年MM月dd日")},

            });

                var count = GsmModem.CalcMsgLong(replacedContent, "11111111111");

                return Json(new ReturnResult()
                {
                    data = new { calcedSMSCount=count, calcedContentCount = replacedContent.Length },success = true
                });

            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                  success = false,
                  msg=e.ToString()
                });
            }



        }

        public class CalcModel
        {
            public string Content { get; set; }
            public int TemplateID { get; set; }
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
