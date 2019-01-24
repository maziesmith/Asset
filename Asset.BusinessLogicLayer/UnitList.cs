using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 单位类UnitList
    /// </summary>
    public class UnitList
    {
        #region 方法

        /// <summary>
        /// 读取所有的Unit
        /// </summary>
        /// <return></return>
        public static DataView QueryUnits()
        {

            string strSql = "";
            strSql = "select UnitsID,UnitsSortID,UnitsName,AddTime From Units order by UnitsSortID,UnitsID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }
        #endregion 方法
    }
}
