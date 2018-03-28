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
using Logger;
using Newtonsoft.Json.Linq;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    [LymiAuthorize(Roles = "admin")]
    public class DepartmentController : ApiController
    {
        private SqlHelper helper;
        private SMSPlatformLogger logger = AppDomain.CurrentDomain.GetData("Logger") as SMSPlatformLogger;

        public DepartmentController()
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            helper = new SqlHelper();
            helper.SetConnection(conn);
        }

        [HttpGet]
        public IHttpActionResult GetDepartmentTreeData(string source = "")
        {
            DepartmentService service = new DepartmentService(helper);
            List<DepartmentService.DepModel> depList = service.GetAll();
            JArray json = new JArray();
            foreach (var item in depList)
            {
                Dictionary<string,object> context = null;
                JObject node = new JObject();
                node.Add(new JProperty("text", item.Dep.DName));
                node.Add(new JProperty("href", "###"));
                node.Add(new JProperty("tags", "[0]"));
                node.Add(new JProperty("MID", item.Dep.ID));
                if (source != "DepartmentManagement")
                {
                    node.Add(new JProperty("selectable", false));
                    context = new Dictionary<string, object>() {{"notLeafSelectable",false}
                }
                ;
                }
                node.Add(new JProperty("level",0));
                if (item.children != null)
                {
                    node.Add(new JProperty("nodes", RecursionTree(item.children,1,context)));
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
            var result = new CustomHttpActionResult();
            result.Response = new HttpResponseMessage(HttpStatusCode.OK);
            result.Response.Content = new StringContent(json.ToString());
            return result;
        }


        [HttpGet]
        public IHttpActionResult Edit([FromUri]DepartmentModel model)
        {
            if (model.ID == null)
            {
                return Json(new ReturnResult()
                {
                    msg = "修改的ID不能为空",
                    success = false,
                    status = 500,
                });
            }


            try
            {
                var pmodel = helper.SelectDataTable($"select * from Department where ID = {model.PDID.Value}").Select().Select(x=>new DepartmentModel().SetData(x) as DepartmentModel).SingleOrDefault();
                if (pmodel!=null)
                {
                    model.LevelIndex = pmodel.LevelIndex + 1;
                }

                var dic = new Dictionary<string,object>();
                model.GetValues(dic);
                dic.Remove("ID");
                helper.Update("Department", dic, $" ID = {model.ID}", new List<SqlParameter>());
                return Json(new ReturnResult()
                {
                    msg="修改成功",
                    success = true,
                    status = 200,
                });
            }
            catch (Exception e)
            {

                logger.Error(e.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = e.ToString(),
#else
                    msg = "内部错误,请联系管理员",
#endif
                    success = false,
                    status = 500,
                });
            }


        }


        [HttpGet]
        public IHttpActionResult Add([FromUri] DepartmentModel model)
        {
            try
            {
                if (!model.PDID.HasValue||string.IsNullOrWhiteSpace(model.DName))
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg="请输入部门信息",

                    });
                }

                var pmodel = helper.SelectDataTable($"select * from Department where ID = "+(model.PDID==null?"":model.PDID.Value+"")).Select().Select(x => new DepartmentModel().SetData(x) as DepartmentModel).SingleOrDefault();
                if (pmodel != null)
                {
                    model.LevelIndex = pmodel.LevelIndex + 1;
                }

                var dic = new Dictionary<string,object>();
                model.GetValues(dic);
                dic.Remove("ID");
                var id = (int)helper.Insert("Department", dic, "OUTPUT inserted.ID");
                return Json(new ReturnResult()
                {
                    msg="添加成功",
                    data = id,
                    success = true,
                });
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = e.ToString(),
#else
                    msg = "内部错误,请联系管理员",
#endif
                    success = false,
                    status = 500,
                });
            }
        }

        [HttpGet]
        public IHttpActionResult Del(int id)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {
                var children = helper.SelectDataTable($"select * from Department where PDID = {id}").Select();
                if (children.Any())
                {
                    return Json(new ReturnResult(){
                        success = false,
                        msg="该部门拥有下属部门，请先删除下属部门",
                        status = 500,
                    } );
                }

                var rowDeps = helper.SelectDataTable($"select * from DepartmentTag where DepartmentID = {id}").Select();

                if (rowDeps.Length!=0)
                {
                    helper.Delete("ContactorDepartmentTag",
                        $"DepartmentTagID in ({string.Join(",", rowDeps.Select(x => x["ID"] + ""))})", new List<SqlParameter>());
                }

                helper.Delete("DepartmentTag", $"DepartmentID ={id}", new List<SqlParameter>());


                helper.Delete("Department", $"ID={id}");
                
                tran.Commit();
                return Json(new ReturnResult()
                {
                    msg = "删除成功",
                    success = true,
                    status = 200,
                });
            }
            catch (Exception e)
            {
                tran.Rollback();
                logger.Error(e.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = e.ToString(),
#else
                    msg = "内部错误,请联系管理员",
#endif
                    success = false,
                    status = 500,
                });
            }
            finally
            {
                helper.ClearTransaction();
            }
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
        private static JArray RecursionTree(List<DepartmentService.DepModel> model,int level,Dictionary<string,object> context = null)
        {
            bool notLeafSelectable = true;
            if (context!=null)
            {
                notLeafSelectable = (bool)context["notLeafSelectable"];
            }


            JArray json = new JArray();
            foreach (var item in model)
            {
                JObject node = new JObject();
                node.Add(new JProperty("text", item.Dep.DName));
                node.Add(new JProperty("href", "###"));
                node.Add(new JProperty("tags", "[0]"));
                node.Add(new JProperty("MID", item.Dep.ID));
                node.Add(new JProperty("level",level));
                if (item.children != null)
                {
                    node.Add(new JProperty("nodes", RecursionTree(item.children,level+1,context)));
                    node.Add(new JProperty("selectable", notLeafSelectable));

                }
                json.Add(node);
            }
            return json;
        }

    }
}
