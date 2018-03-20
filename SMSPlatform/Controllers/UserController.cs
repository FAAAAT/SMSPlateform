using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using DataBaseAccessHelper;
using Logger;
using SMSPlatform.Models;
using SMSPlatform.Services;

namespace SMSPlatform.Controllers
{
    public class UserController : ApiController
    {
        private SMSPlatformLogger logger = AppDomain.CurrentDomain.GetData("Logger") as SMSPlatformLogger;

        [HttpGet]
        [LymiAuthorize(Users = "admin")]

        public IHttpActionResult GetUsers(string userName, int[] roleIDs, int? id = null)
        {
            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                var initWhere = " where ";
                var whereStr = initWhere;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    whereStr += (whereStr == initWhere ? "" : " and ") + $" UserName like '%{userName}'";
                }

                if (roleIDs != null && roleIDs.Any())
                {
                    var roles =
                        helper.SelectDataTable(
                            $"select * from UserRoleReference where RoleID in ({string.Join(",", roleIDs)})").Select();

                    whereStr += (whereStr == initWhere ? "" : " and ") +
                                $" ID in ({string.Join(",", roles.Select(x => x["UserID"]))})";
                }

                if (id.HasValue)
                {
                    whereStr += (whereStr == initWhere ? "" : " and ") + $" ID = {id}";
                }


                var userModels =
                        helper.SelectDataTable("select * from [User] " + (whereStr == initWhere ? "" : whereStr))
                            .Select()
                            .Select(x => new UserModel().SetData(x) as UserModel);

                var refs = helper.SelectDataTable("select * from UserRoleReference").Select();

                var roleModels =
                    helper.SelectDataTable("select * from Role").Select().Select(x => new RoleModel().SetData(x) as RoleModel);

                var datas = userModels.Select(x => new UserViewModel()
                {
                    User = x,
                    Roles =
                        refs.Where(y => (int)y["UserID"] == x.ID)
                            .Join(roleModels, y => (int)y["RoleID"], y => y.ID, (y, z) => z)
                            .ToArray(),
                });

                return Json(new ReturnResult()
                {
                    success = true,
                    data = datas,
                });

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),

#else
                    msg = "内部错误 请联系管理员",
                    
#endif
                    success = false,

                });

            }
            finally
            {
                helper.Dispose();
            }
        }


        [HttpPost]
        [LymiAuthorize(Users = "admin")]

        public IHttpActionResult AddUser(UserModel model)
        {
            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);

            try
            {
                if (model != null)
                {
                    var exists = helper.SelectDataTable($"select * from [User] where UserName = '{model.UserName}'").Select();
                    if (exists.Any())
                    {
                        return Json(new ReturnResult()
                        {
                            success = false,
                            msg = "已存在相同用户名用户"
                        });
                    }
                    var dic = new Dictionary<string, object>();
                    model.GetValues(dic);
                    dic.Remove("ID");
                    int id = (int)helper.Insert("User", dic, "OUTPUT inserted.id");
                    helper.Insert("UserRoleReference", new Dictionary<string, object>()
                    {
                        { "UserID",id},
                        { "RoleID",1}

                    });


                    return Json(new ReturnResult()
                    {
                        success = true,
                    });
                }
                else
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg = "未获取到可用的提交数据"
                    });
                }
            }

            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),

#else
                    msg = "内部错误 请联系管理员",
                    
#endif
                    success = false,

                });

            }
            finally
            {
                helper.Dispose();
            }
        }

        [HttpPost]
        [LymiAuthorize(Users = "admin")]
        public IHttpActionResult UpdateUser(UserModel model)
        {
            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                if (!model.ID.HasValue)
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg = "未检测到可用提交数据"
                    });
                }

                var dic = new Dictionary<string, object>();
                model.GetValues(dic);
                dic.Remove("ID");
                helper.Update("[User]", dic, $" ID = {model.ID}", new List<SqlParameter>());
                return Json(new ReturnResult() { success = true });
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),

#else
                    msg = "内部错误 请联系管理员",
                    
#endif
                    success = false,

                });
            }


            finally
            {
                helper.Dispose();
            }
        }

        [HttpGet]
        [LymiAuthorize(Users = "admin")]
        public IHttpActionResult DeleteUser(int id)
        {

            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                if (id == 1)
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg = "不能删除内置管理员账户"
                    });

                }

                helper.Delete("[User]", $" ID = {id}");
                helper.Delete("UserRoleReference", $" UserID = {id}");
                return Json(new ReturnResult()
                {
                    success = true,
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),

#else
                    msg = "内部错误 请联系管理员",
                    
#endif
                    success = false,

                });
            }
            finally
            {
                helper.Dispose();

            }
        }

        [HttpGet]
        [LymiAuthorize(Users = "*")]
        public IHttpActionResult GetUserDetail()
        {
            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {

                var ID = int.Parse((this.RequestContext.Principal.Identity as ClaimsIdentity).Claims.SingleOrDefault(x => x.Type == "UserID")?.Value);

                var userModel = helper.SelectDataTable($"select * from [User] where ID = {ID}").Select().Select(x => new UserModel().SetData(x) as UserModel).SingleOrDefault();



                return Json(new ReturnResult()
                {
                    success = true,
                    data = userModel,
                });


            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),

#else
                    msg = "内部错误 请联系管理员",
                    
#endif
                    success = false,

                });
            }
            finally
            {
                helper.Dispose();

            }


        }

        [HttpPost]
        public IHttpActionResult UpdateUser_Normal(UserModel model)
        {
            SqlConnection conn = new SqlConnection(ConnectionStringUtility.DefaultConnectionStrings);
            conn.Open();
            SqlHelper helper = new SqlHelper();
            helper.SetConnection(conn);
            try
            {
                if (!model.ID.HasValue)
                {
                    return Json(new ReturnResult()
                    {
                        success = false,
                        msg = "未检测到可用提交数据"
                    });
                }


                model.ID = int.Parse((this.RequestContext.Principal.Identity as ClaimsIdentity).Claims.SingleOrDefault(x => x.Type == "UserID")?.Value);

                var dic = new Dictionary<string, object>();
                model.GetValues(dic);
                dic.Remove("ID");
                dic.Remove("Password");
                dic.Remove("UserName");
                helper.Update("[User]", dic, $" ID = {model.ID}", new List<SqlParameter>());
                return Json(new ReturnResult() { success = true });
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return Json(new ReturnResult()
                {
#if DEBUG
                    msg = ex.ToString(),

#else
                    msg = "内部错误 请联系管理员",
                    
#endif
                    success = false,

                });
            }


            finally
            {
                helper.Dispose();
            }

        }



    }
}
