using System;
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
                    whereStr += $" and PhoneNumber like '%{phone}%' ";
                }
                if (tagIds != null && tagIds.Length != 0)
                {
                    var depTagIds =
                        helper.SelectDataTable($"select * from DepartmentTag where TagID in ({string.Join(",",tagIds)})").Select().Select(x=>(int)x["ID"]);
                    IEnumerable<int> contactorIds;
                    if (depTagIds.Any())
                    {
                        contactorIds = helper.SelectDataTable($"select * from ContactorDepartmentTag where DepartmentTagID in ({string.Join(",", depTagIds)})").Select().Select(x=>(int)x["ContactorID"]);
                        whereStr += $" and ID in ({string.Join(",",contactorIds.Select(x=>x+""))})";

                    }
                    else
                    {
                        whereStr += $" and 1=0";
                    }
                }

                if (selectedDeps != null && selectedDeps.Length != 0)
                {
                    var depTagIds = helper.SelectDataTable($"select * from DepartmentTag where DepartmentID in ({string.Join(",", selectedDeps)})").Select().Select(x=>(int)x["ID"]);

                    IEnumerable<int> contactorIds;
                    if (depTagIds.Any())
                    {
                        contactorIds = helper.SelectDataTable($"select * from ContactorDepartmentTag where DepartmentTagID in {string.Join(",", depTagIds)}").Select().Select(x => (int)x["ContactorID"]);
                        whereStr += $" and ID in ({string.Join(",", contactorIds.Select(x => x + ""))})";

                    }
                    else
                    {
                        whereStr += $" and 1=0";
                    }

                   
                }

                var datas = helper.SelectDataTable("select * from contactor " + whereStr, new List<IDataParameter>()).Select().Select(x => new { ContactorName = x["ContactorName"] + "", PhoneNumber = x["PhoneNumber"] + "", ID = x["ID"] + "" });
                var total = datas.Count();
                var allIds = datas.Select(x => x.ID);
                if (pageSize.HasValue && pageIndex.HasValue)
                {
                    datas = datas.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value).ToArray();
                }

                whereStr = datas == null || datas.Count() == 0
                    ? " 1=0 "
                    : $" ContactorID in ({string.Join(",", datas.Select(x => x.ID))})";


                var conDepTagRows = helper.SelectDataTable("select * from ContactorDepartmentTag where " + whereStr).Select();

                var depTagRows = helper.SelectDataTable(conDepTagRows.Any()?$"select * from DepartmentTag where ID in ({string.Join(",",conDepTagRows.Select(x=>x["DepartmentTagID"]))})": "select * from DepartmentTag where 1=0").Select();


                whereStr = depTagRows == null || depTagRows.Count() == 0
                    ? " 1=0 "
                    : $" ID in ({string.Join(",", depTagRows.Select(x => x["TagID"] + ""))})";


                var ttags = helper.SelectDataTable("select * from Tag where " + whereStr).Select().Select(x => new TagModel().SetData(x) as TagModel);

                whereStr = depTagRows == null || depTagRows.Count() == 0
                    ? " 1=0 "
                    : $" ID in ({string.Join(",", depTagRows.Select(x => x["DepartmentID"] + ""))})";

                var departments = helper.SelectDataTable("select * from Department where " + whereStr).Select()
                    .Select(x => new DepartmentModel().SetData(x) as DepartmentModel);

                var allDepartments = helper.SelectDataTable("select * from Department").Select()
                    .Select(x => new DepartmentModel().SetData(x) as DepartmentModel); ;



                var results = datas.Select(x => new
                {
                    contactor = x
                    ,
                    depTags =  depTagRows.Where(y => conDepTagRows.Where(z => z["ContactorID"] + "" == x.ID).Select(z=>z["DepartmentTagID"]+"").Contains(y["ID"] + ""))
                    .Select(y => new
                    {
                        department = new {child = departments.SingleOrDefault(z=>z.ID == (int)y["DepartmentID"]),parent = allDepartments.SingleOrDefault(z=>z.ID == departments.SingleOrDefault(l => l.ID == (int)y["DepartmentID"])?.PDID) },
                        tag =ttags.SingleOrDefault(z=>z.ID == (int)y["TagID"])
                    })
                });


                return Json(new ReturnResult()
                {
                    success = true,
                    data = results,
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
        public IHttpActionResult Addcontactor(string name, string phone, string remark, [FromUri]string[] selectedDepTagIds)
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
                var depTagIds = string.IsNullOrWhiteSpace(nvs["depTagIds"]) ? new string[0] : nvs["depTagIds"].Split(',');

                var existsDepTagIDs = helper.SelectDataTable($"select * from ContactorDepartmentTag where ContactorID = {id}").Select().Select(x=>x["DepartmentTagID"]+"");


                foreach (var VARIABLE in depTagIds.Where(x=> !existsDepTagIDs.Contains(x+"")))
                {

                    helper.Insert("ContactorDepartmentTag", new Dictionary<string, object>(){{"ContactorID",id},{"DepartmentTagID",VARIABLE}});

                }
                
                helper.ClearTransaction();
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
        public IHttpActionResult UpdateContactor(int id, string phone, string name, string remark, string[] selectedDepTagIds)
        {
            var conn = helper.GetOpendSqlConnection();
            var tran = conn.BeginTransaction();
            helper.SetTransaction(tran);
            try
            {

                var nvs = Request.RequestUri.ParseQueryString();
//                var tagIds = string.IsNullOrWhiteSpace(nvs["tagIds"]) ? new string[0] : nvs["tagIds"].Split(',');
                var selectedDepTags = string.IsNullOrWhiteSpace(nvs["selectedDepTagIds"]) ? new string[0] : nvs["selectedDepTagIds"].Split(',');
                helper.Update("contactor",
                    new Dictionary<string, object>()
                    {
                        {"contactorName", name},
                        {"PhoneNumber", phone},
                        {"Remark", remark}
                    }, $" ID = {id}", new List<SqlParameter>());


                
//                var existsDepTagIds = helper.SelectList<int>("ContactorDepartmentTag","DepartmentTagID", $" ContactorID = {id}",
//                    new List<SqlParameter>());
//
//
//                var deletedTagIds = existsDepTagIds.Except(selectedDepTags.Select(int.Parse));
//                var addedTagIds = selectedDepTags.Select(int.Parse).Except(existsDepTagIds);
//                foreach (var addedTagId in addedTagIds)
//                {
//                    helper.Insert("TagContactor",
//                        new Dictionary<string, object>() { { "ContactorID", id }, { "TagID", addedTagId } });
//                }
//                if (deletedTagIds.Any())
//                {
//                    helper.Delete("Tagcontactor", $" TagID in ({string.Join(",", deletedTagIds)})");
//                }

                var existsDepIds = helper.SelectList<int>("ContactorDepartmentTag", "DepartmentTagID", $"ContactorID = {id}");
                var deletedDepIds = existsDepIds.Except(selectedDepTags.Select(int.Parse));
                var addedDepIds = selectedDepTags.Select(int.Parse).Except(existsDepIds);
                foreach (int addedDepId in addedDepIds)
                {
                    helper.Insert("ContactorDepartmentTag", new Dictionary<string, object>()
                    {
                        { "ContactorID",id},
                        { "DepartmentTagID",addedDepId}
                    });
                }
                if (deletedDepIds.Any())
                {
                    helper.Delete("ContactorDepartmentTag", $" DepartmentTagID in ({string.Join(",", deletedDepIds)})");
                    
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
        public IHttpActionResult GetContactorDetail(int id)
        {
            try
            {
                var contactor =
                    helper.SelectDataTable($"select * from contactor where ID = {id}")
                        .Select()
                        .Select(x => (contactorModel)new contactorModel().SetData(x))
                        .SingleOrDefault();
                var cDepTags = helper.SelectDataTable($"select * from ContactorDepartmentTag where ContactorID = {contactor.ID}").Select();
                var depTags =
                    cDepTags.Length==0?helper.SelectDataTable("select * from DepartmentTag where 1=0").Select():
                    helper.SelectDataTable(
                        $"select * from DepartmentTag where ID in ({string.Join(",", cDepTags.Select(x => (int)x["DepartmentTagID"]))})").Select();
                //获取 一级 二级 department 和tag

                var departmentIds = depTags.Select(x => (int) x["DepartmentID"]);
                var allDeps = helper.SelectDataTable($"select * from Department").Select().Select(x=>new DepartmentModel().SetData(x) as DepartmentModel);

                var deps = allDeps.Where(x=>departmentIds.Contains(x.ID.Value)).Select(x => new
                {
                    parent = x.PDID.HasValue ? allDeps.SingleOrDefault(y => y.ID == x.PDID) : null,
                    child = x
                });


                var tagIds = depTags.Select(x => (int) x["TagID"]);
                var tags = 
                    tagIds.Any()?
                    helper.SelectDataTable($"select * from Tag where ID in ({string.Join(",",tagIds.Select(x=>x+""))})"):
                    helper.SelectDataTable($"select * from Tag where 1=0");



                return Json(new ReturnResult()
                {
                    data = new
                    {
                        contactor = contactor,
                        tags = tags,
//                        deps = deps,
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
                    : $" ID in ({string.Join(",", tagIds.Select(x => x["TagID"] + ""))})";


                var tags = helper.SelectDataTable("select * from Tag where " + whereStr).Select().Select(x => new TagModel().SetData(x) as TagModel);

                whereStr = departemntIds == null || departemntIds.Count() == 0
                ? " 1=0 "
                    : $" ID in ({string.Join(",", departemntIds.Select(x => x["DepartmentID"] + ""))})";

                var departments = helper.SelectDataTable("select * from Department where " + whereStr).Select()
                    .Select(x => new DepartmentModel().SetData(x) as DepartmentModel);



                var datas = contactors.Select(x => new
                {
                    contactor = x
                    ,
                    tags = tagIds.Where(y => (int)y["ContactorID"] == x.ID).Select(y => tags.SingleOrDefault(z => z.ID == (int)y["TagID"]))
                    ,
                    departments = departemntIds.Where(y => (int)y["ContactorID"] == x.ID).Select(y => departments.SingleOrDefault(z => z.ID == (int)y["DepartmentID"]))
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
            try
            {

                var provider = this.Request.Content.ReadAsMultipartAsync().GetAwaiter().GetResult();



                //                foreach (HttpContent httpContent in provider.Contents)
                //                {
                //                    var s = httpContent.ReadAsStreamAsync().GetAwaiter().GetResult();
                //                    byte[] buffer = new byte[s.Length];
                //                    s.Read(buffer, 0, buffer.Length);
                //                    var fileName = httpContent.Headers.ContentDisposition.FileName;
#if DEBUG
                //                    var finfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, "..\\..\\Upload\\", fileName.Trim('\"')));
#else
//                    var finfo = new FileInfo(Path.Combine(Environment.CurrentDirectory,"..\\",fileName));
#endif
                //                    var fs = finfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete);

                //                    fs.Seek(0, SeekOrigin.Begin);

                //                    fs.Write(buffer, 0, buffer.Length);
                //                    fs.Flush();
                //                    fs.Close();
                //                    s.Close();

                //                }



                foreach (HttpContent httpContent in provider.Contents)
                {
                    var s = httpContent.ReadAsStreamAsync().GetAwaiter().GetResult();
                    OpenXMLHelp help = new OpenXMLHelp();
                    DataTable table = help.ReadExcel("Sheet1", s);
                    var infoDiffer = false;


                    foreach (var row in table.Select().Skip(1))
                    {
                        var phone = (row["电话"] + "").Trim();
                        var name = (row["姓名"] + "").Trim();
                        var level2DepName = (row["学院"] + "").Trim();
                        var level3DepName = (row["二级名称"] + "").Trim();
                        var tagName = (row["标签"] + "").Trim();
                        var remark = (table.Columns.Contains("备注") ? row["备注"] + "" : "").Trim();
                        var contactor = helper.SelectDataTable($"select * from contactor where PhoneNumber = '{phone}'")
                            .Select().Select(x => new contactorModel().SetData(x) as contactorModel).SingleOrDefault();
                        if (contactor == null)
                        {
                            infoDiffer = true;
                            var dic = new Dictionary<string, object>();
                            contactor = new contactorModel();
                            contactor.PhoneNumber = phone;
                            contactor.ContactorName = name;
                            contactor.Remark = remark;
                            contactor.GetValues(dic);
                            contactor.ID = (int)helper.Insert("Contactor", dic, " OUTPUT inserted.id ");
                        }

                        var level2Dep = helper
                            .SelectDataTable($"select * from Department where DName = '{level2DepName}' and LevelIndex = 2")
                            .Select().Select(x => new DepartmentModel().SetData(x) as DepartmentModel).SingleOrDefault();
                        if (level2Dep == null)
                        {
                            infoDiffer = true;

                            var dic = new Dictionary<string, object>();
                            level2Dep = new DepartmentModel();
                            level2Dep.PDID = DepartmentService.rootID;
                            level2Dep.DName = level2DepName;
                            level2Dep.LevelIndex = 2;
                            level2Dep.GetValues(dic);
                            level2Dep.ID = (int)helper.Insert("Department", dic, " OUTPUT inserted.id ");

                        }

                        var level3Dep = helper
                            .SelectDataTable($"select * from Department where PDID = {level2Dep.ID} and DName = '{level3DepName}' and LevelIndex = 3")
                            .Select().Select(x => new DepartmentModel().SetData(x) as DepartmentModel).SingleOrDefault();
                        if (level3Dep == null)
                        {
                            infoDiffer = true;

                            var dic = new Dictionary<string, object>();

                            level3Dep = new DepartmentModel();
                            level3Dep.PDID = level2Dep.ID;
                            level3Dep.DName = level3DepName;
                            level3Dep.LevelIndex = 3;
                            level3Dep.GetValues(dic);
                            level3Dep.ID = (int)helper.Insert("Department", dic, " OUTPUT inserted.id ");
                        }

                        var tag = helper.SelectDataTable($"select * from Tag where TagName = '{tagName}'").Select()
                            .Select(x => new TagModel().SetData(x) as TagModel).SingleOrDefault();

                        if (tag == null)
                        {
                            infoDiffer = true;

                            var dic = new Dictionary<string, object>();
                            tag = new TagModel();
                            tag.TagName = tagName;
                            tag.GetValues(dic);
                            tag.ID = (int)helper.Insert("Tag", dic, " OUTPUT inserted.id ");
                        }



                        var dt = helper.SelectDataTable($"select * from DepartmentTag where TagID = {tag.ID} and DepartmentID = {level3Dep.ID}").Select().SingleOrDefault();
                        int? departmentTagID = null;
                        if (dt == null)
                        {
                            departmentTagID = (int)helper.Insert("DepartmentTag", new Dictionary<string, object>()
                            {
                                {"DepartmentID",level3Dep.ID},
                                { "TagID",tag.ID}
                            },"OUTPUT inserted.ID");
                        }
                        else
                        {
                            departmentTagID = (int)dt["ID"];
                        }

                        var contactorDepTag = helper.SelectDataTable($"select * from ContactorDepartmentTag where DepartmentTagID = {departmentTagID} and ContactorID = {contactor.ID}").Select().SingleOrDefault();
                        if (contactorDepTag == null&&infoDiffer)
                        {
                            helper.Insert("ContactorDepartmentTag", new Dictionary<string, object>(){{"DepartmentTagID",departmentTagID},{"ContactorID",contactor.ID}});
                        }

                        
                    }
                }




                //                return Json(new ReturnResult()
                //                {
                //                    msg = "导入成功",
                //                    success = true,
                //                    status = 200
                //                });
                return Ok();
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    msg = ex.ToString(),
                    success = false,
                    status = 500
                });
            }
            finally
            {
                helper?.Dispose();
            }


        }

    }





  
}
