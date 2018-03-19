using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSPlatform.Models
{
    public interface IDataRowModel
    {
        IDataRowModel SetData(DataRow row);
    }


    public abstract class DataRowModel : IDataRowModel
    {
        public virtual int? ID
        {
            get { return dataPool["ID"] == null || dataPool["ID"] == DBNull.Value ? null : new int?((int)dataPool["ID"]); }
            set { dataPool["ID"] = value; }
        }

        protected Dictionary<string, object> dataPool = new Dictionary<string, object>();

        public IDataRowModel SetData(DataRow row)
        {

            if (row != null)
            {
                for (int index = 0; index < row.Table.Columns.Count; index++)
                {
                    string key = row.Table.Columns[index].ColumnName;
                    //                dataPool[key] = new Regex("\\S+.*").Match(row[key] + "").Value;
                    dataPool[key] = row[key];
                }
            }

            return this;
        }

        public object GetData(string key)
        {
            return dataPool.ContainsKey(key) ? dataPool[key] : null;
        }

        public void GetValues(DataRow row)
        {
            foreach (var kv in dataPool)
            {
                row[kv.Key] = dataPool[kv.Key];
            }
        }

        /// <summary>
        /// 注意获取的值如果是引用类型  需要先复制对象才能赋值
        /// </summary>
        /// <param name="dic"></param>
        public void GetValues(Dictionary<string, object> dic)
        {
            foreach (KeyValuePair<string, object> keyValuePair in dataPool)
            {
                dic.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public DataTable GetSchemaTable()
        {
            DataTable dt = new DataTable();

            foreach (KeyValuePair<string, object> keyValuePair in dataPool)
            {
                dt.Columns.Add(keyValuePair.Key);
            }
            return dt;
        }

       

    }
}
