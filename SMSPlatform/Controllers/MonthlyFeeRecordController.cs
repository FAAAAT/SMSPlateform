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
        public IHttpActionResult GetMonthlyFeeRecords(string phone, string date)
        {
            try
            {
                var range = date?.Split('-');
                DateTime? startTime = null;
                DateTime? endTime = null;
                if (!string.IsNullOrWhiteSpace(date))
                {
                    startTime = DateTime.Parse(range[0]);
                    endTime = DateTime.Parse(range[1]);
                }
                

                FeeService service = new FeeService(helper);
                var datas = service.GetMonthlyFeeRecordModels(phone, startTime,endTime);
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

        [HttpGet]
        public IHttpActionResult GetDailyFeeRecords(string phone, string date,int? monthlyFeeId = null)
        {
            try
            {
                var range = date?.Split('-');
                DateTime? startTime = null;
                DateTime? endTime = null;
                if (!string.IsNullOrWhiteSpace(date))
                {
                    startTime = DateTime.Parse(range[0]);
                    endTime = DateTime.Parse(range[1]);
                }
                

                FeeService service = new FeeService(helper);
                var datas = service.GetDailyFeeRecordModels(phone, startTime,endTime,monthlyFeeId);
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



        [HttpGet]
        public IHttpActionResult GetMonthlyFeeRecordQueryParas()
        {
            try
            {
                var fees = helper.SelectDataTable("select * from MonthlyFeeRecord").Select().Select(x => new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel);
                var phones = new Select2Model();
                phones.results = fees.Select(x => new Select2Item() { id = x.PhoneNumber.GetHashCode(), text = x.PhoneNumber });

                return Json(new ReturnResult()
                {
                    success = true,
                    data = new { phones },
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
#if DEBUG
                    msg = ex.ToString(),

#else
                    msg = "发生错误,请联系管理员",
#endif
                    status = 500,
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
