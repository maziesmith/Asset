using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 固定资产类FixedAsset
    /// </summary>
    public class FixedAsset
    {
        #region 私有成员

        private int _fixedAssetsID;		        //固定资产ID
        private string _assetsCoding;	        //资产编码
        private string _assetName;	            //资产名称
        private int _majorID;                   //资产一级类别ID
        private string _majorName;	            //资产一级类别名称
        private int _subID;                     //资产二级类别ID
        private string _subName;	            //资产二级类别名称
        private string _specificationsModel;	//规格型号
        private string _brand;                  //品牌名称
        private string _manufacturer;	        //生产厂家
        private int _unitsID;                   //计量单位ID
        private string _unitsName;	            //计量单位名称
        private int _useSituationID;            //使用情况ID
        private int _divisionID;                //事业部ID
        private string _divisionName;	        //事业部名称
        private int _departmentID;              //部门ID
        private string _departmentName;	        //部门名称
        private string _userAccount;	        //保管人员工号
        private string _contactor;	            //保管人员名字
        private int _addWaysID;                 //增加方式ID
        private string _addWaysName;	        //增加方式名称
        //private string _originalValue;	        //原值
        private double _originalValue;
        private DateTime _exFactoryDate;        //出厂日期
        private DateTime _purchaseDate;         //购置日期
        private DateTime _recordedDate;         //入帐日期
        private int _methodID;                  //折旧方式ID
        private string _methodName;	            //折旧方式
        //private string _accumulatedDepreciation;	//累计折旧
        private double _accumulatedDepreciation;	//累计折旧
        private double _monthDepreciation;	        //本月折旧
        private double _residualValueRate;          //残值率
        private int _remainderMonth;            //剩余月份
        private double _netValue;	            //净值
        private double _residuals;	            //残值
        private int _limitedYear;               //有限年份
        private string _storageSites;	        //存放地点
        private string _assetsBackup;	        //资产备注
        private string _useUserAccount;	        //使用人员工号
        private string _useContactor;	        //使用人员名字
        private int _lowConsumables;            //低值易耗品
        private int _assetStatus;               //资产状态
        private int _isNewAdd;                  //是否新增
        private int _applyStatus;               //申请变换状态
        private string _applyUserAccount;	    //申请人工号
        private string _applyContactor;	        //申请联系人
        private DateTime _applyDate;            //申请日期
        private DateTime _addTime;              //增加日期

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int FixedAssetsID
        {
            set
            {
                this._fixedAssetsID = value;
            }
            get
            {
                return this._fixedAssetsID;
            }
        }
        public string AssetsCoding
        {
            set
            {
                this._assetsCoding = value;
            }
            get
            {
                return this._assetsCoding;
            }
        }
        public string AssetName
        {
            set
            {
                this._assetName = value;
            }
            get
            {
                return this._assetName;
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
        public string SpecificationsModel
        {
            set
            {
                this._specificationsModel = value;
            }
            get
            {
                return this._specificationsModel;
            }
        }
        public string Brand
        {
            set
            {
                this._brand = value;
            }
            get
            {
                return this._brand;
            }
        }
        public string Manufacturer
        {
            set
            {
                this._manufacturer = value;
            }
            get
            {
                return this._manufacturer;
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
        public string UnitsName
        {
            set
            {
                this._unitsName = value;
            }
            get
            {
                return this._unitsName;
            }
        }
        public int UseSituationID
        {
            set
            {
                this._useSituationID = value;
            }
            get
            {
                return this._useSituationID;
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
        public string UserAccount
        {
            set
            {
                this._userAccount = value;
            }
            get
            {
                return this._userAccount;
            }
        }
        public string Contactor
        {
            set
            {
                this._contactor = value;
            }
            get
            {
                return this._contactor;
            }
        }
        public int AddWaysID
        {
            set
            {
                this._addWaysID = value;
            }
            get
            {
                return this._addWaysID;
            }
        }
        public string AddWaysName
        {
            set
            {
                this._addWaysName = value;
            }
            get
            {
                return this._addWaysName;
            }
        }
        //public string 
        public double OriginalValue
        {
            set
            {
                this._originalValue = value;
            }
            get
            {
                return this._originalValue;
            }
        }
        public DateTime ExFactoryDate
        {
            set
            {
                this._exFactoryDate = value;
            }
            get
            {
                return this._exFactoryDate;
            }
        }
        public DateTime PurchaseDate
        {
            set
            {
                this._purchaseDate = value;
            }
            get
            {
                return this._purchaseDate;
            }
        }
        public DateTime RecordedDate
        {
            set
            {
                this._recordedDate = value;
            }
            get
            {
                return this._recordedDate;
            }
        }
        public int MethodID
        {
            set
            {
                this._methodID = value;
            }
            get
            {
                return this._methodID;
            }
        }
        public string MethodName
        {
            set
            {
                this._methodName = value;
            }
            get
            {
                return this._methodName;
            }
        }
        public double AccumulatedDepreciation
        {
            set
            {
                this._accumulatedDepreciation = value;
            }
            get
            {
                return this._accumulatedDepreciation;
            }
        }
        public double MonthDepreciation
        {
            set
            {
                this._monthDepreciation = value;
            }
            get
            {
                return this._monthDepreciation;
            }
        }
        public double ResidualValueRate
        {
            set
            {
                this._residualValueRate = value;
            }
            get
            {
                return this._residualValueRate;
            }
        }
        public int RemainderMonth
        {
            set
            {
                this._remainderMonth = value;
            }
            get
            {
                return this._remainderMonth;
            }
        }
        public double NetValue
        {
            set
            {
                this._netValue = value;
            }
            get
            {
                return this._netValue;
            }
        }
        public double Residuals
        {
            set
            {
                this._residuals = value;
            }
            get
            {
                return this._residuals;
            }
        }
        public int LimitedYear
        {
            set
            {
                this._limitedYear = value;
            }
            get
            {
                return this._limitedYear;
            }
        }
        public string StorageSites
        {
            set
            {
                this._storageSites = value;
            }
            get
            {
                return this._storageSites;
            }
        }
        public string AssetsBackup
        {
            set
            {
                this._assetsBackup = value;
            }
            get
            {
                return this._assetsBackup;
            }
        }
        public string UseUserAccount
        {
            set
            {
                this._useUserAccount = value;
            }
            get
            {
                return this._useUserAccount;
            }
        }
        public string UseContactor
        {
            set
            {
                this._useContactor = value;
            }
            get
            {
                return this._useContactor;
            }
        }
        public int LowConsumables
        {
            set
            {
                this._lowConsumables = value;
            }
            get
            {
                return this._lowConsumables;
            }
        }
        public int AssetStatus
        {
            set
            {
                this._assetStatus = value;
            }
            get
            {
                return this._assetStatus;
            }
        }
        public int IsNewAdd
        {
            set
            {
                this._isNewAdd = value;
            }
            get
            {
                return this._isNewAdd;
            }
        }
        public int ApplyStatus
        {
            set
            {
                this._applyStatus = value;
            }
            get
            {
                return this._applyStatus;
            }
        }
        public string ApplyUserAccount
        {
            set
            {
                this._applyUserAccount = value;
            }
            get
            {
                return this._applyUserAccount;
            }
        }
        public string ApplyContactor
        {
            set
            {
                this._applyContactor = value;
            }
            get
            {
                return this._applyContactor;
            }
        }
        public DateTime ApplyDate
        {
            set
            {
                this._applyDate = value;
            }
            get
            {
                return this._applyDate;
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
        /// 根据参数FixedAssetsID，获取固定资产详细信息
        /// </summary>
        /// <param name="fixedAssetsID">固定资产ID</param>
        public void LoadData(int fixedAssetsID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "select FixedAssetsID,AssetsCoding,AssetName,MajorID,MajorName,SubID,SubName,SpecificationsModel,Brand,Manufacturer,UnitsID,UnitsName,UseSituationID,DivisionID,DivisionName,DepartmentID,DepartmentName,UserAccount,Contactor,AddWaysID,AddWaysName,OriginalValue,ExFactoryDate,PurchaseDate,RecordedDate,MethodID,MethodName,AccumulatedDepreciation,MonthDepreciation,ResidualValueRate,RemainderMonth,NetValue,Residuals,LimitedYear,StorageSites,AssetsBackup,UseUserAccount,UseContactor,LowConsumables,AssetStatus,IsNewAdd,ApplyStatus,ApplyUserAccount,ApplyContactor,ApplyDate,AddTime from FixedAssets where FixedAssetsID = "
                + Convert.ToInt32(fixedAssetsID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._assetsCoding = GetSafeData.ValidateDataRow_S(dr, "AssetsCoding");
                this._assetName = GetSafeData.ValidateDataRow_S(dr, "AssetName");
                this._majorID = GetSafeData.ValidateDataRow_N(dr, "MajorID");
                this._majorName = GetSafeData.ValidateDataRow_S(dr, "MajorName");
                this._subID = GetSafeData.ValidateDataRow_N(dr, "SubID");
                this._subName = GetSafeData.ValidateDataRow_S(dr, "SubName");
                this._specificationsModel = GetSafeData.ValidateDataRow_S(dr, "SpecificationsModel");
                this._brand = GetSafeData.ValidateDataRow_S(dr, "Brand");
                this._manufacturer = GetSafeData.ValidateDataRow_S(dr, "Manufacturer");
                this._unitsID = GetSafeData.ValidateDataRow_N(dr, "UnitsID");
                this._unitsName = GetSafeData.ValidateDataRow_S(dr, "UnitsName");
                this._useSituationID = GetSafeData.ValidateDataRow_N(dr, "UseSituationID");
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._divisionName = GetSafeData.ValidateDataRow_S(dr, "DivisionName");
                this._departmentID = GetSafeData.ValidateDataRow_N(dr, "DepartmentID");
                this._departmentName = GetSafeData.ValidateDataRow_S(dr, "DepartmentName");
                this._userAccount = GetSafeData.ValidateDataRow_S(dr, "UserAccount");
                this._contactor = GetSafeData.ValidateDataRow_S(dr, "Contactor");
                this._addWaysID = GetSafeData.ValidateDataRow_N(dr, "AddWaysID");
                this._addWaysName = GetSafeData.ValidateDataRow_S(dr, "AddWaysName");
                //this._originalValue = GetSafeData.ValidateDataRow_S(dr, "OriginalValue");
                this._originalValue = GetSafeData.ValidateDataRow_F(dr, "OriginalValue");
                this._exFactoryDate = GetSafeData.ValidateDataRow_T(dr, "ExFactoryDate");
                this._purchaseDate = GetSafeData.ValidateDataRow_T(dr, "PurchaseDate");
                this._recordedDate = GetSafeData.ValidateDataRow_T(dr, "RecordedDate");
                this._methodID = GetSafeData.ValidateDataRow_N(dr, "MethodID");
                this._methodName = GetSafeData.ValidateDataRow_S(dr, "MethodName");
                this._accumulatedDepreciation = GetSafeData.ValidateDataRow_F(dr, "AccumulatedDepreciation");
                this._monthDepreciation = GetSafeData.ValidateDataRow_F(dr, "MonthDepreciation");
                //this._residualValueRate = GetSafeData.ValidateDataRow_S(dr, "ResidualValueRate");
                this._residualValueRate = GetSafeData.ValidateDataRow_F(dr, "ResidualValueRate");
                this._remainderMonth = GetSafeData.ValidateDataRow_N(dr, "RemainderMonth");
                this._netValue = GetSafeData.ValidateDataRow_F(dr, "NetValue");
                this._residuals = GetSafeData.ValidateDataRow_F(dr, "Residuals");
                this._limitedYear = GetSafeData.ValidateDataRow_N(dr, "LimitedYear");
                this._storageSites = GetSafeData.ValidateDataRow_S(dr, "StorageSites");
                this._assetsBackup = GetSafeData.ValidateDataRow_S(dr, "AssetsBackup");
                this._useUserAccount = GetSafeData.ValidateDataRow_S(dr, "UseUserAccount");
                this._useContactor = GetSafeData.ValidateDataRow_S(dr, "UseContactor");
                this._lowConsumables = GetSafeData.ValidateDataRow_N(dr, "LowConsumables");
                this._assetStatus = GetSafeData.ValidateDataRow_N(dr, "AssetStatus");
                this._isNewAdd = GetSafeData.ValidateDataRow_N(dr, "IsNewAdd");
                this._applyStatus = GetSafeData.ValidateDataRow_N(dr, "ApplyStatus");
                this._applyUserAccount = GetSafeData.ValidateDataRow_S(dr, "ApplyUserAccount");
                this._applyContactor = GetSafeData.ValidateDataRow_S(dr, "ApplyContactor");
                this._applyDate = GetSafeData.ValidateDataRow_T(dr, "ApplyDate");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;

            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数AssetsCoding，获取固定资产详细信息
        /// </summary>
        /// <param name="assetsCoding">固定资产编码</param>
        public void LoadData(string assetsCoding)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "select FixedAssetsID,AssetsCoding,AssetName from FixedAssets where AssetsCoding = "
                + SqlStringConstructor.GetQuotedString(assetsCoding);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._assetsCoding = GetSafeData.ValidateDataRow_S(dr, "AssetsCoding");
                this._assetName = GetSafeData.ValidateDataRow_S(dr, "AssetName");

                this._exist = true;

            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数SubID，获取固定资产详细信息
        /// </summary>
        /// <param name="subID">固定资产编码</param>
        public void LoadData1(int subID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "select distinct FixedAssetsID,AssetsCoding,AssetName from FixedAssets where SubID =" + Convert.ToInt32(subID) + " order by FixedAssetsID desc";

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._assetsCoding = GetSafeData.ValidateDataRow_S(dr, "AssetsCoding");
                this._assetName = GetSafeData.ValidateDataRow_S(dr, "AssetName");

                this._exist = true;

            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数divisionID，获取固定资产详细信息
        /// </summary>
        /// <param name="divisionID">固定资产编码</param>
        public void LoadDataDivisionID(int divisionID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "select distinct FixedAssetsID,AssetsCoding,AssetName from FixedAssets where DivisionID =" + Convert.ToInt32(divisionID) + " order by FixedAssetsID desc";

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._assetsCoding = GetSafeData.ValidateDataRow_S(dr, "AssetsCoding");
                this._assetName = GetSafeData.ValidateDataRow_S(dr, "AssetName");

                this._exist = true;

            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数DepartmentID，获取固定资产详细信息
        /// </summary>
        /// <param name="departmentID">固定资产编码</param>
        public void LoadDataDepartmentID(int departmentID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "select distinct FixedAssetsID,AssetsCoding,AssetName from FixedAssets where DepartmentID =" + Convert.ToInt32(departmentID) + " order by FixedAssetsID desc";

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._assetsCoding = GetSafeData.ValidateDataRow_S(dr, "AssetsCoding");
                this._assetName = GetSafeData.ValidateDataRow_S(dr, "AssetName");

                this._exist = true;

            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 按FixedAssetsID排序,读取所有固定资产信息
        /// </summary>
        /// <return></return>
        public static DataView QueryFixedAssets()
        {

            string strSql = "";
            strSql = "select FixedAssetsID,AssetsCoding,AssetName,SpecificationsModel,Manufacturer,UserAccount,Contactor,PurchaseDate,UseSituationID,AssetStatus,IsNewAdd,ApplyStatus From FixedAssets order by FixedAssetsID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按FixedAssetsID排序,读取所有固定资产信息
        /// </summary>
        /// <return></return>
        public static DataView Query_V_FixedAssets()
        {

            string strSql = "";
            strSql = "select * From Web_V_FixedAssets order by FixedAssetsID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按FixedAssetsID排序,读取符合条件的固定资产信息
        /// </summary>
        /// <return></return>
        public static DataView Query_V_FixedAssets(string userAccount)
        {

            string strSql = "";
            strSql = "select * From Web_V_FixedAssets where UserAccount='"+ userAccount +"' order by FixedAssetsID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按FixedAssetsID排序,读取所有固定资产信息applyStatus=?
        /// </summary>
        /// <return></return>
        public static DataView QueryFixedAssets(int applyStatus)
        {

            string strSql = "";
            strSql = "select FixedAssetsID,AssetsCoding,AssetName,SpecificationsModel,Manufacturer,UserAccount,Contactor,PurchaseDate,UseSituationID,AssetStatus,IsNewAdd,ApplyStatus From FixedAssets where ApplyStatus="+ applyStatus +" order by FixedAssetsID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按FixedAssetsID排序,读取所有固定资产信息
        /// </summary>
        /// <return></return>
        public static DataView QueryFixedAssets1(string strSql)
        {
            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加新的固定资产
        /// </summary>
        /// <return></return>
        public void Add(Hashtable fixedAssets)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("FixedAssets", fixedAssets);
        }

        /// <summary>
        /// 修改固定资产数据
        /// </summary>
        /// <param name="FixedAssetsID"></param>
        public void Update(Hashtable fixedAssets)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where FixedAssetsID = " + this._fixedAssetsID;
            db.Update("FixedAssets", fixedAssets, strCond);
        }

        /// <summary>
        /// 修改固定资产信息
        /// </summary>
        /// <param name="fixedAssetsInfo">修改固定资产信息</param>
        public static void Update(Hashtable fixedAssets, string where)
        {
            Database db = new Database();			//实例化一个Database类
            db.Update("FixedAssets", fixedAssets, where);	//利用Database类的Update方法修改数据
        }

        /// <summary>
        /// 删除固定资产数据
        /// </summary>
        /// <param name="FixedAssetsID"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from FixedAssets where FixedAssetsID = " + this._fixedAssetsID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        #endregion 方法
    }
}