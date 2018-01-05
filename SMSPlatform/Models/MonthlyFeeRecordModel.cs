using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class MonthlyFeeRecordModel:DataRowModel
    {
        public int? ID
        {
            get
            {
                return dataPool["ID"] == DBNull.Value ? null : new int?((int) dataPool["ID"]);
                
            }
            set { dataPool["ID"] = value; }
        }


        public int MonthlySMSCountLimitRecord
        {
            get
            {
                return (int)dataPool["MonthlySMSCountLimitRecord"];

            }
            set { dataPool["MonthlySMSCountLimitRecord"] = value; }
        }

        public int Month
        {
            get
            {
                return (int)dataPool["Month"] ;

            }
            set { dataPool["Month"] = value; }
        }

        public int MonthlySendSMSCount
        {
            get
            {
                return (int)dataPool["MonthlySendSMSCount"] ;

            }
            set { dataPool["MonthlySendSMSCount"] = value; }
        }

        public int Year
        {
            get
            {
                return (int)dataPool["Year"];

            }
            set { dataPool["Year"] = value; }
        }

        public string PhoneNumber
        {
            get { return dataPool["PhoneNumber"] + ""; }
            set { dataPool["PhoneNumber"] = value; }
        }
    }
}
