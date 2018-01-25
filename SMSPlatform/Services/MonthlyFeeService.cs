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
    public class FeeService
    {
        private SqlHelper helper;

        public FeeService(SqlHelper helper)
        {
            this.helper = helper;
        }

        public IEnumerable<DailyFeeRecordModel> GetRecords(string phone, DateTime? date)
        {
            var whereStr = " where 1=1 ";
            if (!string.IsNullOrWhiteSpace(phone))
            {
                whereStr += $" and PhoneNumber like '%{phone}%' ";
            }

            if (date.HasValue)
            {
                whereStr += $" and RecordDate = '{date:yyyy-MM-dd}'";
            }


            //            return helper.SelectDataTable("select * from MonthlyFeeRecord " + whereStr).Select().Select(x => new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel);
            return null;

        }

        public bool SMSSend(string phone, DateTime date, int count, out string error)
        {
            var nowDate = DateTime.Now;
            error = null;
            if (string.IsNullOrWhiteSpace(phone))
            {
                error = "电话不能为空";
                return false;
            }


            var monthlyModel = helper.SelectDataTable(
                $"select * from MonthlyFeeRecord where PhoneNumber = '{phone}' and Month = {nowDate.Month} and Year = {nowDate.Year} ").Select().Select(x => new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel).SingleOrDefault();

            if (monthlyModel == null)
            {
                var limitObj = helper.SelectDataTable($"selct * from systemsettings where PhoneNumber = '{phone}'").Select().FirstOrDefault()?["MonthTotalCountLimit"];
                if (limitObj == null)
                {
                    error = "还未设定该卡限额";
                    return false;
                }

                var dic = new Dictionary<string, object>();
                monthlyModel = new MonthlyFeeRecordModel();
                monthlyModel.PhoneNumber = phone;
                monthlyModel.MonthLimitRecord = (int)limitObj;
                monthlyModel.Month = nowDate.Month;
                monthlyModel.Year = nowDate.Year;
                monthlyModel.SendCount = count;
                monthlyModel.GetValues(dic);
                helper.Insert("MonthlyFeeRecord", dic);

            }
            else
            {

                helper.Update("MonthlyFeeRecord", new Dictionary<string, object>()
                    {
                        {"SendCount",monthlyModel.SendCount+count }
                    }, $" ID = {monthlyModel.ID}",
                    new List<SqlParameter>());

            }

            var dailyModel = helper.SelectDataTable($"select * from DailyFeeRecord where PhoneNumber = '{phone}' and Date = '{nowDate.Date:yyyy-MM-dd}'").Select().Select(x => new DailyFeeRecordModel().SetData(x) as DailyFeeRecordModel).SingleOrDefault();
            if (dailyModel == null)
            {
                var dic = new Dictionary<string, object>();
                dailyModel = new DailyFeeRecordModel();
                dailyModel.PhoneNumber = phone;
                dailyModel.Date = nowDate.Date;
                dailyModel.SendCount = count;
                dailyModel.GetValues(dic);
                helper.Insert("DailyFeeRecord", dic);
            }
            else
            {
                helper.Update("DailyFeeRecord", new Dictionary<string, object>()
                {
                    {"SendCount", dailyModel.SendCount + count}
                }, $"ID={dailyModel.ID}", new List<SqlParameter>());
            }

            return true;
        }


        public bool UpdateOrAddMonthlyLimit(string phone, int limit, out string error)
        {


            var nowDate = DateTime.Now.Date;
            error = null;
            if (string.IsNullOrWhiteSpace(phone))
            {
                error = "电话不能为空";
                return false;
            }
            var setting = helper.SelectDataTable($"select * from systemsettings where PhoneNumber = '{phone}'").Select().FirstOrDefault();
            if (setting == null)
            {
                helper.Insert("systemsettings", new Dictionary<string, object>()
                {
                    { "PhoneNumber",phone},
                    {"MonthTotalCountLimit",limit }
                });
            }
            else
            {
                var conn = helper.GetOpendSqlConnection();
                var tran = conn.BeginTransaction();
                helper.SetTransaction(tran);
                try
                {
                    var monthlyModel = helper
                        .SelectDataTable(
                            $"selct * from MonthlyFeeRecord where phonenumber ='{phone}' and Month = {nowDate.Month} and Year = {nowDate.Year}")
                        .Select().Select(x => new MonthlyFeeRecordModel().SetData(x) as MonthlyFeeRecordModel)
                        .SingleOrDefault();
                    if (monthlyModel != null)
                    {
                        monthlyModel.MonthLimitRecord = limit;
                    }


                    helper.Update("SystemSettings", new Dictionary<string, object>()
                    {
                        {"MonthTotalCountLimit", limit}

                    }, $"ID={setting["ID"]}", new List<SqlParameter>());

                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    error = e.ToString();
                    return false;
                }
                finally
                {
                    helper.ClearTransaction();

                }


            }

            return true;
        }


    }
}
