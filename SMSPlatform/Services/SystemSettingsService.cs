using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAccessHelper;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class SystemSettingsService
    {

        private SqlHelper helper;

        public SystemSettingsService(SqlHelper helper)
        {
            this.helper = helper;
        }

        public SystemSettingsModel GetByPhoneNumber(string phone)
        {
            return helper.SelectDataTable($"select * from systemsettings where phonenumber = {phone}").Select().Select(x=>(SystemSettingsModel)new SystemSettingsModel().SetData(x)).SingleOrDefault();
        }

        public int AddPhoneLimitSettings(SystemSettingsModel model)
        {
            var dic = new Dictionary<string,object>();

            model.GetValues(dic);
            dic.Remove("ID");
            return (int)helper.Insert("SystemSettings", dic, "OUTPUT inserted.ID");

        }

        public void UpdatePhoneLimitSettings(SystemSettingsModel model)
        {
            var dic = new Dictionary<string, object>();

            model.GetValues(dic);
            dic.Remove("ID");
            helper.Update("SystemSettings", dic,$" ID = {model.ID}",new List<SqlParameter>());

        }

        public void DeletePhoneLimitSettings(int id)
        {
            helper.Delete("systemsettings", $" ID = {id}");
        }





    }

}
