using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DataBaseAccessHelper;
using Newtonsoft.Json.Linq;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    [Authorize(Users = "admin")]
    public class DepartmentController:ApiController
    {
        private SqlHelper helper;

        public DepartmentController()
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            helper = new SqlHelper();
            helper.SetConnection(conn);
        }

        [HttpGet]

        public IHttpActionResult GetDepartmentTreeData(string source="")
        {
            DepartmentService service = new DepartmentService(helper);
            List<DepartmentService.DepModel> depList = service.GetAll();
            JArray json = new JArray();
            foreach (var item in depList)
            {
                JObject node = new JObject();
                node.Add(new JProperty("text", item.Dep.DName));
                node.Add(new JProperty("href", "###"));
                node.Add(new JProperty("tags", "[0]"));
                node.Add(new JProperty("MID", item.Dep.ID));
                if (item.children != null)
                {
                    node.Add(new JProperty("nodes", RecursionTree(item.children)));
                    //var ss = ;
                }
                json.Insert(0, node);
            }

            //            if (!string.IsNullOrWhiteSpace(source))
            //            {
            //                if (source == "UserAddEdit")
            //                {
            //                    JObject node = new JObject();
            //                    node.Add(new JProperty("text", "本部门"));
            //                    node.Add(new JProperty("href", "###"));
            //                    node.Add(new JProperty("tags", "[0]"));
            //                    node.Add(new JProperty("MID", "0"));
            //                    json.Add(node);
            //                    //node = new JObject();
            //                    //node.Add(new JProperty("text", "不限部门"));
            //                    //node.Add(new JProperty("href", "###"));
            //                    //node.Add(new JProperty("tags", "[0]"));
            //                    //node.Add(new JProperty("MID", "-1"));
            //                    //json.Insert(0, node);
            //                }
            //            }
            var result =new CustomHttpActionResult();
            result.Response = new HttpResponseMessage(HttpStatusCode.OK);
            result.Response.Content = new StringContent(json.ToString());
            return result;
        }


        protected override void Dispose(bool disposing)
        {
            helper.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 递归树
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static JArray RecursionTree(List<DepartmentService.DepModel> model)
        {
            JArray json = new JArray();
            foreach (var item in model)
            {
                JObject node = new JObject();
                node.Add(new JProperty("text", item.Dep.DName));
                node.Add(new JProperty("href", "###"));
                node.Add(new JProperty("tags", "[0]"));
                node.Add(new JProperty("MID", item.Dep.ID));
                if (item.children != null)
                {
                    node.Add(new JProperty("nodes", RecursionTree(item.children)));
                }
                json.Add(node);
            }
            return json;
        }

    }
}
