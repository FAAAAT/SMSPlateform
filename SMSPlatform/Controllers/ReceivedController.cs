using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DataBaseAccessHelper;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class ReceivedController:ApiController
    {
        [HttpGet]
        public IHttpActionResult GetReceivedSMS(string phone,string date,int? pageSize = null,int? pageIndex = null)
        {
            SqlHelper helper = new SqlHelper();
            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            helper.SetConnection(conn);
            try
            {
                DateTime? startTime = null;
                DateTime? endTime = null;
                if (!string.IsNullOrWhiteSpace(date))
                {
                    startTime = DateTime.Parse(date.Split('-')[0]);
                    endTime = DateTime.Parse(date.Split('-')[1]);
                }

                var initWhere = " where ";
                var whereStr = initWhere;
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    whereStr += (whereStr == initWhere ? "" : " and ") + $"PhoneNumber like '%{phone}%'";
                }
                if (startTime!=null)
                {
                    whereStr += (whereStr == initWhere ? "" : " and ") + $"ReceiveDate >= '{startTime:yyyy-MM-dd}'";

                }
                if (endTime!=null)
                {
                    
                    whereStr += (whereStr == initWhere ? "" : " and ") + $"ReceiveDate <= '{endTime.Value.AddDays(1):yyyy-MM-dd}'";

                }


                var smss = helper.SelectDataTable("select * from ReceivedSMSRecord "+whereStr + " order by ReceiveDate desc").Select()
                    .Select(x => new ReceivedSMSRecordModel().SetData(x) as ReceivedSMSRecordModel);
                var totalCount = smss.Count();
                if (pageSize.HasValue&&pageIndex.HasValue)
                {
                    smss = smss.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value);

                }

                return Json(new ReturnResult()
                {
                    success = true,
                    status = 200,
                    data = smss,

                    total = totalCount,

                });

            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    status = 500,
#if DEBUG
                    msg = e.ToString(),

#else
                    msg = "内部错误 请联系系统管理员",
#endif
                });
            }
        }
    }
}
