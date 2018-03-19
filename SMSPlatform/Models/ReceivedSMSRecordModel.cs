using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    class ReceivedSMSRecordModel:DataRowModel
    {
        

        public string ContactorName
        {
            get { return dataPool["ContactorName"] + ""; }
            set
            {
                dataPool["ContactorName"] = value
                    ;
            }
        }

        public string PhoneNumber
        {
            get { return dataPool["PhoneNumber"] + ""; }
            set { dataPool["PhoneNumber"] = value; }
        }

        public DateTime? ReceiveDate
        {
            get { return dataPool["ReceiveDate"] == null ? null : new DateTime?((DateTime)dataPool["ReceiveDate"]); }
            set { dataPool["ReceiveDate"] = value; }
        }

        public string SMSContent
        {
            get { return dataPool["SMSContent"] + ""; }
            set { dataPool["SMSContent"] = value; }
        }

        public int? ContactorID
        {
            get { return dataPool["ContactorID"] == null||dataPool["ContactorID"] == DBNull.Value ? null : new int?((int)dataPool["ContactorID"]); }
            set { dataPool["ContactorID"] = value; }
        }
    }
}
