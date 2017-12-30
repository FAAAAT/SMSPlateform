using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAccessHelper;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class TagService
    {
        private SqlHelper helper;

        public TagService(SqlHelper helper)
        {
            this.helper = helper;
        }

        public IEnumerable<TagModel> GetTags(string tagName)
        {
            var where = string.IsNullOrWhiteSpace(tagName)?"":$"where TagName like '%{tagName}%'";
            var datas = helper.SelectDataTable($"select * from Tag {where}").Select();
            return datas.Select(x => (TagModel) new TagModel().SetData(x));
        }

        public void AddTag(TagModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TagName))
            {
                throw new Exception("TagName 不能为空");
            }
            if (helper.SelectScalar<int>("select count(1) from Tag where TagName = '" + model.TagName + "'") != 0)
            {
                throw new Exception("TagName 不能添加已存在的标签名称");
            }
            Dictionary<string,object> values = new Dictionary<string, object>();
            model.GetValues(values);
            values.Remove("ID");

            model.ID = (int)helper.Insert("Tag",values,"OUTPUT inserted.ID");
        }

        public void UpdateTag(TagModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TagName))
            {
                throw new Exception("TagName 不能为空");
            }
            if (helper.SelectScalar<int>("select count(1) where TagName = '" + model.TagName + "'") == 0)
            {
                throw new Exception("Tag 不存在");
            }
            Dictionary<string, object> values = new Dictionary<string, object>();
            model.GetValues(values);
            values.Remove("ID");
            helper.Update("Tag", values, " ID = " + model.ID,new List<SqlParameter>());

        }

        public void DeleteTag(TagModel model)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            
            try
            {
                helper.Delete("TagContactor", "TagID=" + model.ID);
                helper.Delete("Tag", " ID=" + model.ID);
                tran.Commit();
            }
            catch (Exception e)
            {
                tran.Rollback();
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                helper.ClearTransaction();
                    
            }
        }

        
    }
}
