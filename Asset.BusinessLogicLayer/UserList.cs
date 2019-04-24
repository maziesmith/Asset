using System;
using System.Collections;
using System.Data;

using Asset.DataAccessLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;

namespace Asset.BusinessLogicLayer
{
    /// <summary>
    /// 使用者列表类UserList
    /// </summary>
    public class UserList
    {
        #region 私有成员

        private int _userListID;		    //使用者ID
        private string _userAccount;	    //使用者帐号
        private string _userPassword;	    //使用者密码
        private int _divisionID;            //事业部ID  DivisionID
        private int _departmentID;          //部门ID    DepartmentID
        private string _departmentName;	    //部门名称
        private string _contactor;	        //联系人
        private string _jobPosition;        //职称
        private string _tel;	            //联系电话
        private string _email;	            //E-mail
        private string _address;	        //联系地址 
        private int _userLevel;		        //使用者级别
        private int _isMaintenance;		        //是否网络维护人员IsMaintenance
        private ArrayList _duties = new ArrayList();	//用户所有的权限


        private bool _exist;		//是否存在标志

        #endregion 私有成员

        #region 属性

        public int UserListID
        {
            set
            {
                this._userListID = value;
            }
            get
            {
                return this._userListID;
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
        public string UserPassword
        {
            set
            {
                this._userPassword = value;
            }
            get
            {
                return this._userPassword;
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

        public string JobPosition
        {
            set
            {
                this._jobPosition = value;
            }
            get
            {
                return this._jobPosition;
            }
        }
        public string Tel
        {
            set
            {
                this._tel = value;
            }
            get
            {
                return this._tel;
            }
        }
        public string Email
        {
            set
            {
                this._email = value;
            }
            get
            {
                return this._email;
            }
        }
        public string Address
        {
            set
            {
                this._address = value;
            }
            get
            {
                return this._address;
            }
        }
        public int UserLevel
        {
            set
            {
                this._userLevel = value;
            }
            get
            {
                return this._userLevel;
            }
        }
        public int IsMaintenance
        {
            set
            {
                this._isMaintenance = value;
            }
            get
            {
                return this._isMaintenance;
            }
        }
        public ArrayList Duties
        {
            set
            {
                this._duties = value;
            }
            get
            {
                return this._duties;
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
        /// 根据参数userAccount，获取使用者详细信息IsAllowed=0
        /// </summary>
        /// <param name="userAccount">使用者用户名</param>
        public void LoadData(string userAccount)
        {
            Database db = new Database();		//实例化一个Database类
            string sql = "";
            sql = "Select * From UserLists where IsAllowed=0 and UserAccount = "
                + SqlStringConstructor.GetQuotedString(userAccount);
            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据
            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._userListID = GetSafeData.ValidateDataRow_N(dr, "UserListID");
                this._userAccount = GetSafeData.ValidateDataRow_S(dr, "UserAccount");
                this._userPassword = GetSafeData.ValidateDataRow_S(dr, "UserPassword");
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._departmentID = GetSafeData.ValidateDataRow_N(dr, "DepartmentID");
                this._departmentName = GetSafeData.ValidateDataRow_S(dr, "DepartmentName");
                this._contactor = GetSafeData.ValidateDataRow_S(dr, "Contactor");
                this._jobPosition = GetSafeData.ValidateDataRow_S(dr, "JobPosition");
                this._tel = GetSafeData.ValidateDataRow_S(dr, "Tel");
                this._email = GetSafeData.ValidateDataRow_S(dr, "Email");
                this._address = GetSafeData.ValidateDataRow_S(dr, "Address");
                this._userLevel = GetSafeData.ValidateDataRow_N(dr, "UserLevel");
                this._isMaintenance = GetSafeData.ValidateDataRow_N(dr, "IsMaintenance");
                //获取权限集合
                string colName = "";
                for (int i = 0; i < dr.ItemArray.Length; i++)
                {
                    colName = dr.Table.Columns[i].ColumnName;
                    if (colName.StartsWith("HasDuty_") && GetSafeData.ValidateDataRow_N(dr, colName) == 1)
                    {
                        this._duties.Add(dr.Table.Columns[i].ColumnName.Substring(8));	//去掉前缀“HasDuty_”
                    }
                }
                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据参数userListID，获取使用者详细信息IsAllowed=0
        /// </summary>
        /// <param name="userListID">使用者ID</param>
        public void LoadData(int userListID)
        {
            Database db = new Database();		//实例化一个Database类

            string sql = "";
            sql = "Select * From UserLists where IsAllowed=0 and UserListID = "
                + Convert.ToInt32(userListID);

            DataRow dr = db.GetDataRow(sql);	//利用Database类的GetDataRow方法查询用户数据

            //根据查询得到的数据，对成员赋值
            if (dr != null)
            {
                this._userListID = GetSafeData.ValidateDataRow_N(dr, "UserListID");
                this._userAccount = GetSafeData.ValidateDataRow_S(dr, "UserAccount");
                this._userPassword = GetSafeData.ValidateDataRow_S(dr, "UserPassword");
                this._divisionID = GetSafeData.ValidateDataRow_N(dr, "DivisionID");
                this._departmentID = GetSafeData.ValidateDataRow_N(dr, "DepartmentID");
                this._departmentName = GetSafeData.ValidateDataRow_S(dr, "DepartmentName");
                this._contactor = GetSafeData.ValidateDataRow_S(dr, "Contactor");
                this._jobPosition = GetSafeData.ValidateDataRow_S(dr, "JobPosition");
                this._tel = GetSafeData.ValidateDataRow_S(dr, "Tel");
                this._email = GetSafeData.ValidateDataRow_S(dr, "Email");
                this._address = GetSafeData.ValidateDataRow_S(dr, "Address");
                this._userLevel = GetSafeData.ValidateDataRow_N(dr, "UserLevel");
                this._isMaintenance = GetSafeData.ValidateDataRow_N(dr, "IsMaintenance");

                //获取权限集合
                string colName = "";
                for (int i = 0; i < dr.ItemArray.Length; i++)
                {
                    colName = dr.Table.Columns[i].ColumnName;
                    if (colName.StartsWith("HasDuty_") && GetSafeData.ValidateDataRow_N(dr, colName) == 1)
                    {
                        this._duties.Add(dr.Table.Columns[i].ColumnName.Substring(8));	//去掉前缀“HasDuty_”
                    }
                }

                this._exist = true;
            }
            else
            {
                this._exist = false;
            }
        }

        /// <summary>
        /// 根据查询条件哈希表,查询数据
        /// </summary>
        /// <param name="queryItems">查询条件哈希表</param>
        /// <returns>查询结果数据DataTable</returns>
        public static DataTable Query(Hashtable queryItems)
        {
            string where = SqlStringConstructor.GetConditionClause(queryItems);
            string sql = "Select * From UserLists" + where;
            Database db = new Database();
            return db.GetDataTable(sql);
        }


        /// <summary>
        /// 按UserListID排序,读取所有用户
        /// </summary>
        /// <return></return>
        public static DataView QueryUserLists(string strSql)
        {
            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按UserListID排序,读取所有用户
        /// </summary>
        /// <return></return>
        public static DataView QueryAllUserLists()
        {

            string strSql = "";
            strSql = "Select a.UserListID,a.UserAccount,a.UserPassword,a.Contactor,a.DivisionID,a.DepartmentID,a.JobPosition,a.Tel,a.Email,a.Address,a.UserLevel,a.IsAllowed,a.IsMaintenance,b.DepartmentName,c.DivisionName From UserLists a left outer join Departments b on a.DepartmentID=b.DepartmentID left outer join Divisions c on b.DivisionID=c.DivisionID order by a.UserListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按UserListID排序,读取所有用户UserLevel=2
        /// </summary>
        /// <return></return>
        public static DataView QueryUserLists()
        {

            string strSql = "";
            strSql = "Select UserListID,UserAccount,UserPassword,DivisionID,DepartmentID,DepartmentName,Contactor,JobPosition,Tel,Email,Address,UserLevel,IsMaintenance From UserLists where UserLevel=2 order by UserListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按UserListID排序,读取某个部门DepartmentID的所有用户
        /// </summary>
        /// <return></return>
        public static DataView QueryUserLists(int departmentID)
        {

            string strSql = "";
            strSql = "Select UserListID,UserAccount,UserPassword,DivisionID,DepartmentID,Contactor From UserLists where DepartmentID=" + departmentID + " order by Contactor";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 按UserListID排序,读取所有网络维护用户IsMaintenance=1
        /// </summary>
        /// <return></return>
        public static DataView QueryUserLists1()
        {

            string strSql = "";
            strSql = "Select UserListID,UserAccount,UserPassword,DivisionID,DepartmentID,DepartmentName,Contactor,JobPosition,Tel,Email,Address,UserLevel,IsMaintenance From UserLists where IsMaintenance=1 order by UserListID desc";

            //绑定数据
            Database db = new Database();
            return db.GetDataView(strSql);
        }

        /// <summary>
        /// 增加新的用户
        /// </summary>
        /// <return></return>
        public void Add(Hashtable userLists)
        {
            Database db = new Database();           //实例化一个Database类
            db.Insert("UserLists", userLists);
        }

        /// <summary>
        /// 修改用户数据
        /// </summary>
        /// <param name="userListID"></param>
        public void Update(Hashtable userLists)
        {
            Database db = new Database();           //实例化一个Database类
            string strCond = "where UserListID = " + this._userListID;
            db.Update("UserLists", userLists, strCond);
        }

        /// <summary>
        /// 修改用户权限信息
        /// </summary>
        /// <param name="roleInfo">用户权限信息哈希表</param>
        public static void Update(Hashtable userLists, string where)
        {
            Database db = new Database();			//实例化一个Database类
            db.Update("UserLists", userLists, where);	//利用Database类的Update方法修改数据
        }

        /// <summary>
        /// 删除用户数据
        /// </summary>
        /// <param name="userListID"></param>
        public void Delete()
        {
            string sql = "";
            sql = "Delete from UserLists where UserListID = " + this._userListID;

            Database db = new Database();
            db.ExecuteSQL(sql);
        }

        #endregion 方法
    }
}
