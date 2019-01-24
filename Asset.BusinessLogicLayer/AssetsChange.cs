using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 固定资产异动类AssetsChange
    /// </summary>
    public class AssetsChange
    {
        #region 私有成员

        private int _assetsChangesID;		//固定资产异动ID
        private string _assetsChangesNum;	//异动单号
        private int _fixedAssetsID;         //固定资产ID
        private int _cDivisionID;           //事业部ID
        private string _cDivisionName;	    //事业部名称
        private int _cDepartmentID;         //部门ID
        private string _cDepartmentName;	//部门名称
        private string _cUserAccount;	    //保管人员工号
        private string _cContactor;         //保管联系人
        private string _cStorageSites;	    //存放地点
        private DateTime _cChangesDate;	    //异动日期
        private string _transferUserAccount;	//转移人工号
        private string _transferPeople;	        //转移人
        private string _approvedUserAccount;	//批准人工号
        private string _approvedPerson;	        //批准人
        private string _cBackup;	            //备注信息
        private int _changesStatus;		        //异动状态 0已完成，1申请中
        private DateTime _addTime;              //增加日期

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int AssetsChangesID
        {
            set
            {
                this._assetsChangesID = value;
            }
            get
            {
                return this._assetsChangesID;
            }
        }
        public string AssetsChangesNum
        {
            set
            {
                this._assetsChangesNum = value;
            }
            get
            {
                return this._assetsChangesNum;
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
        public int CDivisionID
        {
            set
            {
                this._cDivisionID = value;
            }
            get
            {
                return this._cDivisionID;
            }
        }
        public string CDivisionName
        {
            set
            {
                this._cDivisionName = value;
            }
            get
            {
                return this._cDivisionName;
            }
        }
        public int CDepartmentID
        {
            set
            {
                this._cDepartmentID = value;
            }
            get
            {
                return this._cDepartmentID;
            }
        }
        public string CDepartmentName
        {
            set
            {
                this._cDepartmentName = value;
            }
            get
            {
                return this._cDepartmentName;
            }
        }
        public string CUserAccount
        {
            set
            {
                this._cUserAccount = value;
            }
            get
            {
                return this._cUserAccount;
            }
        }
        public string CContactor
        {
            set
            {
                this._cContactor = value;
            }
            get
            {
                return this._cContactor;
            }
        }
        public string CStorageSites
        {
            set
            {
                this._cStorageSites = value;
            }
            get
            {
                return this._cStorageSites;
            }
        }
        public DateTime CChangesDate
        {
            set
            {
                this._cChangesDate = value;
            }
            get
            {
                return this._cChangesDate;
            }
        }
        public string TransferUserAccount
        {
            set
            {
                this._transferUserAccount = value;
            }
            get
            {
                return this._transferUserAccount;
            }
        }
        public string TransferPeople
        {
            set
            {
                this._transferPeople = value;
            }
            get
            {
                return this._transferPeople;
            }
        }
        public string ApprovedUserAccount
        {
            set
            {
                this._approvedUserAccount = value;
            }
            get
            {
                return this._approvedUserAccount;
            }
        }
        public string ApprovedPerson
        {
            set
            {
                this._approvedPerson = value;
            }
            get
            {
                return this._approvedPerson;
            }
        }
        public string CBackup
        {
            set
            {
                this._cBackup = value;
            }
            get
            {
                return this._cBackup;
            }
        }
        public int ChangesStatus
        {
            set
            {
                this._changesStatus = value;
            }
            get
            {
                return this._changesStatus;
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
        /// 根据参数AssetsChangesID，获取详细信息异动详细信息
        /// </summary>
        /// <param name="assetsChangesID">资产异动ID</param>
        public void LoadData(int assetsChangesID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select AssetsChangesID,AssetsChangesNum,FixedAssetsID,CDivisionID,CDivisionName,CDepartmentID,CDepartmentName,CUserAccount,CContactor,CStorageSites,CChangesDate,TransferUserAccount,TransferPeople,ApprovedUserAccount,ApprovedPerson,CBackup,ChangesStatus,AddTime From AssetsChanges where AssetsChangesID = "
                + Convert.ToInt32(assetsChangesID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._assetsChangesID = GetSafeData.ValidateDataRow_N(dr, "AssetsChangesID");
                this._assetsChangesNum = GetSafeData.ValidateDataRow_S(dr, "AssetsChangesNum");
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._cDivisionID = GetSafeData.ValidateDataRow_N(dr, "CDivisionID");
                this._cDivisionName = GetSafeData.ValidateDataRow_S(dr, "CDivisionName");
                this._cDepartmentID = GetSafeData.ValidateDataRow_N(dr, "CDepartmentID");
                this._cDepartmentName = GetSafeData.ValidateDataRow_S(dr, "CDepartmentName");
                this._cUserAccount = GetSafeData.ValidateDataRow_S(dr, "CUserAccount");
                this._cContactor = GetSafeData.ValidateDataRow_S(dr, "CContactor");
                this._cStorageSites = GetSafeData.ValidateDataRow_S(dr, "CStorageSites");
                this._cChangesDate = GetSafeData.ValidateDataRow_T(dr, "CChangesDate");
                this._transferUserAccount = GetSafeData.ValidateDataRow_S(dr, "TransferUserAccount");
                this._transferPeople = GetSafeData.ValidateDataRow_S(dr, "TransferPeople");
                this._approvedUserAccount = GetSafeData.ValidateDataRow_S(dr, "ApprovedUserAccount");
                this._approvedPerson = GetSafeData.ValidateDataRow_S(dr, "ApprovedPerson");
                this._cBackup = GetSafeData.ValidateDataRow_S(dr, "CBackup");
                this._changesStatus = GetSafeData.ValidateDataRow_N(dr, "ChangesStatus");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数fixedAssetsID，获取详细信息异动详细信息
        /// </summary>
        /// <param name="fixedAssetsID">固定资产ID</param>
        public void LoadData1(int fixedAssetsID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select top 1 AssetsChangesID,AssetsChangesNum,FixedAssetsID,CDivisionID,CDivisionName,CDepartmentID,CDepartmentName,CUserAccount,CContactor,CStorageSites,CChangesDate,TransferUserAccount,TransferPeople,ApprovedUserAccount,ApprovedPerson,CBackup,ChangesStatus,AddTime From AssetsChanges where ChangesStatus=1 and FixedAssetsID =" + Convert.ToInt32(fixedAssetsID) + " order by AssetsChangesID desc";

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._assetsChangesID = GetSafeData.ValidateDataRow_N(dr, "AssetsChangesID");
                this._assetsChangesNum = GetSafeData.ValidateDataRow_S(dr, "AssetsChangesNum");
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._cDivisionID = GetSafeData.ValidateDataRow_N(dr, "CDivisionID");
                this._cDivisionName = GetSafeData.ValidateDataRow_S(dr, "CDivisionName");
                this._cDepartmentID = GetSafeData.ValidateDataRow_N(dr, "CDepartmentID");
                this._cDepartmentName = GetSafeData.ValidateDataRow_S(dr, "CDepartmentName");
                this._cUserAccount = GetSafeData.ValidateDataRow_S(dr, "CUserAccount");
                this._cContactor = GetSafeData.ValidateDataRow_S(dr, "CContactor");
                this._cStorageSites = GetSafeData.ValidateDataRow_S(dr, "CStorageSites");
                this._cChangesDate = GetSafeData.ValidateDataRow_T(dr, "CChangesDate");
                this._transferUserAccount = GetSafeData.ValidateDataRow_S(dr, "TransferUserAccount");
                this._transferPeople = GetSafeData.ValidateDataRow_S(dr, "TransferPeople");
                this._approvedUserAccount = GetSafeData.ValidateDataRow_S(dr, "ApprovedUserAccount");
                this._approvedPerson = GetSafeData.ValidateDataRow_S(dr, "ApprovedPerson");
                this._cBackup = GetSafeData.ValidateDataRow_S(dr, "CBackup");
                this._changesStatus = GetSafeData.ValidateDataRow_N(dr, "ChangesStatus");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }


        /// <summary>
        /// 按AssetsChangesID排序,读取所有固定资产异动信息
        /// </summary>
        /// <return></return>
        public static DataView QueryAssetsChanges()
        {

            string strSql = "";
            strSql = "Select AssetsChangesID,AssetsChangesNum,FixedAssetsID,CDivisionID,CDivisionName,CDepartmentID,CDepartmentName,CUserAccount,CContactor,CStorageSites,CChangesDate,TransferUserAccount,TransferPeople,ApprovedUserAccount,ApprovedPerson,CBackup,ChangesStatus,AddTime From AssetsChanges order by AssetsChangesID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按AssetsChangesID排序,读取所有固定资产异动信息
        /// </summary>
        /// <return></return>
        public static DataView QueryAssetsChanges(int fixedAssetsID)
        {
            string strSql = "";
            strSql = "Select AssetsChangesID,AssetsChangesNum,FixedAssetsID,CDivisionID,CDivisionName,CDepartmentID,CDepartmentName,CUserAccount,CContactor,CStorageSites,CChangesDate,TransferUserAccount,TransferPeople,ApprovedUserAccount,ApprovedPerson,CBackup,ChangesStatus,AddTime From AssetsChanges where ChangesStatus=0 and FixedAssetsID =" + Convert.ToInt32(fixedAssetsID) + " order by AssetsChangesID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加新的固定资产异动信息
        /// </summary>
        /// <return></return>
        public void Add(Hashtable assetsChanges)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("AssetsChanges", assetsChanges);
        }

        /// <summary>
        /// 修改固定资产异动信息
        /// </summary>
        /// <param name="assetsChangesID"></param>
        public void Update(Hashtable assetsChanges)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where AssetsChangesID = " + this._assetsChangesID;
            db.Update("AssetsChanges", assetsChanges, strCond);
        }

        /// <summary>
        /// 删除固定资产异动信息
        /// </summary>
        /// <param name="assetsChangesID"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from AssetsChanges where AssetsChangesID = " + this._assetsChangesID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        #endregion 方法
    }
}