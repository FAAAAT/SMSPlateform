using System;
using System.Collections.Generic;
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
            return (int) helper.Insert("RecordContainer", dic, "OUTPUT inserted.ID");
        }

        public int AddSMSSendQueue(SMSSendQueueModel model)
        {
            var dic = new Dictionary<string, object>();
            model.GetValues(dic);
            model.Status = 0;
            dic.Remove("ID");
            return (int) helper.Insert("SMSSendQueue", dic, "OUTPUT inserted.ID");
        }

        public void CompleteSMS(int smsID,bool success)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            try
            {
                var model = helper.SelectDataTable($"select * from SMSSendQueue where ID = {smsID}").Select().Select(x=>(SMSSendQueueModel)new SMSSendQueueModel().SetData(x)).SingleOrDefault();
                helper.Delete("SMSSendQueue", $" ID = {model.ID}");
                var dic = new Dictionary<string,object>();
                model.Status = success?2:3;
                model.GetValues(dic);
                dic.Remove("ID");
                helper.Insert("SMSSendQueue", dic);

                tran.Commit();
            }
            catch (Exception)
            {
                tran.Rollback();
            }
            
        }

        public void DeleteSMS(int smsID)
        {
            helper.Delete("SMSSendQueue", $" ID = {smsID}");

        }

        public IEnumerable<SMSSendQueueModel> GetSMSSend(int? containerId,string toName,string toPhone,int? status,DateTime? beginTime,DateTime? endTime,string smsContent)
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

            return helper.SelectDataTable("select * from SMSSendQueue "+whereStr+" union all select * from SMSSendRecord "+whereStr).Select().Select(x=>(SMSSendQueueModel)new SMSSendQueueModel().SetData(x));

        }


        



    }
}
