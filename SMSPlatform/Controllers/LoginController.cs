using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using DataBaseAccessHelper;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class LoginController:ApiController
    {
        [HttpGet]
        [HttpPost]
        public IHttpActionResult Login(string userName,string password)
        {
            
            var connection = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            connection.Open();
            using (var helper = new SqlHelper())
            {
                helper.SetConnection(connection);

                var datas = helper.SelectDataTable("select * from [User] where UserName = '" + userName+"'").Select();
                if (!datas.Any())
                {
                    return Json(new ReturnResult()
                    {
                        msg = "用户名不存在",
                        success = false,
                    });
                }
                var userData = datas.SingleOrDefault(x => x["Password"] + "" == password);
                if (userData == null)
                {
                    return Json(new ReturnResult()
                    {
                        msg = "密码错误",
                        success = false
                    });
                }
                AuthenticationCookieModel model = new AuthenticationCookieModel();

                model.UserID = userData["ID"] + "";
                model.RoleID = helper.SelectList<int>("UserRoleReference", "RoleID", " UserID = " + model.UserID).Select(x => x + "");
                model.UserName = userData["UserName"] + "";
                model.RoleName = helper.SelectList<string>("Role", "RoleName",
                    model.RoleID.Count()==0?" 1=0 ":" ID in (" + string.Join(",", model.RoleID) + ")");


                CancellationToken token = new CancellationToken();
                var responseMsg = Json(new ReturnResult()
                {
                    msg = "验证成功",
                    success = true,
                    status = 301,
                    data = "/Pages/Index.html"
                }).ExecuteAsync(token).GetAwaiter().GetResult();

                responseMsg.SignIn(model);

                return new CustomHttpActionResult() { Response = responseMsg };
            }
           
        }

        [HttpGet]
        [HttpPost]
        public IHttpActionResult Logout()
        {
            CancellationToken token = new CancellationToken();
            var responseMsg = Json(new ReturnResult()
            {
                msg = "注销成功",
                success = true,
                status = 301,
                data = "/Pages/login.html"
            }).ExecuteAsync(token).GetAwaiter().GetResult();
            
            responseMsg.SignOut(ActionContext.ActionDescriptor.Properties["AuthenModel"] as AuthenticationCookieModel);

            return new CustomHttpActionResult(){Response = responseMsg};

        }

        [HttpGet]
        [HttpPost]
        [LymiAuthorize(Users = "admin")]
        public IHttpActionResult GetUserInfo()
        {
            return Json(ActionContext.GetAuthModel());
        }


    }

    public class CustomHttpActionResult : IHttpActionResult
    {
        public HttpResponseMessage Response { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Response);
        }

    }
}
