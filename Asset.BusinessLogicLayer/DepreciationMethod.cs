using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 折旧方法类DepreciationMethod
    /// </summary>
    public class DepreciationMethod
    {
        #region 方法

        /// <summary>
        /// 读取所有的Unit
        /// </summary>
        /// <return></return>
        public static DataView QueryDepreciationMethod()
        {

            string strSql = "";
            strSql = "select MethodID,MethodSortID,MethodName,AddTime From DepreciationMethod order by MethodSortID,MethodID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }
        #endregion 方法
    }
}
