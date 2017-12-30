using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Services
{
    public class DBService
    {

    }


    public class ConnectionStringUtility
    {
        public static string DefaultConnectionStrings
        {
            get
            {
                ConfigurationManager.RefreshSection("connectionStrings");
                return ConfigurationManager.ConnectionStrings["default"].ConnectionString;
            }
        }
    }

}
