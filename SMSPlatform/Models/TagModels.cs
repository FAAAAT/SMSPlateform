using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class TagModel:DataRowModel
    {
        

        public string TagName
        {
            get { return dataPool["TagName"] + ""; }
            set { dataPool["TagName"] = value; }
        }
    }


}
