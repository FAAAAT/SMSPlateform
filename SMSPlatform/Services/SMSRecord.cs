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
            var container = helper.SelectDataTable($"select * from RecordContainer where ID = (select Min(ID) from RecordContainer where SIMsPhone like '%{phone}%' and (status = 0 or status = 1)) and (status = 0 or status = 1)").Select().Select(x => new RecordContainerModel().SetData(x) as RecordContainerModel).SingleOrDefault();
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

        public void CompleteSMS(int smsID, bool success)
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
                model.GetValues(dic);
                dic.Remove("ID");

                if (success)
                {
                    helper.Delete("SMSSendQueue", $" ID = {model.ID}");
                    helper.Insert("SMSSendRecord", dic);
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
                    containerModel.Status = errCount==0?2:3;
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
            var count = helper.SelectScalar<int>($"select count(1) from SMSSendQueue where ContainerID = {id} and status = 3");
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
                whereStr += $" and Status = {status}";
            }
            if (beginTime.HasValue)
            {
                whereStr += $" and CreateTime > '{beginTime}'";
            }
            if (endTime.HasValue)
            {
                whereStr += $" and CreateTime < '{endTime}'";
            }
            if (!string.IsNullOrWhiteSpace(smsContent))
            {
                whereStr += $" and SMSContent like '%{smsContent}%'";
            }

            return helper.SelectDataTable("select * from SMSSendRecord " + whereStr + " union all select * from SMSSendRecord " + whereStr).Select().Select(x => (SMSSendQueueModel)new SMSSendQueueModel().SetData(x));

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
