using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 部门类Department
    /// </summary>
    public class Department
    {
        #region 私有成员

        private int _departmentID;		            //部门ID
        private int _departmentSortID;              //部门排序ID
        private int _divisionID;		            //事业部ID
        private string _departmentName;	            //部门名称
        private DateTime _addTime;                  //增加时间

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int DepartmentID
        {
            set
            {
                this._departmentID = value;
            }
            get
            {
                return this._departmentID;
            }
        }
        public int DepartmentSortID
        {
            set
            {
                this._departmentSortID = value;
            }
            get
            {
                return this._departmentSortID;
            }
        }
        public int DivisionID
        {
            set
            {
                this._divisionID = value;
            }
            get
            {
                return this._divisionID;
            }
        }
        public string DepartmentName
        {
            set
            {
                this._departmentName = value;
            }
            get
            {
                return this._departmentName;
            }
        }
        public DateTime AddTime
        {
            set
            {
                this._addTime = value;
            }
            get
            {
                return this._addTime;
            }
        }
        public bool Exist
        {
            get
            {
                return this._exist;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 根据参数DepartmentID，获取详细信息
        /// </summary>
        /// <param name="DepartmentID">部门ID</param>
        public void LoadData(int departmentID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select DepartmentID,DepartmentSortID,DivisionID,DepartmentName,AddTime From Departments where DepartmentID = "
                + Convert.ToInt32(departmentID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._departmentID = GetSafeData.ValidateDataRow_N(dr, "DepartmentID");
                this._departmentSortID = GetSafeData.ValidateDataRow_N(dr, "DepartmentSortID");
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._departmentName = GetSafeData.ValidateDataRow_S(dr, "DepartmentName");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数DepartmentName，获取详细信息
        /// </summary>
        /// <param name="DepartmentName">部门名称</param>
        public void LoadData1(string departmentName)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select DepartmentID,DepartmentSortID,DivisionID,DepartmentName,AddTime From Departments where DepartmentName = "
                + SqlStringConstructor.GetQuotedString(departmentName);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._departmentID = GetSafeData.ValidateDataRow_N(dr, "DepartmentID");
                this._departmentSortID = GetSafeData.ValidateDataRow_N(dr, "DepartmentSortID");
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._departmentName = GetSafeData.ValidateDataRow_S(dr, "DepartmentName");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数divisionID,DepartmentName，获取详细信息
        /// </summary>
        /// <param name="DepartmentName">部门名称</param>
        public void LoadData2(int divisionID,string departmentName)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select DepartmentID,DepartmentSortID,DivisionID,DepartmentName,AddTime From Departments where DivisionID="+ divisionID +" and DepartmentName = "
                + SqlStringConstructor.GetQuotedString(departmentName);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._departmentID = GetSafeData.ValidateDataRow_N(dr, "DepartmentID");
                this._departmentSortID = GetSafeData.ValidateDataRow_N(dr, "DepartmentSortID");
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._departmentName = GetSafeData.ValidateDataRow_S(dr, "DepartmentName");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }



        /// <summary>
        /// 按DepartmentSortID排序,读取所有部门名称
        /// </summary>
        /// <return></return>
        public static DataView QueryDepartment()
        {

            string strSql = "";
            strSql = "select DM.DepartmentID,DM.DepartmentSortID,DM.DivisionID,D.DivisionName,DM.DepartmentName,DM.AddTime from Departments DM inner join Divisions D on DM.DivisionID=D.DivisionID order by DM.DepartmentSortID,DM.DepartmentID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按DepartmentSortID排序,读取所有部门名称
        /// </summary>
        /// <return></return>
        public static DataView QueryDepartment(int divisionID)
        {

            string strSql = "";
            strSql = "Select DepartmentID,DepartmentSortID,DivisionID,DepartmentName,AddTime From Departments where DivisionID=" + divisionID + " order by DepartmentSortID,DepartmentID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加部门名称
        /// </summary>
        /// <return></return>
        public void Add(Hashtable department)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("Departments", department);
        }

        /// <summary>
        /// 修改部门名称
        /// </summary>
        /// <param name="division"></param>
        public void Update(Hashtable department)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where DepartmentID = " + this._departmentID;
            db.Update("Departments", department, strCond);
        }

        /// <summary>
        /// 删除部门名称
        /// </summary>
        /// <param name="division"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from Departments where DepartmentID = " + this._departmentID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        #endregion 方法
    }
}