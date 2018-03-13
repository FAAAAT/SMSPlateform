using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAccessHelper;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class SMSSendQueueService
    {
        SqlHelper helper;

        public SMSSendQueueService(SqlHelper helper)
        {
            this.helper = helper;
        }

        public int AddContainer(RecordContainerModel model)
        {
            var dic = new Dictionary<string, object>();
            model.GetValues(dic);
            dic.Remove("ID");
            return (int)helper.Insert("RecordContainer", dic, "OUTPUT inserted.ID");
        }

        public int AddSMSSendQueue(SMSSendQueueModel model)
        {
            var dic = new Dictionary<string, object>();
            model.GetValues(dic);
            model.Status = 0;
            dic.Remove("ID");
            return (int)helper.Insert("SMSSendQueue", dic, "OUTPUT inserted.ID");
        }

        public SMSSendQueueModel GetNextData(string phone)
        {
            var container = helper.SelectDataTable($"select * from RecordContainer where ID = (select Min(ID) from RecordContainer where SIMsPhone like '%{phone}%' and (status = 5 or status =1))").Select().Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel).SingleOrDefault();
            if (container == null)
            {
                return null;
            }
            else
            {
                var model = helper.SelectDataTable(
                    $"select * from SMSSendQueue where ID = (select Min(ID) from SMSSendQueue where ContainerID = {container.ID}) and (status = 0)").Select().Select(x => new SMSSendQueueModel().SetData(x) as SMSSendQueueModel).SingleOrDefault();
                if (model == null)
                {
                    helper.Update("RecordContainer", new Dictionary<string, object>() {{"Status", 2}},
                        $"ID = {container.ID}",new List<SqlParameter>());
                    return model;
                }
                model.Status = 1;
                var dic = new Dictionary<string, object>();
                model.GetValues(dic);
                dic.Remove("ID");
                helper.Update("SMSSendQueue", dic, " ID = " + model.ID, new List<SqlParameter>());

                return model;
            }


        }

        public void CompleteSMS(int smsID,string phone, bool success,int actSendCount)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            SMSSendQueueModel model = null;
            helper.SetTransaction(tran);
            try
            {
                model = helper.SelectDataTable($"select * from SMSSendQueue where ID = {smsID}").Select()
                    .Select(x => (SMSSendQueueModel)new SMSSendQueueModel().SetData(x)).SingleOrDefault();

                var dic = new Dictionary<string, object>();
                model.Status = success ? 2 : 3;
                model.SendTime = DateTime.Now;

                model.GetValues(dic);
                dic.Remove("ID");
                dic["SIMPhone"]=phone;
                helper.Delete("SMSSendQueue", $" ID = {model.ID}");
                helper.Insert("SMSSendRecord", dic);

                lock (this)
                {
                    var year = model.SendTime.Value.Year;
                    var month = model.SendTime.Value.Month;
                    var monthlyFeeRecord = helper.SelectDataTable($"select * from MonthlyFeeRecord where PhoneNumber = '{phone}' and Year = {year} and Month = '{month}'")
                        .Select().Select(x=>new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel).SingleOrDefault();
                    int monthlyID = -1;
                    if (monthlyFeeRecord == null)
                    {
                        var limitRow = helper.SelectDataTable($"select * from SystemSettings where PhoneNumber = '{phone}'").Select().SingleOrDefault();
                      

                        monthlyFeeRecord = new MonthlyFeeRecordModel();
                        monthlyFeeRecord.Month = month;
                        monthlyFeeRecord.Year = year;
                        monthlyFeeRecord.MonthLimitRecord =
                            (int?) limitRow?["MonthTotalCountLimit"] ?? 0;
                        monthlyFeeRecord.SendCount = actSendCount;
                        monthlyFeeRecord.PhoneNumber = phone;
                        var feeDic = new Dictionary<string,object>();
                        monthlyFeeRecord.GetValues(feeDic);
                        monthlyID = (int)helper.Insert("MonthlyFeeRecord", feeDic,"OUTPUT inserted.ID");
                    }
                    else
                    {
                        monthlyID = monthlyFeeRecord.ID.Value;
                        helper.ExecuteNoneQuery(
                            $"update MonthlyFeeRecord set SendCount = SendCount+{actSendCount} where ID = {monthlyFeeRecord.ID}");
                    }

                    var DailyFeeRecord =
                        helper.SelectDataTable(
                            $"select * from DailyFeeRecord where PhoneNumber='{phone}' and Date = '{model.SendTime.Value.Date:yyyy-MM-dd}'").Select().Select(x=>new DailyFeeRecordModel().SetData(x) as DailyFeeRecordModel).SingleOrDefault();
                    if (DailyFeeRecord == null)
                    {
                        DailyFeeRecord = new DailyFeeRecordModel();
                        DailyFeeRecord.Date = model.SendTime.Value;
                        DailyFeeRecord.MonthlyID = monthlyID;
                        DailyFeeRecord.PhoneNumber = phone;
                        DailyFeeRecord.SendCount = actSendCount;

                        var dailyDic = new Dictionary<string,object>();
                        DailyFeeRecord.GetValues(dailyDic);
                        helper.Insert("DailyFeeRecord", dailyDic);
                    }
                    else
                    {
                        helper.ExecuteNoneQuery(
                            $"update DailyFeeRecord set SendCount = SendCount+{actSendCount} where ID = {DailyFeeRecord.ID}");
                    }

                }


                tran.Commit();
            }
            catch (Exception)
            {
                tran.Rollback();
            }
            finally
            {
                helper.ClearTransaction();
            }
            if (model != null)
            {
                var count = GetNotCompletedContainerQueueCount(model.ContainerID.Value);
                var errCount = GetErrorContainerQueueCount(model.ContainerID.Value);

                if (count == 0)
                {
                    var containerModel = helper.SelectDataTable($"select * from RecordContainer where ID = {model.ContainerID}").Select().Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel).SingleOrDefault();
                    var dic = new Dictionary<string, object>();
                    containerModel.Status = errCount == 0 ? 2 : 3;
                    containerModel.GetValues(dic);
                    dic.Remove("ID");
                    helper.Update("RecordContainer", dic, " ID = " + containerModel.ID.Value, new List<SqlParameter>());
                }



            }
        }


        public int GetNotCompletedContainerQueueCount(int id)
        {
            var count = helper.SelectScalar<int>($"select count(1) from SMSSendQueue where ContainerID = {id} and (status =0 or status =1)");
            return count;
        }

        public int GetErrorContainerQueueCount(int id)
        {
            var count = helper.SelectScalar<int>($"select count(1) from SMSSendRecord where ContainerID = {id} and status = 3");
            return count;
        }

        public int? GetContainerStatus(int id)
        {
            return helper.SelectScalar<int?>($"select status from recordcontainer where ID = {id}");
        }


        /// <summary>
        /// 只能删除还未发送的短消息
        /// </summary>
        /// <param name="smsID"></param>
        public void DeleteSMS(int smsID)
        {
            helper.Delete("SMSSendQueue", $" ID = {smsID}");

        }

        public IEnumerable<SMSSendQueueModel> GetSMSSend(int? containerId, string toName, string toPhone, int? status, DateTime? beginTime, DateTime? endTime, string smsContent)
        {
            var whereStr = " where 1=1 ";
            if (containerId.HasValue)
            {
                whereStr += $" and ContainerID ={containerId}";
            }
            if (!string.IsNullOrWhiteSpace(toName))
            {
                whereStr += $" and ToName like '%{toName}%'";
            }
            if (!string.IsNullOrWhiteSpace(toPhone))
            {
                whereStr += $" and ToPhoneNumber like '%{toPhone}%'";
            }
            if (status.HasValue)
            {
                whereStr += $" and Status = {status}";
            }
            if (beginTime.HasValue)
            {
                whereStr += $" and CreateTime > '{beginTime}'";
            }
            if (endTime.HasValue)
            {
                whereStr += $" and CreateTime < '{endTime.Value.AddDays(1)}'";
            }
            if (!string.IsNullOrWhiteSpace(smsContent))
            {
                whereStr += $" and SMSContent like '%{smsContent}%'";
            }

            return helper.SelectDataTable("select * from SMSSendQueue " + whereStr + " union all select * from SMSSendRecord " + whereStr).Select().Select(x => (SMSSendQueueModel)new SMSSendQueueModel().SetData(x));

        }


        public IEnumerable<SMSSendQueueModel> GetSMSRecord(int? containerId, string toName, string toPhone, int? status, DateTime? beginTime, DateTime? endTime, string smsContent)
        {
            var whereStr = " where 1=1 ";
            if (containerId.HasValue)
            {
                whereStr += $" and ContainerID ={containerId}";
            }
            if (!string.IsNullOrWhiteSpace(toName))
            {
                whereStr += $" and ToName like '%{toName}%'";
            }
            if (!string.IsNullOrWhiteSpace(toPhone))
            {
                whereStr += $" and ToPhoneNumber like '%{toPhone}%'";
            }
            if (status.HasValue)
            {
                whereStr += $" and Status <> {status}";
            }
            if (beginTime.HasValue)
            {
                whereStr += $" and CreateTime > '{beginTime}'";
            }
            if (endTime.HasValue)
            {
                whereStr += $" and CreateTime < '{endTime.Value.AddDays(1)}'";

            }
            if (!string.IsNullOrWhiteSpace(smsContent))
            {
                whereStr += $" and SMSContent like '%{smsContent}%'";
            }

            return helper.SelectDataTable(" select * from SMSSendRecord " + whereStr).Select().Select(x => (SMSSendQueueModel)new SMSSendQueueModel().SetData(x));

        }

        /// <summary>
        /// 只能修改还未发送的短消息
        /// </summary>
        /// <param name="model"></param>
        public void UpdateSMS(SMSSendQueueModel model)
        {
            var dic = new Dictionary<string, object>();
            var id = model.ID;
            if (!id.HasValue)
            {
                throw new Exception("ID不能为空");
            }
            dic.Remove("ID");
            helper.Update("SMSSendQueue", dic, " ID=" + id, new List<SqlParameter>());


        }

        public IEnumerable<int> GetSendingSMSID()
        {
            return helper.SelectList<int>("SMSSendQueue", "ID");
        }

        public SMSSendQueueModel GetSendQueueModel(int id)
        {
            return helper.SelectDataTable("select * from SMSSendQueue where ID = " + id).Select().Select(x => (SMSSendQueueModel)new SMSSendQueueModel().SetData(x)).SingleOrDefault();
        }

        public RecordContainerModel GetContainerModel(int id)
        {
            return helper.SelectDataTable("select * from RecordContainer where ID = " + id).Select().Select(x => (RecordContainerModel)new RecordContainerModel().SetData(x)).SingleOrDefault();
        }


    }
}
