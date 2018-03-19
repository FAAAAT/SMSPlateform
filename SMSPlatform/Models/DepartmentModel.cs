using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class DepartmentModel:DataRowModel
    {
        

        public string DName
        {
            get { return dataPool["DName"] + ""; }
            set { dataPool["DName"] = value; }
        }

        public int? PDID
        {
            get { return dataPool["PDID"] == DBNull.Value||string.IsNullOrWhiteSpace(dataPool["PDID"]+"") ? null : new int?((int)dataPool["PDID"]); }
            set { dataPool["PDID"] = value; }
        }

        public int? DIndex
        {
            get { return dataPool["DIndex"] == DBNull.Value ? null : new int?((int)dataPool["DIndex"]); }
            set { dataPool["DIndex"] = value; }
        }

        public int? LevelIndex
        {
            get { return dataPool["LevelIndex"] == DBNull.Value ? null : new int?((int)dataPool["LevelIndex"]); }
            set { dataPool["LevelIndex"] = value; }
        }

    }
}
