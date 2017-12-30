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
    public class DynamicTemplateController:ApiController
    {

        [HttpGet]
        public IHttpActionResult HeaderAndFooter()
        {
            try
            {
                HeaderFooterTemplateModel model = new HeaderFooterTemplateModel();
                model.footer = "技术支持 天津恒创伟业科技有限公司";
                model.menus.Add(new MenuItem() { name = "测试", href = "www.baidu.com" });
                model.applicationName = "天津商业大学短信平台";
                return Json(new ReturnResult()
                {
                    success = true,
                    status = 200,
                    data = model
                });
            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    status = 500,
                    msg = e+""
                });

            }
        

        }







    }
}
    