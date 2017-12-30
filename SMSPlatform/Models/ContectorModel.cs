using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class ContectorModel:DataRowModel
    {
        public int ID {
            get { return (int) dataPool["ID"]; }
            set { dataPool["ID"] = value; }
        }

//        public string ContectorName
//        {
//            
//        }
        public string PhoneNumber;
        public string Remark;

    }

}
