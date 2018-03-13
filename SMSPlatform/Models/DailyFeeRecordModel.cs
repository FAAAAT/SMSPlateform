using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class DailyFeeRecordModel:DataRowModel
    {
        public int? ID
        {
            get { return dataPool["ID"] == DBNull.Value ? null : new int?((int) dataPool["ID"]); }
            set { dataPool["ID"] = value; }
        }

        public string PhoneNumber
        {
            get { return dataPool["PhoneNumber"] + ""; }
            set { dataPool["PhoneNumber"] = value; }
        }

        public int SendCount
        {
            get { return (int)dataPool["SendCount"]; }
            set { dataPool["SendCount"] = value; }
        }

        public DateTime Date
        {
            get { return (DateTime) dataPool["Date"]; }
            set { dataPool["Date"] = value; }
        }

        public int? MonthlyID
        {
            get { return dataPool["MonthlyID"] == DBNull.Value ? null : new int?((int)dataPool["MonthlyID"]); }
            set { dataPool["MonthlyID"] = value; }
        }

    }
}
