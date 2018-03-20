using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
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
            if ( actionContext.RequestContext.Principal == null || actionContext.RequestContext.Principal.Identity.IsAuthenticated)
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

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            IPrincipal user = actionContext.ControllerContext.RequestContext.Principal;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }
          

            if (!string.IsNullOrWhiteSpace(Users)&&Users.Split(',').Length > 0 
                && !Users.Split(',').Any(x=>(user.Identity as ClaimsIdentity).Claims.Where(y=>y.Type == "UserName").Select(y=>y.Value).Contains(x, StringComparer.OrdinalIgnoreCase)))
            {
                if (Users.Split(',').Contains("*"))
                {
                    return true;
                }
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Roles) &&Roles.Split(',').Length > 0 
                && !Roles.Split(',').Any(x => (user.Identity as ClaimsIdentity).Claims.Where(y => y.Type == "RoleName").Select(y => y.Value).Contains(x, StringComparer.OrdinalIgnoreCase)))
            {
                if (Roles.Split(',').Contains("*"))
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}
