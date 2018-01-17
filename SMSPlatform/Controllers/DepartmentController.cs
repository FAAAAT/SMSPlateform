﻿using System;
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
    [Authorize(Users = "admin")]
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
                JObject node = new JObject();
                node.Add(new JProperty("text", item.Dep.DName));
                node.Add(new JProperty("href", "###"));
                node.Add(new JProperty("tags", "[0]"));
                node.Add(new JProperty("MID", item.Dep.ID));
                node.Add(new JProperty("level",0));
                if (item.children != null)
                {
                    node.Add(new JProperty("nodes", RecursionTree(item.children,1)));
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

                helper.Delete("Department", $"ID={id}");
                helper.Delete("DepartmentContactor", $"DepartmentID = {id}");
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
        private static JArray RecursionTree(List<DepartmentService.DepModel> model,int level)
        {
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
                    node.Add(new JProperty("nodes", RecursionTree(item.children,level+1)));
                }
                json.Add(node);
            }
            return json;
        }

    }
}
