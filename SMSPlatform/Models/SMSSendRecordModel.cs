﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class SMSSendRecordModel:DataRowModel
    {
        

        public string SMSContent
        {
            get { return dataPool["SMSContent"] + ""; }
            set { dataPool["SMSContent"] = value; }
        }

        public int TemplateID
        {
            get { return (int)dataPool["TemplateID"]; }
            set { dataPool["TemplateID"] = value; }
        }
        public string TemplateContent
        {
            get { return dataPool["TemplateContent"] + ""; }
            set { dataPool["TemplateContent"] = value; }
        }

        public string InnerTemplateContent
        {
            get { return dataPool["InnerTemplateContent"] + ""; }
            set { dataPool["InnerTemplateContent"] = value; }
        }


        public string ToPhoneNumber
        {
            get { return dataPool["ToPhoneNumber"] + ""; }
            set { dataPool["ToPhoneNumber"] = value; }
        }

        public string ToName
        {
            get { return dataPool["ToName"] + ""; }
            set { dataPool["ToName"] = value; }
        }

        public DateTime? SendTime
        {
            get { return (DateTime)dataPool["SendTime"]; }
            set { dataPool["SendTime"] = value; }
        }

        public int Status
        {
            get { return (int)dataPool["Status"]; }
            set { dataPool["Status"] = value; }
        }

        public int TocontactorID
        {
            get { return (int)dataPool["TocontactorID"]; }
            set { dataPool["TocontactorID"] = value; }
        }

        public DateTime CreateTime
        {
            get { return (DateTime)dataPool["CreateTime"]; }
            set { dataPool["CreateTime"] = value; }
        }

        public int? ContainerID
        {
            get { return (int?)dataPool["ContainerID"]; }
            set { dataPool["ContainerID"] = value; }
        }
    }
}
