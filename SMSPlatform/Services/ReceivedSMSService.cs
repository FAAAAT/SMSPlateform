using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataBaseAccessHelper;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class ReceivedSMSService
    {
        private SqlHelper helper;

        public ReceivedSMSService(SqlHelper helper)
        {
            this.helper = helper;
        }

        public List<int> RestoreReceivedSMS(List<Dictionary<string, object>> dics)
        {
            var result
                 = new List<int>();
            var contactors = helper.SelectDataTable("select * from Contactor").Select().Select(x=>new contactorModel().SetData(x) as contactorModel);
            foreach (Dictionary<string, object> dic in dics)
            {
                ReceivedSMSRecordModel model = new ReceivedSMSRecordModel();

                var phoneNumber  = (dic["PhoneNumber"] + "").StartsWith("86")
                    ? (dic["PhoneNumber"] + "").Substring(2, 11)
                    : (dic["PhoneNumber"] + "");
              


                var contactor = contactors.SingleOrDefault(x => x.PhoneNumber == phoneNumber);
                model.ContactorID = contactor?.ID;
                model.ContactorName = contactor?.ContactorName;
                model.SMSContent = dic["SMSContent"]+"";
                model.ReceiveDate = (DateTime) dic["ReceivedTime"];

               
                model.PhoneNumber =phoneNumber;
                var dataDic = new Dictionary<string,object>();
                model.GetValues(dataDic);
                result.Add((int)helper.Insert("ReceivedSMSRecord", dataDic, "OUTPUT inserted.ID"));
            }
            return result;
        }

    }
}
