using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 增加方式类AddWays
    /// </summary>
    public class AddWays
    {
        #region 方法

        /// <summary>
        /// 读取所有的AddWays
        /// </summary>
        /// <return></return>
        public static DataView QueryAddWays()
        {

            string strSql = "";
            strSql = "select AddWaysID,AddWaysSortID,AddWaysName,AddTime From AddWays order by AddWaysSortID,AddWaysID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }
        #endregion 方法
    }
}
