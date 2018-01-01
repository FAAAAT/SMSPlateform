using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Owin.Infrastructure;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Security;
using Newtonsoft.Json;
using SMSPlatform.Controllers;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Filters
{
    public class AuthenticationFilter : IAuthenticationFilter
    {
        public bool AllowMultiple { get; } = false;
        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {

            var authAttrs = actionContext.ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>();
            if (authAttrs.Any())
            {
                var authed = false;
                var authModel = actionContext.Request.Headers.GetCookies(AuthenticationServiceExtensions.HeaderName).SingleOrDefault()
                    ?.GetAuthenticationCookieModel(AuthenticationServiceExtensions.HeaderName);
                if (authModel == null)
                {
                    goto noAuthed;

                }

                foreach (AuthorizeAttribute aa in authAttrs)
                {
                    if (!string.IsNullOrWhiteSpace(aa.Users))
                    {
                        authed = aa.Users == "*" || aa.Users.Contains(authModel.UserID);
                    }
                    if (!string.IsNullOrWhiteSpace(aa.Roles))
                    {
                        authed = aa.Roles == "*" || authModel.RoleID.Any(x => aa.Roles.Contains(x));
                    }
                }

                noAuthed:
                if (!authed)
                {
                    return new RedirectResult(new Uri("", UriKind.Relative), actionContext.Request).ExecuteAsync(cancellationToken);
                }
                else
                {
                    actionContext.ActionDescriptor.Properties.TryAdd("AuthenModel", authModel);
                }

            }
            return continuation();
        }


        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var authAttrs = context.ActionContext.ActionDescriptor.GetCustomAttributes<LymiAuthorizeAttribute>();
            var str = context.Request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var authModel = context.Request.Headers.GetCookies(AuthenticationServiceExtensions.HeaderName).SingleOrDefault()?.GetAuthenticationCookieModel(AuthenticationServiceExtensions.HeaderName);
            if (authModel != null)
            {
                context.ActionContext.ActionDescriptor.Properties.TryAdd("AuthenModel",authModel);
                var claims = new List<Claim>() { new Claim("UserName", authModel.UserName) };
                var roles = authModel.RoleName;
                claims.AddRange(roles.Select(x => new Claim("RoleName", x)));
                var identity = new ClaimsIdentity(claims, "SMSAuthentication", "UserName", "RoleName");
                context.Principal = new GenericPrincipal(identity,roles.ToArray());
                context.ActionContext.RequestContext.Principal = context.Principal;
                foreach (LymiAuthorizeAttribute authorizeAttribute in authAttrs)
                {
                    authorizeAttribute.OnAuthorization(context.ActionContext);
                }
            }
            
            
                        
            return Task.CompletedTask;

        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var authAttrs = context.ActionContext.ActionDescriptor.GetCustomAttributes<LymiAuthorizeAttribute>();
            var principal = context.ActionContext.RequestContext.Principal;
            //            context.ActionContext.RequestContext.Principal.Identity.IsAuthenticated
            if ((principal==null||!principal.Identity.IsAuthenticated) && authAttrs.Any())
            {
                context.Result = new JsonResult<ReturnResult>(new ReturnResult()
                {
                    status = 302,
                    data = "/pages/login.html"
                }, new JsonSerializerSettings(), Encoding.UTF8, context.Request);
            }

           

//            return context.Result.ExecuteAsync(cancellationToken);
            return Task.CompletedTask;
        }


    }
}
