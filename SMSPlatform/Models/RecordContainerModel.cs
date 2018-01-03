﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SMSPlatform.Models
{
    public class RecordContainerModel:DataRowModel
    {
        public int? ID {
            get { return (int?) dataPool["ID"]; }
            set { dataPool["ID"] = value; }
        }

        public string ContainerName
        {
            get { return dataPool["ContainerName"] + ""; }
            set { dataPool["ContainerName"] = value; }
        }

        public DateTime CreateTime
        {
            get { return (DateTime) dataPool["CreateTime"]; }
            set { dataPool["CreateTime"] = value; }
        }

        public string[] SIMsPhone
        {
            get { return JsonConvert.DeserializeObject<string[]>(dataPool["SIMsPhone"] + ""); }
            set { dataPool["SIMsPhone"] = JsonConvert.SerializeObject(value); }
        }


    }
}