﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAccessHelper;
using SMSPlatform.Models;

namespace SMSPlatform.Services
{
    public class DepartmentService
    {
        private SqlHelper helper;

        public static readonly int rootID = 656;

        public DepartmentService(SqlHelper helper)
        {
            this.helper = helper;
        }
        //获取所有部门递归结果，包括学校(根节点)
        public List<DepModel> GetAll()
        {
            //把学校加上
            List<DepModel> returnDepModels = new List<DepModel>()
            {
                new DepModel()
                {
                    Dep = helper.SelectDataTable($"select * from Department where id = {rootID}").Select().Select(x=>(DepartmentModel)new DepartmentModel().SetData(x)).SingleOrDefault(),
                    children=returnDepList(rootID)
                }
            };
            return returnDepModels;
        }
        /// <summary>
        /// 获取不包括学校(根节点)的部门
        /// </summary>
        /// <returns></returns>
        public object GetAllChild()
        {
            return returnDepList(rootID);
        }

        /// <summary>
        /// 获取传入部门的子部门。
        /// </summary>
        /// <param name="DID"></param>
        /// <returns>返回值不包括传入的部门</returns>
        public object GetAllChild(int id)
        {
            return returnDepList(id);
        }


        private List<DepModel> returnDepList(int parentID)
        {
            List<DepModel> list = new List<DepModel>();
            List<DepartmentModel> tempList = helper.SelectDataTable($"select * from Department where PDID = {parentID}")
                .Select().Select(x => (DepartmentModel) new DepartmentModel().SetData(x)).ToList();


            DepModel depModel;
            foreach (var item in tempList)
            {
                depModel = new DepModel();
                depModel.Dep = item;
                if (helper.SelectScalar<int>("select count(1) from Department where PDID = " + item.ID) > 0)
                {
                    depModel.children = returnDepList(item.ID.Value);
                }
                list.Add(depModel);
            }
            return list;
        }

        public class DepModel
        {
            public DepartmentModel Dep { get; set; }
            //public string Type { get; set; }
            //public bool selected { get; set; }
            public List<DepModel> children { get; set; }
        }
    }
}
