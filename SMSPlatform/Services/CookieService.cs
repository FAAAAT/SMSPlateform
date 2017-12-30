using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class CookieService
    {
//        public static string AuthenticationHeaderName = "Userinfo";
        public static string AuthenticationCookieDomain = "";
        public static string Path = "/";
        
        public static DateTimeOffset AuthenticationCookieExpire {
            get
            {
                return DateTimeOffset.Now.AddDays(7);
            }
        }



    }

    public static class CookieServiceExtensions
    {
        public static CookieHeaderValue GetAuthenCookie(this AuthenticationCookieModel model, string headerName)
        {
            var nvc = new NameValueCollection();

            var pinfos = model.GetType().GetProperties();
            foreach (var VARIABLE in pinfos)
            {
                nvc.Add(VARIABLE.Name, JsonConvert.SerializeObject(VARIABLE.GetValue(model)));
            }
            CookieHeaderValue value = new CookieHeaderValue(headerName,nvc);
            if (!string.IsNullOrWhiteSpace(CookieService.AuthenticationCookieDomain))
            {
                value.Domain = CookieService.AuthenticationCookieDomain;
            }
            if (!string.IsNullOrWhiteSpace(CookieService.Path))
            {
                value.Path = CookieService.Path;
            }
            value.Expires = CookieService.AuthenticationCookieExpire;
            

            return value;
        }

        public static AuthenticationCookieModel GetAuthenticationCookieModel(this CookieHeaderValue cookie,string headerName)
        {
            var result = new AuthenticationCookieModel();
            var pinfos = typeof(AuthenticationCookieModel).GetProperties();
            var k = cookie.Cookies.SingleOrDefault(x => x.Name == headerName);
            if (k==null)
            {
                return null;
            }
            foreach (PropertyInfo pInfo in pinfos)
            {   
                pInfo.SetValue(result,JsonConvert.DeserializeObject(k.Values[pInfo.Name],pInfo.PropertyType));
            }
            return result;
        }
    }
}
