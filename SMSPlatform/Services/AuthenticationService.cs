using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Security;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class AuthenticationService
    {
        
    }

    public static class AuthenticationServiceExtensions
    {
        public static string HeaderName;
        public static void SignIn(this HttpResponseMessage message,AuthenticationCookieModel model)
        {
            if (model==null)
            {
                return;
            }
            var cookie = model.GetAuthenCookie(HeaderName);
            message.Headers.AddCookies(new List<CookieHeaderValue>(){cookie});
        }

        public static void SignOut(this HttpResponseMessage message,AuthenticationCookieModel model)
        {
            if (model == null)
            {
                return;
            }
            var cookie = model.GetAuthenCookie(HeaderName);
            cookie.Expires = DateTimeOffset.Now;
            
            message.Headers.AddCookies(new List<CookieHeaderValue>() { cookie });
        }

        public static void Authenticate(this HttpRequestMessage message)
        {
            var cookieHeaderValue =  message.Headers.GetCookies(HeaderName).FirstOrDefault();
            var model =cookieHeaderValue.GetAuthenticationCookieModel(HeaderName);
            var claims = new List<Claim>() {new Claim("userID", model.UserID)};
            foreach (string s in model.RoleID)
            {
                claims.Add(new Claim("RoleID",s));
            }

            var identity = new ClaimsIdentity();
            var iprinciple = new ClaimsPrincipal(identity);


            message.Properties.Add("ClaimsPrinciple",iprinciple);
            
        }

//        public static void Challenge(this HttpResponseMessage )


    }
}
