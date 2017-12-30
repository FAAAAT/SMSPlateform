using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class UserService
    {
        

    }

    public static class UserServiceExtension
    {
        public static AuthenticationCookieModel GetAuthModel(this HttpActionContext context)
        {
            if (context == null)
            {
                return null;
            }
            return context.ActionDescriptor.Properties["AuthenModel"] as AuthenticationCookieModel;

        }
    }
}
