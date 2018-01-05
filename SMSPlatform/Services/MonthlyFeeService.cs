using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAccessHelper;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class MonthlyFeeService
    {
        private SqlHelper helper;

        public MonthlyFeeService(SqlHelper helper)
        {
            this.helper = helper;
        }

        public IEnumerable<MonthlyFeeRecordModel> GetRecords(string phone,int?month,int? year)
        {
            var whereStr = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(phone))
            {
                whereStr += $" and PhoneNumber like '%{phone}%' ";
            }

            if (month.HasValue)
            {
                whereStr += $" and Month = {month}";
            }

            if (year.HasValue)
            {
                whereStr += $" and Year = '{year}'";
            }

            return helper.SelectDataTable("select * from MonthlyFeeRecord " + whereStr).Select().Select(x=>new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel);


        }
    }
}
