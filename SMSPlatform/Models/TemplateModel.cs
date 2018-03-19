using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class TemplateModel : DataRowModel
    {
        

        public string TemplateName
        {
            get { return dataPool["TemplateName"] + ""; }
            set { dataPool["TemplateName"] = value; }
        }
        public string TemplateContent
        {
            get { return dataPool["TemplateContent"] + ""; }
            set { dataPool["TemplateContent"] = value; }
        }

    }
}
