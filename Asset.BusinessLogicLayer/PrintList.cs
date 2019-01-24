using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 打印列表类PrintList
    /// </summary>
    public class PrintList
    {
        #region 私有成员

        private int _printListID;		    //打印列表ID
        private int _pFixedAssetsID;        //固定资产ID
        private string _pUserAccount;	    //工号
        private int _printStatus;           //状态
        private DateTime _addTime;          //增加时间

        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int PrintListID
        {
            set
            {
                this._printListID = value;
            }
            get
            {
                return this._printListID;
            }
        }
        public int PFixedAssetsID
        {
            set
            {
                this._pFixedAssetsID = value;
            }
            get
            {
                return this._pFixedAssetsID;
            }
        }
        public string PUserAccount
        {
            set
            {
                this._pUserAccount = value;
            }
            get
            {
                return this._pUserAccount;
            }
        }
        public int PrintStatus
        {
            set
            {
                this._printStatus = value;
            }
            get
            {
                return this._printStatus;
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
        /// 根据参数PrintListID + userAccount，获取详细信息
        /// </summary>
        /// <param name="PrintListID">PrintListID</param>
        public void LoadData(int fixedAssetsID,string userAccount)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select PrintListID,PFixedAssetsID,PUserAccount,PrintStatus,AddTime From PrintLists where PFixedAssetsID = "
                + Convert.ToInt32(fixedAssetsID) + " and PUserAccount=" + SqlStringConstructor.GetQuotedString(userAccount);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._printListID = GetSafeData.ValidateDataRow_N(dr, "PrintListID");
                this._pFixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "PFixedAssetsID");
                this._pUserAccount = GetSafeData.ValidateDataRow_S(dr, "PUserAccount");
                this._printStatus = GetSafeData.ValidateDataRow_N(dr, "PrintStatus");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数PrintListID，获取详细信息
        /// </summary>
        /// <param name="PrintListID">PrintListID</param>
        public void LoadData1(int printListID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select PrintListID,PFixedAssetsID,PUserAccount,PrintStatus,AddTime From PrintLists where PrintListID = "
                + Convert.ToInt32(printListID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._printListID = GetSafeData.ValidateDataRow_N(dr, "PrintListID");
                this._pFixedAssetsID = GetSafeData.ValidateDataRow_N(dr, "PFixedAssetsID");
                this._pUserAccount = GetSafeData.ValidateDataRow_S(dr, "PUserAccount");
                this._printStatus = GetSafeData.ValidateDataRow_N(dr, "PrintStatus");
                this._addTime = GetSafeData.ValidateDataRow_T(dr, "AddTime");

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
        public static DataView Query_V_PrintList(string userAccount)
        {

            string strSql = "";
            strSql = "select *,convert(varchar(10),PurchaseDate,120) as Show_PurchaseDate From Web_V_PrintLists where PUserAccount='" + userAccount + "' order by PrintListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }


        /// <summary>
        /// 增加打印列表信息
        /// </summary>
        /// <return></return>
        public void Add(Hashtable printLists)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("PrintLists", printLists);
        }

        /// <summary>
        /// 修改打印列表信息
        /// </summary>
        /// <param name="PrintLists"></param>
        public void Update(Hashtable printLists)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where PrintListID = " + this._printListID;
            db.Update("PrintLists", printLists, strCond);
        }


        /// <summary>
        /// 删除打印列表信息
        /// </summary>
        /// <param name="PrintLists"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from PrintLists where PrintListID = " + this._printListID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        /// <summary>
        /// 删除打印列表信息
        /// </summary>
        /// <param name="PrintLists"></param>
        public void Delete(string userAccount)
        {
            string sql = "";
            sql = "Delete from PrintLists where PUserAccount = " + SqlStringConstructor.GetQuotedString(userAccount);

            Database db = new Database();
            db.ExecuteSQL(sql);
        }
        #endregion 方法
    }
}

