using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class WizardUpdateDataModel
    {
        public int? ID { get; set; }
        public int[] selectedContactors { get; set; }
        public ContainerData containerData { get; set; }
        public int templateID { get; set; }
        public string actualContent { get; set; }
    }

    public class ContainerData
    {
        public string containerName { get; set; }
        public string[] simPhones { get; set; }
    }

    
}
