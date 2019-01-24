using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 固定资产报废类AssetsScrapped
    /// </summary>
    public class AssetsScrapped
    {
        #region 私有成员

        private int _assetsScrappedID;		//固定资产报废ID
        private string _assetsScrappedNum;	//固定资产报废单号
        private int _fixedAssetsID;         //固定资产ID
        private string _applyUserAccount;	//申请人工号
        private string _applicant;	        //申请人
        private string _approvedUserAccount;	//批准人工号
        private string _approvedPerson;	    //批准人
        private DateTime _reduceDate;	    //减少日期
        private int _reduceWaysID;          //减少方式ID
        private string _reduceWays;	        //减少方式
        private string _scrappedReason;	    //报废理由
        private int _scrappedStatus;		//报废状态 0已完成，1申请中
        private DateTime _addTime;          //增加日期

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int AssetsScrappedID
        {
            set
            {
                this._assetsScrappedID = value;
            }
            get
            {
                return this._assetsScrappedID;
            }
        }
        public string AssetsScrappedNum
        {
            set
            {
                this._assetsScrappedNum = value;
            }
            get
            {
                return this._assetsScrappedNum;
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
        public string Applicant
        {
            set
            {
                this._applicant = value;
            }
            get
            {
                return this._applicant;
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
        public DateTime ReduceDate
        {
            set
            {
                this._reduceDate = value;
            }
            get
            {
                return this._reduceDate;
            }
        }
        public int ReduceWaysID
        {
            set
            {
                this._reduceWaysID = value;
            }
            get
            {
                return this._reduceWaysID;
            }
        }
        public string ReduceWays
        {
            set
            {
                this._reduceWays = value;
            }
            get
            {
                return this._reduceWays;
            }
        }
        public string ScrappedReason
        {
            set
            {
                this._scrappedReason = value;
            }
            get
            {
                return this._scrappedReason;
            }
        }
        public int ScrappedStatus
        {
            set
            {
                this._scrappedStatus = value;
            }
            get
            {
                return this._scrappedStatus;
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
        /// 根据参数assetsScrappedID，获取固定资产报废详细信息
        /// </summary>
        /// <param name="assetsScrappedID">资产报废ID</param>
        public void LoadData(int assetsScrappedID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select AssetsScrappedID,AssetsScrappedNum,FixedAssetsID,ApplyUserAccount,Applicant,ApprovedUserAccount,ApprovedPerson,ReduceDate,ReduceWaysID,ReduceWays,ScrappedReason,ScrappedStatus,AddTime From AssetsScrapped where AssetsScrappedID = "
                + Convert.ToInt32(assetsScrappedID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值,
            if (dr != null)
            {
                this._assetsScrappedID = GetSafeData.ValidateDataRow_N(dr, "AssetsScrappedID");
                this._assetsScrappedNum = GetSafeData.ValidateDataRow_S(dr, "AssetsScrappedNum");
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._applyUserAccount = GetSafeData.ValidateDataRow_S(dr, "ApplyUserAccount");
                this._applicant = GetSafeData.ValidateDataRow_S(dr, "Applicant");
                this._approvedUserAccount = GetSafeData.ValidateDataRow_S(dr, "ApprovedUserAccount");
                this._approvedPerson = GetSafeData.ValidateDataRow_S(dr, "ApprovedPerson");
                this._reduceDate = GetSafeData.ValidateDataRow_T(dr, "ReduceDate");
                this._reduceWaysID = GetSafeData.ValidateDataRow_N(dr, "ReduceWaysID");
                this._reduceWays = GetSafeData.ValidateDataRow_S(dr, "ReduceWays");
                this._scrappedReason = GetSafeData.ValidateDataRow_S(dr, "ScrappedReason");
                this._scrappedStatus = GetSafeData.ValidateDataRow_N(dr, "ScrappedStatus");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数FixedAssetsID，获取固定资产报废详细信息
        /// </summary>
        /// <param name="fixedAssetsID">资产报废ID</param>
        public void LoadData1(int fixedAssetsID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select top 1 AssetsScrappedID,AssetsScrappedNum,FixedAssetsID,ApplyUserAccount,Applicant,ApprovedUserAccount,ApprovedPerson,ReduceDate,ReduceWaysID,ReduceWays,ScrappedReason,ScrappedStatus,AddTime From AssetsScrapped where ScrappedStatus=1 and FixedAssetsID=" + Convert.ToInt32(fixedAssetsID) + " order by AssetsScrappedID desc";

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值,
            if (dr != null)
            {
                this._assetsScrappedID = GetSafeData.ValidateDataRow_N(dr, "AssetsScrappedID");
                this._assetsScrappedNum = GetSafeData.ValidateDataRow_S(dr, "AssetsScrappedNum");
                this._fixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "FixedAssetsID");
                this._applyUserAccount = GetSafeData.ValidateDataRow_S(dr, "ApplyUserAccount");
                this._applicant = GetSafeData.ValidateDataRow_S(dr, "Applicant");
                this._approvedUserAccount = GetSafeData.ValidateDataRow_S(dr, "ApprovedUserAccount");
                this._approvedPerson = GetSafeData.ValidateDataRow_S(dr, "ApprovedPerson");
                this._reduceDate = GetSafeData.ValidateDataRow_T(dr, "ReduceDate");
                this._reduceWaysID = GetSafeData.ValidateDataRow_N(dr, "ReduceWaysID");
                this._reduceWays = GetSafeData.ValidateDataRow_S(dr, "ReduceWays");
                this._scrappedReason = GetSafeData.ValidateDataRow_S(dr, "ScrappedReason");
                this._scrappedStatus = GetSafeData.ValidateDataRow_N(dr, "ScrappedStatus");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }



        /// <summary>
        /// 按AssetsScrappedID排序,读取所有固定资产报废信息
        /// </summary>
        /// <return></return>
        public static DataView QueryAssetsScrapped()
        {

            string strSql = "";
            strSql = "Select AssetsScrappedID,AssetsScrappedNum,FixedAssetsID,ApplyUserAccount,Applicant,ApprovedUserAccount,ApprovedPerson,ReduceDate,ReduceWaysID,ReduceWays,ScrappedReason,ScrappedStatus,AddTime From AssetsScrapped order by AssetsScrappedID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按AssetsScrappedID排序,读取所有固定资产报废信息
        /// </summary>
        /// <return></return>
        public static DataView QueryAssetsScrapped(int fixedAssetsID)
        {

            string strSql = "";
            strSql = "Select AssetsScrappedID,AssetsScrappedNum,FixedAssetsID,ApplyUserAccount,Applicant,ApprovedUserAccount,ApprovedPerson,ReduceDate,ReduceWaysID,ReduceWays,ScrappedReason,ScrappedStatus,AddTime From AssetsScrapped where ScrappedStatus=0 and FixedAssetsID=" + fixedAssetsID + " order by AssetsScrappedID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加新的固定资产报废信息
        /// </summary>
        /// <return></return>
        public void Add(Hashtable assetsScrapped)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("AssetsScrapped", assetsScrapped);
        }

        /// <summary>
        /// 修改固定资产报废信息
        /// </summary>
        /// <param name="assetsScrappedID"></param>
        public void Update(Hashtable assetsScrapped)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where AssetsScrappedID = " + this._assetsScrappedID;
            db.Update("AssetsScrapped", assetsScrapped, strCond);
        }

        /// <summary>
        /// 删除固定资产报废信息
        /// </summary>
        /// <param name="assetsScrappedID"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from AssetsScrapped where AssetsScrappedID = " + this._assetsScrappedID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        #endregion 方法
    }
}