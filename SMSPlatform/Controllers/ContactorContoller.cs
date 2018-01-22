﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
        public IHttpActionResult Getcontactor(string name, string phone, [FromUri]string[] tagIds, [FromUri]string[] selectedDeps, int? pageIndex = null, int? pageSize = null)
        {
            try
            {

                var nvs = Request.RequestUri.ParseQueryString();
                tagIds = string.IsNullOrWhiteSpace(nvs["tagIds"]) ? new string[0] : nvs["tagIds"].Split(',');
                selectedDeps = string.IsNullOrWhiteSpace(nvs["selectedDeps"]) ? new string[0] : nvs["selectedDeps"].Split(',');
                string whereStr = " where 1=1";
                if (!string.IsNullOrWhiteSpace(name))
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
                    if (tagIds.Except(groupedTags.Select(x => x.Key + "")).Any())
                    {
                        whereStr += $" and 1=0";

                    }
                    else
                    {
                        if (tagcontactorIDs.Any())
                        {
                            var contactorIds = tagcontactorIDs[0].ToList();
                            for (int i = 1; i < tagcontactorIDs.Count; i++)
                            {
                                contactorIds = contactorIds.Join(tagcontactorIDs[i], x => x["contactorID"] + "",
                                    x => x["contactorID"] + "", (x, y) => x).ToList();
                            }
                            if (contactorIds == null || contactorIds.Count == 0)
                                whereStr += $" and 1=0";
                            else
                                whereStr += $" and ID in ({string.Join(",", contactorIds.Select(x => x["contactorID"] + ""))})";

                        }
                        else
                        {
                            whereStr += $" and 1=0";

                        }

                    }


                }

                if (selectedDeps != null && selectedDeps.Length != 0)
                {
                    var depContactors = helper.SelectDataTable($"select * from DepartmentContactor where DepartmentID in ({string.Join(",", selectedDeps)})").Select();
                    var groupedDepCon = depContactors.GroupBy(x => x["DepartmentID"]);

                    if (selectedDeps.Except(groupedDepCon.Select(x => x.Key + "")).Any())
                        whereStr += $" and 1=0";
                    else
                    {
                        if (depContactors.Any())
                        {
                            var contactorIds = groupedDepCon.First().ToList();
                            for (int i = 1; i < groupedDepCon.Count(); i++)
                            {
                                contactorIds = contactorIds.Join(groupedDepCon.Skip(i).Take(1).Single(),
                                    x => x["contactorID"] + "", x => x["contactorID"] + "", (x, y) => x).ToList();
                            }
                            if (contactorIds == null || contactorIds.Count == 0)
                                whereStr += $" and 1=0";
                            else
                                whereStr += $" and ID in ({string.Join(",", contactorIds.Select(x => x["contactorID"] + ""))})";

                        }
                        else
                        {
                            whereStr += $" and 1=0";
                        }
                    }







                }

                var datas = helper.SelectDataTable("select * from contactor " + whereStr, new List<IDataParameter>()).Select().Select(x => new { ContactorName = x["ContactorName"] + "", PhoneNumber = x["PhoneNumber"] + "", ID = x["ID"] + "" });
                var total = datas.Count();
                var allIds = datas.Select(x => x.ID);
                if (pageSize.HasValue && pageIndex.HasValue)
                {
                    datas = datas.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value).ToArray();
                }




                return Json(new ReturnResult()
                {
                    success = true,
                    data = datas,
                    status = 200,
                    total = total,
                    allIds = allIds.ToArray()
                });


            }
            catch (Exception ex)
            {
                return Json(new ReturnResult() { success = false, msg = ex.ToString(), status = 500 });
            }
            finally
            {
                helper.Dispose();
            }


        }

        [HttpGet]
        public IHttpActionResult Addcontactor(string name, string phone, string remark, [FromUri]string[] selectedDeps, [FromUri]string[] tagIds)
        {
            var count = helper.SelectScalar<int>($"select count(1) from contactor where PhoneNumber = '{phone}'");
            if (count != 0)
            {
                return Json(new ReturnResult() { msg = "电话号码已存在", success = false, status = 200 });
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
                tagIds = string.IsNullOrWhiteSpace(nvs["tagIds"]) ? new string[0] : nvs["tagIds"].Split(',');
                selectedDeps = string.IsNullOrWhiteSpace(nvs["selectedDeps"]) ? new string[0] : nvs["selectedDeps"].Split(',');
                foreach (var tagId in tagIds)
                {
                    helper.Insert("Tagcontactor",
                        new Dictionary<string, object>() { { "contactorID", id }, { "TagID", tagId } });
                }
                foreach (string selectedDep in selectedDeps)
                {
                    helper.Insert("departmentContactor", new Dictionary<string, object>()
                    {
                        {"contactorID",id },{"DepartmentID",selectedDep}
                    });
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
                helper.ClearTransaction();
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
                helper.Delete("DepartmentContractor", $"contactorID={id}", new List<SqlParameter>());

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
                helper.ClearTransaction();
            }
        }

        [HttpGet]
        public IHttpActionResult UpdateContactor(int id, string phone, string name, string remark, string[] selectedDeps)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {

                var nvs = Request.RequestUri.ParseQueryString();
                var tagIds = string.IsNullOrWhiteSpace(nvs["tagIds"]) ? new string[0] : nvs["tagIds"].Split(',');
                selectedDeps = string.IsNullOrWhiteSpace(nvs["selectedDeps"]) ? new string[0] : nvs["selectedDeps"].Split(',');
                helper.Update("contactor",
                    new Dictionary<string, object>()
                    {
                        {"contactorName", name},
                        {"PhoneNumber", phone},
                        {"Remark", remark}
                    }, $" ID = {id}", new List<SqlParameter>());
                var existsTagIds = helper.SelectList<int>("Tagcontactor", "TagID", $" ContactorID = {id}",
                    new List<SqlParameter>());
                var deletedTagIds = existsTagIds.Except(tagIds.Select(int.Parse));
                var addedTagIds = tagIds.Select(int.Parse).Except(existsTagIds);
                foreach (var addedTagId in addedTagIds)
                {
                    helper.Insert("TagContactor",
                        new Dictionary<string, object>() { { "ContactorID", id }, { "TagID", addedTagId } });
                }
                if (deletedTagIds.Any())
                {
                    helper.Delete("Tagcontactor", $" TagID in ({string.Join(",", deletedTagIds)})");
                }

                var existsDepIds = helper.SelectList<int>("DepartmentContactor", "DepartmentID", $"ContactorID = {id}");
                var deletedDepIds = existsDepIds.Except(selectedDeps.Select(int.Parse));
                var addedDepIds = selectedDeps.Select(int.Parse).Except(existsDepIds);
                foreach (int addedDepId in addedDepIds)
                {
                    helper.Insert("DepartmentContactor", new Dictionary<string, object>()
                    {
                        { "contactorID",id},
                        { "DepartmentID",addedDepId}
                    });
                }
                if (deletedDepIds.Any())
                {
                    helper.Delete("DepartmentContactor", $" DepartmentID in ({string.Join(",", deletedDepIds)})");

                }


                tran.Commit();

                return Json(new ReturnResult() { msg = "更新联系人成功", success = true, status = 200 });
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return Json(new ReturnResult() { msg = ex.ToString(), success = false, status = 500 });
            }
            finally
            {
                helper.ClearTransaction();
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
                        .Select(x => (contactorModel)new contactorModel().SetData(x))
                        .SingleOrDefault();
                var tags = helper.SelectList<int>("Tagcontactor", "TagID", $"contactorID = {contactor.ID}");

                var departmentIds = helper.SelectList<int>("DepartmentContactor", "DepartmentID", $"ContactorID = {id}");

                return Json(new ReturnResult()
                {
                    data = new
                    {
                        contactor = contactor,
                        tags = tags,
                        depIds = departmentIds
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

        [HttpGet]
        public IHttpActionResult GetContactorDetails(int[] ids)
        {

            var queryDatas = new FormDataCollection(Request.RequestUri);
            var temp = queryDatas["ids[]"]?.Split(',');

            ids = temp?.Select(x => int.Parse(x)).ToArray();

            try
            {
                var whereStr = ids == null || ids.Length == 0
                    ? " where 1=0 "
                    : $" where ID in ({string.Join(",", ids)})";

                var contactors =
                    helper.SelectDataTable($"select * from contactor {whereStr}")
                        .Select()
                        .Select(x => (contactorModel)new contactorModel().SetData(x))
                        ;

                whereStr = contactors == null || contactors.Count() == 0
                    ? " 1=0 "
                    : $" ContactorID in ({string.Join(",", contactors.Select(x => x.ID))})";


                var tagIds = helper.SelectDataTable("select * from TagContactor where " + whereStr).Select();

                var departemntIds = helper.SelectDataTable("select * from DepartmentContactor where " + whereStr).Select();

                whereStr = tagIds == null || tagIds.Count() == 0
                    ? " 1=0 "
                    : $" ID in ({string.Join(",", tagIds.Select(x=>x["TagID"]+""))})";


                var tags = helper.SelectDataTable("select * from Tag where " + whereStr).Select().Select(x => new TagModel().SetData(x) as TagModel);

                whereStr = departemntIds == null || departemntIds.Count() == 0
                ? " 1=0 "
                    : $" ID in ({string.Join(",", departemntIds.Select(x=>x["DepartmentID"]+""))})";

                var departments = helper.SelectDataTable("select * from Department where " + whereStr).Select()
                    .Select(x => new DepartmentModel().SetData(x) as DepartmentModel);



                var datas = contactors.Select(x => new
                {
                    contactor = x
                    , tags = tagIds.Where(y=>(int)y["ContactorID"] == x.ID).Select(y=>tags.SingleOrDefault(z=>z.ID == (int)y["TagID"]))
                    , departments = departemntIds.Where(y=>(int)y["ContactorID"] == x.ID).Select(y=>departments.SingleOrDefault(z=>z.ID == (int)y["DepartmentID"]))
                });


                return Json(new ReturnResult()
                {
                    data = datas,
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

        [HttpPost]
        public IHttpActionResult UploadContactors()
        {
            var s =this.Request.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            byte[] buffer = new byte[s.Length];
            s.Read(buffer, 0, buffer.Length);
            var fileName = this.Request.Content.Headers.ContentDisposition.FileName;
#if DEBUG
//            var finfo = new FileInfo(Path.Combine(Environment.CurrentDirectory,"..\\..\\",fileName));
#else
//            var finfo = new FileInfo(Path.Combine(Environment.CurrentDirectory,"..\\",fileName));
#endif
//            var fs = finfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite,FileShare.Delete);
//
//            fs.Seek(0, SeekOrigin.Begin);

            


        }

    }
}
