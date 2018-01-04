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
    public class TemplateService
    {
        private SqlHelper helper;

        public TemplateService(SqlHelper helper)
        {
            this.helper = helper;
        }

        public IEnumerable<TemplateModel> GetTemplates(string templateName)
        {
            var whereSTr = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(templateName))
            {
                whereSTr += $" and TemplateName like '%{templateName}%'";

            }
            return helper.SelectDataTable("select * from Template " + whereSTr).Select().Select(x=>(TemplateModel)new TemplateModel().SetData(x));
        }

        public int AddTemplate(TemplateModel model)
        {
            var valueDIc = new Dictionary<string,object>();
            model.GetValues(valueDIc);
            valueDIc.Remove("ID");
            return (int)helper.Insert("Template", valueDIc," OUTPUT inserted.ID");
        }

        public void UpdateTemplate(TemplateModel model)
        {
            var valueDIc = new Dictionary<string, object>();
            model.GetValues(valueDIc);
            valueDIc.Remove("ID");
            helper.Update("Template", valueDIc,"",new List<SqlParameter>());
        }

        public void DeleteTemplate(int id)
        {
            helper.Delete("Template", $" ID = {id} ", new List<SqlParameter>());
        }

        public TemplateModel GetTemplate(int id)
        {
            return helper.SelectDataTable("select * from Template where ID = "+id).Select().Select(x=>(TemplateModel)new TemplateModel().SetData(x)).SingleOrDefault();
        }

        public string TemplateReplace(string content,Dictionary<string, string> kv)
        {
            foreach (var data in kv)
            {
               content=  content.Replace("{"+data.Key+"}",data.Value);
            }

            return content;
        }

    }
}
