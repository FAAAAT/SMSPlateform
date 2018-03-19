using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public class UserModel:DataRowModel
    {
        public string UserName
        {
            get { return dataPool["UserName"] + ""; }
            set { dataPool["UserName"] = value; }
        }
        public string Password
        {
            get { return dataPool["Password"] + ""; }
            set { dataPool["Password"] = value; }
        }

        public string Name {
            get { return dataPool["Name"] + ""; }
            set { dataPool["Name"] = value; }
        }

    }


    public class RoleModel : DataRowModel
    {
        public string RoleName
        {
            get { return dataPool["RoleName"] + ""; }
            set { dataPool["RoleName"] = value; }
        }
    }


    public class UserViewModel
    {
        public UserModel User
        {
            get;
            set;
        }

        public RoleModel[] Roles { get; set; }
    }
}
