using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 事业部类Division
    /// </summary>
    public class Division
    {
        #region 私有成员

        private int _divisionID;		            //事业部ID
        private int _divisionSortID;                //事业部排序ID
        private string _divisionName;	            //事业部名称
        private DateTime _addTime;                  //增加时间

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

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
        public int DivisionSortID
        {
            set
            {
                this._divisionSortID = value;
            }
            get
            {
                return this._divisionSortID;
            }
        }
        public string DivisionName
        {
            set
            {
                this._divisionName = value;
            }
            get
            {
                return this._divisionName;
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
        /// 根据参数DivisionID，获取详细信息
        /// </summary>
        /// <param name="divisionID">事业部ID</param>
        public void LoadData(int divisionID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select DivisionID,DivisionSortID,DivisionName,AddTime From Divisions where divisionID = "
                + Convert.ToInt32(divisionID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._divisionSortID = GetSafeData.ValidateDataRow_N(dr, "DivisionSortID");
                this._divisionName = GetSafeData.ValidateDataRow_S(dr, "DivisionName");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数DivisionID，获取详细信息
        /// </summary>
        /// <param name="DivisionName">事业部名称</param>
        public void LoadData1(string divisionName)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select DivisionID,DivisionSortID,DivisionName,AddTime From Divisions where DivisionName = "
                + SqlStringConstructor.GetQuotedString(divisionName);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._divisionSortID = GetSafeData.ValidateDataRow_N(dr, "DivisionSortID");
                this._divisionName = GetSafeData.ValidateDataRow_S(dr, "DivisionName");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }



        /// <summary>
        /// 按DivisionSortID排序,读取所有事业部名称
        /// </summary>
        /// <return></return>
        public static DataView QueryDivision()
        {

            string strSql = "";
            strSql = "select DivisionID,DivisionSortID,DivisionName,AddTime From Divisions order by DivisionSortID,DivisionID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加事业部名称
        /// </summary>
        /// <return></return>
        public void Add(Hashtable division)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("Divisions", division);
        }

        /// <summary>
        /// 修改事业部名称
        /// </summary>
        /// <param name="division"></param>
        public void Update(Hashtable division)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where DivisionID = " + this._divisionID;
            db.Update("Divisions", division, strCond);
        }

        /// <summary>
        /// 删除事业部名称
        /// </summary>
        /// <param name="division"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from Divisions where DivisionID = " + this._divisionID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        #endregion 方法
    }
}