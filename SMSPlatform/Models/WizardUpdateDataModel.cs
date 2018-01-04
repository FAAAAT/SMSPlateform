using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class WizardUpdateDataModel
    {
        public int[] selectedContactors { get; set; }
        public ContainerData containerData { get; set; }
    }

    public class ContainerData
    {
        public string containerName { get; set; }
        public string[] simPhones { get; set; }
    }
}
