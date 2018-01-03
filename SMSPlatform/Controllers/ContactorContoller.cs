using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DataBaseAccessHelper;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class contactorController : ApiController
    {
        private SqlHelper helper;

        public contactorController()
        {
            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            helper = new SqlHelper();
            helper.SetConnection(conn);
        }

        [HttpGet]
        public IHttpActionResult Getcontactor(string name, string phone, string[] tagIds,int? pageIndex = null,int? pageSize = null)
        {
            try
            {

                var nvs = Request.RequestUri.ParseQueryString();
                tagIds = nvs["tagIds"]?.Split(',');

                string whereStr = " where 1=1";
                if (string.IsNullOrWhiteSpace(name))
                {
                    whereStr += $" and contactorName like '%{name}%' ";


                }
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    whereStr += $" and PhoneNumber = '{phone}' ";
                }
                if (tagIds != null && tagIds.Length != 0)
                {
                    var tags =
                        helper.SelectDataTable(
                            $"select * from Tagcontactor where TagID in ({string.Join(",", tagIds)})").Select();
                    var groupedTags = tags.GroupBy(x => x["TagID"]);
                    var tagcontactorIDs = groupedTags.Select(x => x).ToList();
                    var contactorIds = tagcontactorIDs[0].ToList();
                    for (int i = 1; i < tagcontactorIDs.Count; i++)
                    {
                        contactorIds = contactorIds.Join(tagcontactorIDs[i], x => x["contactorID"] + "",
                            x => x["contactorID"] + "", (x, y) => x).ToList();
                    }

                    whereStr += $" and ID in ({string.Join(",", contactorIds.Select(x => x["contactorID"] + ""))})";


                }

                var datas = helper.SelectDataTable("select * from contactor " + whereStr, new List<IDataParameter>()).Select().Select(x=>new {ContactorName=x["ContactorName"]+"", PhoneNumber = x["PhoneNumber"]+""});
                var total = datas.Count();
                if (pageSize.HasValue&&pageIndex.HasValue)
                {
                    datas = datas.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value).ToArray();
                }
                



                return Json(new ReturnResult() {success = true, data = datas, status = 200,total = total});


            }
            catch (Exception ex)
            {
                return Json(new ReturnResult() {success = false, msg = ex.ToString(), status = 500});
            }
            finally
            {
                helper.Dispose();
            }


        }


        [HttpGet]
        public IHttpActionResult Addcontactor(string name, string phone, string remark, string[] tagIds)
        {
            var count = helper.SelectScalar<int>($"select count(1) from contactor where PhoneNumber = '{phone}'");
            if (count != 0)
            {
                return Json(new ReturnResult() {msg = "电话号码已存在", success = false, status = 200});
            }
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {
                var id = helper.Insert("contactor",
                    new Dictionary<string, object>()
                    {
                        {"contactorName", name},
                        {"PhoneNumber", phone},
                        {"Remark", remark}
                    }, "OUTPUT inserted.ID");
                var nvs = Request.RequestUri.ParseQueryString();
                tagIds = nvs["tagIds"].Split(',');
                foreach (var tagId in tagIds)
                {
                    helper.Insert("Tagcontactor",
                        new Dictionary<string, object>() {{"contactorID", id}, {"TagID", tagId}});


                }
                tran.Commit();
                return Json(new ReturnResult()
                {
                    msg = "新增联系人成功",
                    success = true,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Json(new ReturnResult()
                {
                    msg = ex.ToString(),
                    success = false,
                    status = 500

                });
            }
            finally
            {
                helper.Dispose();
            }

        }


        [HttpGet]
        public IHttpActionResult Deletecontactor(int id)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {
                helper.Delete("contactor", $" ID = {id}", new List<SqlParameter>());
                helper.Delete("Tagcontactor", $" contactorID = {id}", new List<SqlParameter>());
                tran.Commit();
                return Json(new ReturnResult()
                {
                    msg = "删除成功",
                    success = true,
                    status = 200
                });

            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Json(new ReturnResult()
                {
                    msg = "删除失败",
                    success = false,
                    status = 500,
                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpGet]
        public IHttpActionResult UpdateCongractor(int id, string phoneNumber, string name, string remark, string[] tagIds)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {

                var nvs = Request.RequestUri.ParseQueryString();
                tagIds = nvs["tagIds"].Split(',');

                helper.Update("contactor",
                    new Dictionary<string, object>()
                    {
                        {"contactorName", name},
                        {"PhoneNumber", phoneNumber},
                        {"Remark", remark}
                    }, $" ID = {id}", new List<SqlParameter>());
                var existsTagIds = helper.SelectList<int>("Tagcontactor", "TagID", $" ContractID = {id}",
                    new List<SqlParameter>());
                var addedTagIds = existsTagIds.Except(tagIds.Select(int.Parse));
                var deletedTagIds = tagIds.Select(int.Parse).Except(existsTagIds);
                foreach (var addedTagId in addedTagIds)
                {
                    helper.Insert("TagContract",
                        new Dictionary<string, object>() {{"ContractID", id}, {"TagID", addedTagId}});
                }
                if (deletedTagIds.Any())
                {
                    helper.Delete("Tagcontactor", $" TagID in ({string.Join(",", tagIds)})");

                }
                tran.Commit();

                return Json(new ReturnResult() {msg = "更新联系人成功", success = true, status = 200});
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Json(new ReturnResult() {msg = ex.ToString(), success = false, status = 500});
            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpGet]
        public IHttpActionResult GetcontactorDetail(int id)
        {
            try
            {
                var contactor =
                    helper.SelectDataTable($"select * from contactor where ID = {id}")
                        .Select()
                        .Select(x => (contactorModel) new contactorModel().SetData(x))
                        .SingleOrDefault();
                var tags = helper.SelectList<int>("Tagcontactor", "TagID", $"contactorID = {contactor.ID}");

                return Json(new ReturnResult()
                {
                    data = new
                    {
                        contactor = contactor,
                        tags = tags
                    },
                    success = true,
                    status = 200
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    status = 500,
                    msg = ex.ToString()
                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        



    }
}
