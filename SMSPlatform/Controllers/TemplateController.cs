using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using DataBaseAccessHelper;
using Newtonsoft.Json;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    [LymiAuthorize(Users = "admin")]
    public class TemplateController : ApiController
    {
        private SqlHelper helper;

        public TemplateController()
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            helper = new SqlHelper();
            helper.SetConnection(conn);
        }

        [HttpGet]
        public IHttpActionResult GetTemplates(string name)
        {
            try
            {
                TemplateService service = new TemplateService(helper);
                var temps = service.GetTemplates(name);
                return Json(new ReturnResult()
                {
                    success = true,
                    data=temps,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex+"",
                    status = 500
                });
            }
            
          
        }

        [HttpGet]
        public IHttpActionResult GetTemplate(int id)
        {
            try
            {
                TemplateService service = new TemplateService(helper);
                var temp = service.GetTemplate(id);
                return Json(new ReturnResult()
                {
                    success = true,
                    data = temp,
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
        public IHttpActionResult DeleteTemplate(int id)
        {
            try
            {
                TemplateService service = new TemplateService(helper);
                service.DeleteTemplate(id);
                return Json(new ReturnResult()
                {
                    success = true,
                    msg="删除成功",
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
        public IHttpActionResult AddTemplate([FromUri]TemplateModel model)
        {
            try
            {
                model.ID = null;
                if (model.ID!=null)
                {
                    return Json(new ReturnResult()
                    {
                        msg = "ID can not be setted",
                        success = false
                    });
                }
                TemplateService service = new TemplateService(helper);
                service.AddTemplate(model);
                return Json(new ReturnResult()
                {
                    success = true,
                    msg = "添加成功",
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
        public IHttpActionResult UpdateTemplate([FromUri]TemplateModel model)
        {
            try
            {
                if (model.ID == null)
                {
                    return Json(new ReturnResult()
                    {
                        msg = "ID can not be null",
                        success = false,
                        status = 200
                        
                    });
                }
                TemplateService service = new TemplateService(helper);
                service.UpdateTemplate(model);
                return Json(new ReturnResult()
                {
                    success = true,
                    msg = "更新成功",
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                helper?.Dispose();
            }
           

            base.Dispose(disposing);
        }
    }
}