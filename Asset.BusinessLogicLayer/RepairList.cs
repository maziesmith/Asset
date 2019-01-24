using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 报修列表类RepairList
    /// </summary>
    public class RepairList
    {
        #region 私有成员

        private int _repairListID;		    //报修ID
        private int _fixedAssetsID;		    //固定资产ID
        private string _assetsCoding;	    //资产编码
        private string _assetName;	        //资产名称
        private string _specificationsModel;	    //规格型号
        private string _repairUserAccount;	//报修人帐号
        private string _repairContactor;	//报修人名字
        private string _repairDepartmentName;   //报修部门名称
        private string _repairTel;	        //报修电话
        private string _repairAddress;	    //报修地址
        private string _repairContent;	    //报修内容
        private string _repairIP;	        //报修IP
        private DateTime _repairTime;       //报修时间
        private string _dealUserAccount;	//处理人帐号
        private string _dealContactor;	    //处理人名字
        private DateTime _dealTime;         //处理时间
        private int _dealState;		        //处理状态 0未处理，1维修中，2完成
        private string _dealContent;	    //处理意见

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int RepairListID
        {
            set
            {
                this._repairListID = value;
            }
            get
            {
                return this._repairListID;
            }
        }
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
        public string RepairUserAccount
        {
            set
            {
                this._repairUserAccount = value;
            }
            get
            {
                return this._repairUserAccount;
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
        public string RepairContactor
        {
            set
            {
                this._repairContactor = value;
            }
            get
            {
                return this._repairContactor;
            }
        }
        public string RepairDepartmentName
        {
            set
            {
                this._repairDepartmentName = value;
            }
            get
            {
                return this._repairDepartmentName;
            }
        }
        public string RepairTel
        {
            set
            {
                this._repairTel = value;
            }
            get
            {
                return this._repairTel;
            }
        }
        public string RepairAddress
        {
            set
            {
                this._repairAddress = value;
            }
            get
            {
                return this._repairAddress;
            }
        }
        public string RepairContent
        {
            set
            {
                this._repairContent = value;
            }
            get
            {
                return this._repairContent;
            }
        }
        public string RepairIP
        {
            set
            {
                this._repairIP = value;
            }
            get
            {
                return this._repairIP;
            }
        }
        public DateTime RepairTime
        {
            set
            {
                this._repairTime = value;
            }
            get
            {
                return this._repairTime;
            }
        }
        public string DealUserAccount
        {
            set
            {
                this._dealUserAccount = value;
            }
            get
            {
                return this._dealUserAccount;
            }
        }
        public string DealContactor
        {
            set
            {
                this._dealContactor = value;
            }
            get
            {
                return this._dealContactor;
            }
        }
        public DateTime DealTime
        {
            set
            {
                this._dealTime = value;
            }
            get
            {
                return this._dealTime;
            }
        }
        public int DealState
        {
            set
            {
                this._dealState = value;
            }
            get
            {
                return this._dealState;
            }
        }
        public string DealContent
        {
            set
            {
                this._dealContent = value;
            }
            get
            {
                return this._dealContent;
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
        /// 根据参数repairListID，获取详细信息
        /// </summary>
        /// <param name="repairListID">报修ID</param>
        public void LoadData(int repairListID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select a.RepairListID,a.FixedAssetsID,a.AssetsCoding,a.AssetName,a.SpecificationsModel,a.RepairUserAccount,a.RepairContactor,a.RepairDepartmentName,a.RepairTel,a.RepairAddress,a.RepairContent,a.RepairIP,a.RepairTime,a.DealUserAccount,a.DealContactor,a.DealTime,a.DealState,a.DealContent,b.DepartmentName From RepairLists a left outer join Departments b on a.RepairDepartmentID=b.DepartmentID where a.RepairListID = "
                + Convert.ToInt32(repairListID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._repairListID = GetSafeData.ValidateDataRow_N(dr, "RepairListID");
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._assetsCoding = GetSafeData.ValidateDataRow_S(dr, "AssetsCoding");
                this._assetName = GetSafeData.ValidateDataRow_S(dr, "AssetName");
                this._specificationsModel = GetSafeData.ValidateDataRow_S(dr, "SpecificationsModel");
                this._repairUserAccount = GetSafeData.ValidateDataRow_S(dr, "RepairUserAccount");
                this._repairContactor = GetSafeData.ValidateDataRow_S(dr, "RepairContactor");
                this._repairDepartmentName = GetSafeData.ValidateDataRow_S(dr, "DepartmentName");
                this._repairTel = GetSafeData.ValidateDataRow_S(dr, "RepairTel");
                this._repairAddress = GetSafeData.ValidateDataRow_S(dr, "RepairAddress");
                this._repairContent = GetSafeData.ValidateDataRow_S(dr, "RepairContent");
                this._repairIP = GetSafeData.ValidateDataRow_S(dr, "RepairIP");
                this._repairTime = GetSafeData.ValidateDataRow_T(dr, "RepairTime");
                this._dealUserAccount = GetSafeData.ValidateDataRow_S(dr, "DealUserAccount");
                this._dealContactor = GetSafeData.ValidateDataRow_S(dr, "DealContactor");
                this._dealTime = GetSafeData.ValidateDataRow_T(dr, "DealTime");
                this._dealState = GetSafeData.ValidateDataRow_N(dr, "DealState");
                this._dealContent = GetSafeData.ValidateDataRow_S(dr, "DealContent");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数fixedAssetsID，获取详细信息
        /// </summary>
        /// <param name="fixedAssetsID">报修ID</param>
        public void LoadData1(int fixedAssetsID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select top 1 RepairListID,FixedAssetsID,RepairUserAccount,RepairContactor,RepairContent,RepairIP,RepairTime,DealUserAccount,DealContactor,DealTime,DealState,DealContent From RepairLists where DealState=0 and FixedAssetsID =" + Convert.ToInt32(fixedAssetsID) + " order by RepairListID desc";

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._repairListID = GetSafeData.ValidateDataRow_N(dr, "RepairListID");
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._repairUserAccount = GetSafeData.ValidateDataRow_S(dr, "RepairUserAccount");
                this._repairContactor = GetSafeData.ValidateDataRow_S(dr, "RepairContactor");
                this._repairContent = GetSafeData.ValidateDataRow_S(dr, "RepairContent");
                this._repairIP = GetSafeData.ValidateDataRow_S(dr, "RepairIP");
                this._repairTime = GetSafeData.ValidateDataRow_T(dr, "RepairTime");
                this._dealUserAccount = GetSafeData.ValidateDataRow_S(dr, "DealUserAccount");
                this._dealContactor = GetSafeData.ValidateDataRow_S(dr, "DealContactor");
                this._dealTime = GetSafeData.ValidateDataRow_T(dr, "DealTime");
                this._dealState = GetSafeData.ValidateDataRow_N(dr, "DealState");
                this._dealContent = GetSafeData.ValidateDataRow_S(dr, "DealContent");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 按RepairTime降序,读取所有报修单
        /// </summary>
        /// <return></return>
        public static DataView QueryRepairLists()
        {

            string strSql = "";
            strSql = "Select a.RepairListID,a.FixedAssetsID,a.AssetsCoding,a.AssetName,a.SpecificationsModel,a.RepairUserAccount,a.RepairContactor,a.RepairDepartmentName,a.RepairTel,a.RepairAddress,a.RepairContent,a.RepairIP,a.RepairTime,a.DealUserAccount,a.DealContactor,a.DealTime,a.DealState,a.DealContent,b.DepartmentName From RepairLists a left outer join Departments b on a.RepairDepartmentID=b.DepartmentID order by a.RepairListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按RepairTime降序,读取符合条件的报修单
        /// </summary>
        /// <return></return>
        public static DataView QueryRepairLists(string strSql)
        {
            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }


        /// <summary>
        /// 读取符合条件的报修单
        /// </summary>
        /// <return></return>
        public static DataView QueryRepairLists(int dealState, string userAccount)
        {

            string strSql = "";
            strSql = "Select a.RepairListID,a.FixedAssetsID,a.AssetsCoding,a.AssetName,a.SpecificationsModel,a.RepairUserAccount,a.RepairContactor,a.RepairDepartmentName,a.RepairTel,a.RepairAddress,a.RepairContent,a.RepairIP,a.RepairTime,a.DealUserAccount,a.DealContactor,a.DealTime,a.DealState,a.DealContent,b.DepartmentName From RepairLists a left outer join Departments b on a.RepairDepartmentID=b.DepartmentID where a.DealState = "
                + Convert.ToInt32(dealState) + " and a.RepairUserAccount = " + SqlStringConstructor.GetQuotedString(userAccount) + " order by a.RepairListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 读取符合条件的报修单
        /// </summary>
        /// <return></return>
        public static DataView QueryRepairLists(int dealState)
        {

            string strSql = "";
            strSql = "Select a.RepairListID,a.FixedAssetsID,a.AssetsCoding,a.AssetName,a.SpecificationsModel,a.RepairUserAccount,a.RepairContactor,a.RepairDepartmentName,a.RepairTel,a.RepairAddress,a.RepairContent,a.RepairIP,a.RepairTime,a.DealUserAccount,a.DealContactor,a.DealTime,a.DealState,a.DealContent,b.DepartmentName From RepairLists a left outer join Departments b on a.RepairDepartmentID=b.DepartmentID where a.DealState = "
                + Convert.ToInt32(dealState) + " order by a.RepairListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 读取符合条件的报修单
        /// </summary>
        /// <return></return>
        public static DataView QueryRepairHistoryRecord(int fixedAssetsID)
        {

            string strSql = "";
            strSql = "Select a.RepairListID,a.FixedAssetsID,a.AssetsCoding,a.AssetName,a.SpecificationsModel,a.RepairUserAccount,a.RepairContactor,a.RepairDepartmentName,a.RepairTel,a.RepairAddress,a.RepairContent,a.RepairIP,a.RepairTime,a.DealUserAccount,a.DealContactor,a.DealTime,a.DealState,a.DealContent,b.DepartmentName From RepairLists a left outer join Departments b on a.RepairDepartmentID=b.DepartmentID where a.DealState=2 and a.FixedAssetsID = "
                + Convert.ToInt32(fixedAssetsID) + " order by a.RepairListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加新的报修单
        /// </summary>
        /// <return></return>
        public void Add(Hashtable repairLists)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("RepairLists", repairLists);
        }

        /// <summary>
        /// 修改报修单
        /// </summary>
        /// <param name="repairListID"></param>
        public void Update(Hashtable repairLists)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where RepairListID = " + this._repairListID;
            db.Update("RepairLists", repairLists, strCond);
        }

        /// <summary>
        /// 删除报修单
        /// </summary>
        /// <param name="RepairListID"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from RepairLists where RepairListID = " + this._repairListID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        #endregion 方法
    }
}