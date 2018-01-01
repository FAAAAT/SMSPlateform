using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class Select2Model
    {
        public IEnumerable<Select2Item> results { get; set; }
    }

    public class Select2Item
    {
        public int id { get; set; }
        public string text { get; set; }
        public bool selected { get; set; } = false;
        public bool disabled { get; set; } = false;

    }

    public class Select2QueryModel
    {
        public string term { get; set; }
        public string q { get; set; }
        public string _type { get; set; }
        public int? page { get; set; }
    }
}
