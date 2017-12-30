using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class AuthenticationCookieModel
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        public IEnumerable<string> RoleID { get; set; }

        public IEnumerable<string>RoleName { get; set; }

    }

    
}
