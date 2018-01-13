using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DataBaseAccessHelper;
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
                    count = helper.SelectScalar<int>($"select count(1) from TagContactor where TagID = {x.ID}")
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
                        results = service.GetTags("").Select(x => new Select2Item() {id = x.ID, text = x.TagName})
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
    }
}
