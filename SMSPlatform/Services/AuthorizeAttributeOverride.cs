using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Newtonsoft.Json;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class LymiAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                actionContext.Response = new JsonResult<ReturnResult>(new ReturnResult()
                {
                    msg = "没有足够的权限访问此功能",
                    status = 401,

                },new JsonSerializerSettings(), Encoding.UTF8,actionContext.Request).ExecuteAsync(new CancellationToken()).Result;
            }
            else
            {
                base.HandleUnauthorizedRequest(actionContext);
            }
        }
    }
}
