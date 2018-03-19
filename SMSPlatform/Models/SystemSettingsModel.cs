using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class SystemSettingsModel : DataRowModel
    {


        public int? MonthTotalCountLimit
        {
            get { return (int?)dataPool["MonthTotalCountLimit"]; }
            set { dataPool["MonthTotalCountLimit"] = value; }
        }

        public string PhoneNumber
        {
            get { return dataPool["PhoneNumber"] + ""; }
            set { dataPool["PhoneNumber"] = value; }
        }



    }
}
