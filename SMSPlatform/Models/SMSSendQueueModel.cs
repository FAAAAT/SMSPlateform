using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class SMSSendQueueModel:DataRowModel
    {
        public int? ID {
            get { return (int?) dataPool["ID"]; }
            set { dataPool["ID"] = value; }
        }

        public string SMSContent
        {
            get { return dataPool["SMSContent"]+""; }
            set { dataPool["SMSContent"] = value; }
        }

        public DateTime CreateTime
        {
            get { return (DateTime) dataPool["CreateTime"]; }
            set { dataPool["CreateTime"] = value; }
        }

        public int ToContractorID
        {
            get { return (int) dataPool["ToContractorID"]; }
            set { dataPool["ToContractorID"] = value; }
        }

        public int Status
        {
            get { return (int)dataPool["Status"]; }
            set { dataPool["Status"] = value; }
        }

        public int? ContainerID
        {
            get { return (int?)dataPool["ContainerID"]; }
            set { dataPool["ContainerID"] = value; }
        }

    }
}
