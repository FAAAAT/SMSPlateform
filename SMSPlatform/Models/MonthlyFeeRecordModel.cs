using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class MonthlyFeeRecordModel:DataRowModel
    {
        

        public int MonthLimitRecord
        {
            get { return (int) dataPool["MonthLimitRecord"]; }
            set { dataPool["MonthLimitRecord"] = value; }
        }

        public int SendCount
        {
            get { return (int)dataPool["SendCount"]; }
            set { dataPool["SendCount"] = value; }
        }

        public string PhoneNumber
        {
            get { return dataPool["PhoneNumber"] + ""; }
            set { dataPool["PhoneNumber"] = value; }
        }

        public int Month
        {
            get { return (int) dataPool["Month"]; }
            set { dataPool["Month"] = value; }
        }

        public int Year
        {
            get { return (int) dataPool["Year"]; }
            set { dataPool["Year"] = value; }
        }

    }
}
