using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using DataBaseAccessHelper;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class SettingsController : ApiController
    {
        private SqlHelper helper;

        public SettingsController()
        {
            helper = new SqlHelper();
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            helper.SetConnection(conn);
        }

        [HttpGet]
        public IHttpActionResult GetSettings(string phone)
        {
            try
            {
                var whereStr = " where 1=1 ";
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    whereStr += $" and phonenumber like '%{phone}%'";
                }

                var models = helper.SelectDataTable("select * from systemsettings " + whereStr)
                    .Select()
                    .Select(x => (SystemSettingsModel) new SystemSettingsModel().SetData(x));
                return Json(new ReturnResult()
                {
                    success = true,
                    data = models,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex + "",
                    status = 500
                });
            }

        }

        [HttpGet]
        public IHttpActionResult AddSettings([FromUri] SystemSettingsModel model)
        {
            try
            {
                SystemSettingsService service = new SystemSettingsService(helper);
                service.AddPhoneLimitSettings(model);
                return Json(new ReturnResult()
                {
                    msg = "添加成功",
                    success = true,
                    status = 200,
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    msg = ex + "",
                    success = false,
                    status = 500
                });
            }
        }

        [HttpGet]
        public IHttpActionResult UpdateSettings([FromUri] SystemSettingsModel model)
        {
            try
            {
                SystemSettingsService service = new SystemSettingsService(helper);
                service.UpdatePhoneLimitSettings(model);
                return Json(new ReturnResult()
                {
                    msg = "更新成功",
                    success = true,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex + "",
#else
                    msg = "更新失败",
#endif
                    success = false,
                    status = 500
                });
            }
        }

        [HttpGet]
        public IHttpActionResult DeleteSettings(int id)
        {
            try
            {
                var service = new SystemSettingsService(helper);
                service.DeletePhoneLimitSettings(id);
                return Json(new ReturnResult()
                {
                    msg = "删除成功",
                    success = true,
                    status = 200,
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex+"",
#else 
                    msg = "删除失败",
#endif
                    success = false,
                    status = 500
                });
            }
        }

        [HttpGet]
        public IHttpActionResult GetSetting(int id)
        {
            try
            {
                var model = helper.SelectDataTable("select * from systemsettings where ID = " + id).Select().Select(x => (SystemSettingsModel)new SystemSettingsModel().SetData(x)).SingleOrDefault();
                return Json(new ReturnResult()
                {
                    success = true,
                    data = model,
                    status = 200
                });
            }
            catch (Exception e)
            {

                return Json(new ReturnResult()
                {
#if true
                    msg = e+"",

#else
                    msg = "获取失败",
#endif
                    success = false,
                    status = 500,

                });
            }


        }



        protected override void Dispose(bool disposing)
        {
            helper.Dispose();

            //true 释放托管资源
            base.Dispose(disposing);
        }
    }
}
