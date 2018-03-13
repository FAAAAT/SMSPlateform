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
    public class DynamicTemplateController : ApiController
    {

        [HttpGet]
        public IHttpActionResult HeaderAndFooter()
        {
            try
            {
                HeaderFooterTemplateModel model = new HeaderFooterTemplateModel();
                model.footer = "技术支持 天津恒创伟业科技有限公司";
                model.menus.Add(new MenuItem()
                {
                    name = "部门管理",
                    children = new List<MenuItem>()
                    {
                        new MenuItem(){name="部门管理",href="/pages/department.html",},
                        new MenuItem(){name = "部门标签管理",href = "/pages/departmenttag.html"}
                    },
                });

                model.menus.Add(new MenuItem()
                {
                    name = "标签管理",
                    href = "/pages/tag.html"
                });

                model.menus.Add(new MenuItem() { name = "联系人", href = "/pages/contactors.html", });


                model.menus.Add(new MenuItem()
                {
                    name = "模板管理",
                    href = "/pages/template.html"
                });


               

               

                

                model.menus.Add(new MenuItem()
                {
                    name = "任务管理",
                    children = new List<MenuItem>()
                    {
                        new MenuItem()
                        {
                            name = "创建任务",
                            href = "/pages/wizard.html"
                        },
                        new MenuItem()
                        {
                            name = "任务列表",
                            href = "/pages/sendqueue.html",
                        },
                        new MenuItem()
                        {
                            name = "历史记录",
                            href = "/pages/sendrecord.html",
                        }

                    }
                });

               
              

                model.menus.Add(new MenuItem()
                {
                    name="收件箱",
                    href="/pages/Receiver.html"
                });

                model.menus.Add(new MenuItem()
                {
                    name = "资费列表",
                    href = "/pages/MonthlyFeeRecord.html",
                });

                model.menus.Add(new MenuItem()
                {
                    name = "资费设置",
                    href = "/pages/MonthlyLimitSettings.html"
                });


                model.menus.Add(new MenuItem()
                {
                    name = "SIM卡设置",
                    href = "/pages/SIMCardManagement.html",
                });

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
                    msg = e + ""
                });

            }


        }







    }
}
