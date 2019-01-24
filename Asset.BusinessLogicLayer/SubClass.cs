using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 资产二级分类SubClass。
    /// </summary>
    public class SubClass
    {
        #region 私有成员

        private int _subID;                 //小类ID
        private int _subSortID;             //小类排序ID
        private int _majorID;		        //大类ID
        private string _subName;	        //小类名称
        private int _unitsID;		        //单位ID
        private string _units;	            //单位
        private string _usefulLife;	        //使用年限
        private string _depreciationRate;	//折旧率
        private DateTime _addTime;          //增加时间

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性


        public int SubID
        {
            set
            {
                this._subID = value;
            }
            get
            {
                return this._subID;
            }
        }
        public int SubSortID
        {
            set
            {
                this._subSortID = value;
            }
            get
            {
                return this._subSortID;
            }
        }
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
        public string SubName
        {
            set
            {
                this._subName = value;
            }
            get
            {
                return this._subName;
            }
        }
        public int UnitsID
        {
            set
            {
                this._unitsID = value;
            }
            get
            {
                return this._unitsID;
            }
        }
        public string Units
        {
            set
            {
                this._units = value;
            }
            get
            {
                return this._units;
            }
        }
        public string UsefulLife
        {
            set
            {
                this._usefulLife = value;
            }
            get
            {
                return this._usefulLife;
            }
        }
        public string DepreciationRate
        {
            set
            {
                this._depreciationRate = value;
            }
            get
            {
                return this._depreciationRate;
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
        /// 根据参数SubID，获取详细信息
        /// </summary>
        /// <param name="SubID">产品大类ID</param>
        public void LoadData(int subID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select SubID,SubSortID,MajorID,SubName,UnitsID,Units,UsefulLife,DepreciationRate,AddTime From SubClass where SubID = "
                + Convert.ToInt32(subID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._subID = GetSafeData.ValidateDataRow_N(dr, "SubID");
                this._subSortID = GetSafeData.ValidateDataRow_N(dr, "SubSortID");
                this._majorID = GetSafeData.ValidateDataRow_N(dr, "MajorID");
                this._subName = GetSafeData.ValidateDataRow_S(dr, "SubName");
                this._unitsID = GetSafeData.ValidateDataRow_N(dr, "UnitsID");
                this._units = GetSafeData.ValidateDataRow_S(dr, "Units");
                this._usefulLife = GetSafeData.ValidateDataRow_S(dr, "UsefulLife");
                this._depreciationRate = GetSafeData.ValidateDataRow_S(dr, "DepreciationRate");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }


        /// <summary>
        /// 根据参数majorID，获取详细信息
        /// </summary>
        /// <param name="SubID">产品大类ID</param>
        public void LoadData1(int majorID,string subName)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select SubID,SubSortID,MajorID,SubName,UnitsID,Units,UsefulLife,DepreciationRate,AddTime From SubClass where MajorID=" + majorID + " and SubName = "
                + SqlStringConstructor.GetQuotedString(subName);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._subID = GetSafeData.ValidateDataRow_N(dr, "SubID");
                this._subSortID = GetSafeData.ValidateDataRow_N(dr, "SubSortID");
                this._majorID = GetSafeData.ValidateDataRow_N(dr, "MajorID");
                this._subName = GetSafeData.ValidateDataRow_S(dr, "SubName");
                this._unitsID = GetSafeData.ValidateDataRow_N(dr, "UnitsID");
                this._units = GetSafeData.ValidateDataRow_S(dr, "Units");
                this._usefulLife = GetSafeData.ValidateDataRow_S(dr, "UsefulLife");
                this._depreciationRate = GetSafeData.ValidateDataRow_S(dr, "DepreciationRate");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 按SubSortID排序,读取所有产品小类信息
        /// </summary>
        /// <return></return>
        public static DataView QuerySubClass()
        {

            string strSql = "";
            //strSql = "select SubID,SubSortID,MajorID,SubName,AddTime From SubClass order by SubSortID";
            strSql = "select SC.SubID,SC.SubSortID,SC.MajorID,MC.MajorName,SC.SubName,SC.UnitsID,SC.Units,SC.UsefulLife,SC.DepreciationRate,SC.AddTime from MajorClass MC inner join SubClass SC on MC.MajorID=SC.MajorID order by SC.SubSortID,SC.SubID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按SubSortID排序,读取所有产品小类信息
        /// </summary>
        /// <return></return>
        public static DataView QuerySubClass(int majorID)
        {

            string strSql = "";
            strSql = "select SubID,SubSortID,MajorID,SubName,UnitsID,Units,UsefulLife,DepreciationRate,AddTime From SubClass where MajorID= " + majorID + " order by SubSortID";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加产品小类
        /// </summary>
        /// <return></return>
        public void Add(Hashtable subName)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("SubClass", subName);
        }

        /// <summary>
        /// 修改产品小类
        /// </summary>
        /// <param name="SubName"></param>
        public void Update(Hashtable subName)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where SubID = " + this._subID;
            db.Update("SubClass", subName, strCond);
        }


        /// <summary>
        /// 删除产品小类
        /// </summary>
        /// <param name="SubName"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from SubClass where SubID = " + this._subID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }
        #endregion 方法
    }
}

