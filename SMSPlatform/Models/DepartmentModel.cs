using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class DepartmentModel:DataRowModel
    {
        public int? ID
        {
            get { return dataPool["ID"]==null?null:new int?((int) dataPool["ID"]); }
            set { dataPool["ID"] = value; }
        }

        public string DName
        {
            get { return dataPool["DName"] + ""; }
            set { dataPool["DName"] = value; }
        }

        public int? PDID
        {
            get { return dataPool["PDID"] == null ? null : new int?((int)dataPool["PDID"]); }
            set { dataPool["PDID"] = value; }
        }

        public int? DIndex
        {
            get { return dataPool["DIndex"] == null ? null : new int?((int)dataPool["DIndex"]); }
            set { dataPool["DIndex"] = value; }
        }

        public int? LevelIndex
        {
            get { return dataPool["LevelIndex"] == null ? null : new int?((int)dataPool["LevelIndex"]); }
            set { dataPool["LevelIndex"] = value; }
        }

    }
}
