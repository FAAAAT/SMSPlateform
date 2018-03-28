using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DataBaseAccessHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class TagController : ApiController
    {

        [HttpGet]
        public IHttpActionResult GetTags(string tagName = null)
        {
            SqlHelper helper = new SqlHelper();
            try
            {
                var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
                conn.Open();

                helper.SetConnection(conn);
                TagService service = new TagService(helper);

                var models = service.GetTags(tagName).Select(x => new
                {
                    name = x.TagName,
                    id = x.ID,
                    count = helper.SelectDataTable($"select *,ContactorDepartmentTag.ID as CDTID,ContactorID from DepartmentTag inner join ContactorDepartmentTag on DepartmentTag.ID = ContactorDepartmentTag.DepartmentTagID where TagID = {x.ID}").Select().Length
                }).ToList();

                return Json(new ReturnResult()
                {
                    success = true,
                    data = models,
                    status = 200
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = e.ToString(),
                    status = 500
                });
            }
            finally
            {
                helper.Dispose();
            }

        }

        [HttpGet]
        public IHttpActionResult AddTag(string tagName)
        {
            SqlHelper helper = new SqlHelper();
            try
            {
                if (string.IsNullOrWhiteSpace(tagName))
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg = "请输入标签名称",
                    });
                }


                var conno = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
                conno.Open();
                helper.SetConnection(conno);
                TagService service = new TagService(helper);

                service.AddTag(new TagModel() { TagName = tagName });

                return Json(new ReturnResult()
                {
                    msg = "添加成功",
                    success = true,
                    status = 200
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = e.ToString(),
                    status = 500
                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpGet]
        public IHttpActionResult DeleteTag(int tagID)
        {
            SqlHelper helper = new SqlHelper();
            try
            {
                var conno = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
                conno.Open();
                helper.SetConnection(conno);
                TagService service = new TagService(helper);

                service.DeleteTag(new TagModel() { ID = tagID });

                return Json(new ReturnResult()
                {
                    msg = "删除成功",
                    success = true,
                    status = 200
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = e.ToString(),
                    status = 500
                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpGet]
        public IHttpActionResult UpdateTag([FromUri]TagModel model)
        {
            SqlHelper helper = new SqlHelper();
            try
            {
                var conno = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
                conno.Open();
                helper.SetConnection(conno);
                TagService service = new TagService(helper);

                service.UpdateTag(model);

                return Json(new ReturnResult()
                {
                    msg = "修改成功",
                    success = true,
                    status = 200
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = e.ToString(),
                    status = 500
                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpGet]
        public IHttpActionResult Select2GetTags(int? id = null)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            var helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                var service = new TagService(helper);
                Select2Model result;
                if (id == null)
                {
                    result = new Select2Model()
                    {
                        results = service.GetTags("").Select(x => new Select2Item() { id = x.ID ?? 0, text = x.TagName })
                    };


                }
                else
                {
                    result = service.GetSelect2ModelsBycontactorID(id + "");
                }
                return Json(new ReturnResult()
                {
                    success = true,
                    data = result,
                    status = 200,

                });


            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status = 500,
                });
            }
            finally
            {
                helper.Dispose();

            }
        }

        [HttpGet]
        public IHttpActionResult Select2GetDepartmentTags(int? id = null)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            var helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                var service = new TagService(helper);
                Select2Model result;
                if (id == null)
                {
                    result = service.GetDepartmentTagsByContactorID(id + "");
                }
                else
                {
                    result = service.GetDepartmentTagsByContactorID(id + "");
                }
                return Json(new ReturnResult()
                {
                    success = true,
                    data = result,
                    status = 200,

                });


            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status = 500,
                });
            }
            finally
            {
                helper.Dispose();

            }
        }

        [HttpPost]
        public IHttpActionResult GetDepartmentTags()
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            var helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
//                var dt = helper.SelectDataTable("select * from DepartmentTag " + (departmentId.HasValue ? $" where DepartmentID = {departmentId}" : ""));
                var dt = helper.SelectDataTable("select * from DepartmentTag " );
                var dtDepartment = helper.SelectDataTable($"select * from Department where ID in ({string.Join(",", dt.Select().Select(x => x["DepartmentID"] + ""))})");
                var dtTag = helper.SelectDataTable($"select * from Tag where ID in ({string.Join(",", dt.Select().Select(x => x["TagID"] + ""))})");


                var allDepModels = helper.SelectDataTable("select * from Department").Select()
                    .Select(x => new DepartmentModel().SetData(x) as DepartmentModel);

                var depModel = dtDepartment.Select().Select(x => new DepartmentModel().SetData(x) as DepartmentModel);
                var tagModel = dtTag.Select().Select(x => new TagModel().SetData(x) as TagModel);

                var UserCountDt =
                    helper.SelectDataTable(
                        "select count(1) as CCount,DepartmentTagID from ContactorDepartmentTag Group By DepartmentTagID").Select();


                return Json(new ReturnResult()
                {
                    success = true,
                    data = dt.Select().Select(x => new {
                        id = (int)x["ID"],
                        dep = new {self= depModel.SingleOrDefault(y => y.ID == (int)x["DepartmentID"]) , parent = allDepModels.SingleOrDefault(y=>y.ID == depModel.SingleOrDefault(z => z.ID == (int)x["DepartmentID"]).PDID) },
                        tag = tagModel.SingleOrDefault(y => y.ID == (int)x["TagID"]),
                        count = UserCountDt.SingleOrDefault(y => (int)y["DepartmentTagID"] == (int)x["ID"])!=null? UserCountDt.SingleOrDefault(y => (int)y["DepartmentTagID"] == (int)x["ID"])["CCount"]:0 }),
                    status = 200,

                });


            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status = 500,

                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpPost]
        public IHttpActionResult GetDepartmentTagById()
        {

            var json =Request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var jToken= JToken.Parse(json);
            int id = (int)jToken["id"];



            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            var helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {

                var dt =  helper.SelectDataTable($"select * from DepartmentTag where ID = {id}");
                var data = dt.Select().Select(x => new { id = (int)x["ID"],depId = (int)x["DepartmentID"],tagId = (int)x["TagID"] }).SingleOrDefault();

               

                return Json(new ReturnResult()
                {
                    success = true,
                    data = data,
                    status = 200,
                });


            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status = 500,

                });
            }
            finally
            {
                helper.Dispose();
            }
        }
        
        [HttpPost]
        public IHttpActionResult AddDepartmentTag()
        {
            var content = Request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var nvs = new FormDataCollection(content);
            int? depId = string.IsNullOrWhiteSpace(nvs["depId"]) ?new int?(): int.Parse(nvs["depId"]);
            int? tagId = string.IsNullOrWhiteSpace(nvs["tagId"]) ? new int?() : int.Parse(nvs["tagId"]);

           


            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            
            var helper = new SqlHelper();
            conn.Open();
            helper.SetConnection(conn);



            try
            {

                if (!depId.HasValue)
                {
                    throw new Exception("请选择一个部门");
                }
                if (!tagId.HasValue)
                {
                    throw new Exception("请选择一个标签");

                }

                var depTags = helper.SelectDataTable($"select * from DepartmentTag where DepartmentID = {depId} and TagID = {tagId}").Select();
                
                if (depTags.Length !=0)
                {

                    return Json(new ReturnResult()
                    {
                        success =  false,
                        msg = "已经存在该标签",
                        status = 200,
                    });

                }



                TagService service = new TagService(helper);
                service.AddDepartmentTag(depId.Value, tagId.Value);
                return Json(new ReturnResult()
                {
                    success = true,

                    status = 200,
                });
            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = e.ToString(),
                    status = 500,
                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpGet]
        public IHttpActionResult DeleteDepartemntTag(int depTagId)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            var helper = new SqlHelper();
            conn.Open();
            helper.SetConnection(conn);

            try
            {
                helper.Delete("ContactorDepartmentTag", $"DepartmentTagID = {depTagId} ");
                helper.Delete("DepartmentTag", $"ID={depTagId}");
                return Json(new ReturnResult()
                {
                    success = true,
                    status = 500,
                });
            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status
                    = 500,
                });
            }
            finally
            {
                helper.Dispose();
            }

        }

        [HttpGet]
        public IHttpActionResult UpdateDepartmentTag(int depTagId, int depId, int tagId)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            var helper = new SqlHelper();
            conn.Open();
            helper.SetConnection(conn);
            try
            {

                var exists = helper.SelectDataTable($"select * from DepartmentTag where TagID = {tagId} and DepartmentID = {depId} and ID <> {depTagId}").Select();
                if (exists.Any())
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg="已存在该标签",
                    });
                }

                helper.Update("DepartmentTag", new Dictionary<string, object>() { { "DepartmentID", depId }, { "TagID", tagId } }, $" ID = {depTagId}", new List<SqlParameter>());


                return Json(new ReturnResult()
                {
                    success = true,
                    status = 500,
                });

            }
            catch (Exception e)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = e.ToString(),
                    status = 500,
                });
            }
            finally
            {
                helper.Dispose();
            }

        }

        [HttpGet]
        public IHttpActionResult GetContactorsByDepartmentTagID(int depTagID)
        {
            var conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            var helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {

                var contactorIds =
                    helper.SelectDataTable(
                        $"select ContactorID from ContactorDepartmentTag where DepartmentTagID = {depTagID}").Select();
                var contactors = helper.SelectDataTable(
                    contactorIds.Any() ?
                    $"select * from Contactor where ID in ({string.Join(",", contactorIds.Select(x => x["ContactorID"] + ""))})" : "select * from Contactor where 1=0").Select().Select(x => new contactorModel().SetData(x) as contactorModel);

                

                return Json(new ReturnResult()
                {
                    success = true,
                    data = contactors,
                    status = 200,

                });


            }
            catch (Exception ex)
            {
                return Json(new ReturnResult()
                {
                    success = false,
                    msg = ex.ToString(),
                    status = 500,

                });
            }
            finally
            {
                helper.Dispose();
            }
        }

        

    }
}
