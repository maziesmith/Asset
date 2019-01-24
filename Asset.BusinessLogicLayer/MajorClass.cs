using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 资产一级类别MajorClass
    /// </summary>
    public class MajorClass
    {
        #region 私有成员

        private int _majorID;		        //大类ID
        private int _majorSortID;        //大类排序ID
        private string _majorName;	        //大类名称
        private DateTime _addTime;          //增加时间

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int MajorID
        {
            set
            {
                this._majorID = value;
            }
            get
            {
                return this._majorID;
            }
        }
        public int MajorSortID
        {
            set
            {
                this._majorSortID = value;
            }
            get
            {
                return this._majorSortID;
            }
        }
        public string MajorName
        {
            set
            {
                this._majorName = value;
            }
            get
            {
                return this._majorName;
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
        /// 根据参数MajorID，获取详细信息
        /// </summary>
        /// <param name="MajorID">产品大类ID</param>
        public void LoadData(int majorID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select MajorID,MajorSortID,MajorName,AddTime From MajorClass where MajorID = "
                + Convert.ToInt32(majorID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._majorID = GetSafeData.ValidateDataRow_N(dr, "MajorID");
                this._majorSortID = GetSafeData.ValidateDataRow_N(dr, "MajorSortID");
                this._majorName = GetSafeData.ValidateDataRow_S(dr, "MajorName");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 按MajorSortID排序,读取所有产品大类信息
        /// </summary>
        /// <return></return>
        public static DataView QueryMajorClass()
        {

            string strSql = "";
            strSql = "select MajorID,MajorSortID,MajorName,AddTime From MajorClass order by MajorSortID,MajorID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加产品大类
        /// </summary>
        /// <return></return>
        public void Add(Hashtable majorName)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("MajorClass", majorName);
        }

        /// <summary>
        /// 修改产品大类
        /// </summary>
        /// <param name="MajorName"></param>
        public void Update(Hashtable majorName)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where MajorID = " + this._majorID;
            db.Update("MajorClass", majorName, strCond);
        }


        /// <summary>
        /// 删除产品大类
        /// </summary>
        /// <param name="MajorName"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from MajorClass where MajorID = " + this._majorID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }
        #endregion 方法
    }
}

