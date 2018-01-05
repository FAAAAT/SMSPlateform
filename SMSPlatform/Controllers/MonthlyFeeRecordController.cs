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
    public class MonthlyFeeRecordController : ApiController
    {
        private SqlHelper helper;

        public MonthlyFeeRecordController()
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            helper = new SqlHelper();
            helper.SetConnection(conn);
        }

        [HttpGet]
        public IHttpActionResult GetFeeRecords(string phone, int? month, int? year)
        {
            try
            {
                MonthlyFeeService service = new MonthlyFeeService(helper);
                var datas = service.GetRecords(phone, month, year);
                return Json(new ReturnResult()
                {
                    data = datas,
                    success = true,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),
#else
                    msg="内部错误",
#endif
                    success = false,
                    status = 500
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            helper.Dispose();
            base.Dispose(disposing);
        }
    }
}
