using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class contactorModel : DataRowModel
    {
        public int? ID
        {
            get { return (int?)dataPool["ID"]; }
            set { dataPool["ID"] = value; }
        }

        public string CongractorName
        {
            get { return dataPool["CongractorName"] +""; }
            set { dataPool["CongractorName"] = value; }
        }

        public string PhoneNumber
        {
            get { return dataPool["PhoneNumber"] + ""; }
            set { dataPool["PhoneNumber"] = value; }
        }

        public string Remark
        {
            get { return dataPool["Remark"] + ""; }
            set { dataPool["Remark"] = value; }
        }
    }
}
