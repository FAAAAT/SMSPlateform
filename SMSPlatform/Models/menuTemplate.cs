using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class HeaderFooterTemplateModel
    {
        public string footer { get; set; }

        public List<MenuItem> menus { get; set; } = new List<MenuItem>();

        public string applicationName { get; set; }
    }

    public class MenuItem
    {
        public string name { get; set; }
        public string href { get; set; }
        public IEnumerable<MenuItem> children { get; set; }
    
}
}

