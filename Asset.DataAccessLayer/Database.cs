using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Asset.DataAccessLayer
{
    /// <summary>
	/// 数据操作层，用于数据访问的类。
	/// </summary>
    public class Database : IDisposable
    {
        /// <summary>
        /// 保护变量，数据库连接。
        /// </summary>
        protected SqlConnection Connection;

        /// <summary>
        /// 保护变量，数据库连接串。
        /// </summary>
        protected String ConnectionString;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="DatabaseConnectionString">数据库连接串</param>
        public Database()
        {
            ConnectionString = ConfigurationManager.AppSettings["DBConnectionString"];
        }

        /// <summary>
        /// 析构函数，释放非托管资源
        /// </summary>
        ~Database()
        {
            try
            {
                if (Connection != null)
                    Connection.Close();
            }
            catch { }
            try
            {
                Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 保护方法，打开数据库连接。
        /// </summary>
        protected void Open()
        {
            if (Connection == null)
            {
                Connection = new SqlConnection(ConnectionString);
            }
            if (Connection.State.Equals(ConnectionState.Closed))
            {
                Connection.Open();
            }
        }

        /// <summary>
        /// 公有方法，关闭数据库连接。
        /// </summary>
        public void Close()
        {
            if (Connection != null)
                Connection.Close();
        }

        /// <summary>
        /// 公有方法，释放资源。
        /// </summary>
        public void Dispose()
        {
            // 确保连接被关闭
            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }

        /// <summary>
        /// 公有方法，获取数据，返回一个DataSet。
        /// </summary>
        /// <param name="SqlString">Sql语句</param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(String SqlString)
        {
            DataSet dataset = new DataSet();
            Open();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(SqlString, Connection);
                adapter.Fill(dataset);
            }
            catch
            {
            }
            finally
            {
                Close();
            }
            return dataset;
        }

        /// <summary>
        /// 公有方法，获取数据，返回一个DataView。
        /// </summary>
        /// <param name="SqlString">Sql语句</param>
        /// <returns>DataView</returns>
        public DataView GetDataView(string strSQL)
        {
            DataSet Rs = new DataSet();
            Open();
            try
            {
                SqlDataAdapter SqlAdp = new SqlDataAdapter(strSQL, Connection);
                SqlAdp.Fill(Rs);
            }
            catch
            {
            }
            finally
            {
                Close();
            }
            return Rs.Tables[0].DefaultView;
        }

        /// <summary>
        /// 公有方法，获取数据，返回一个DataRow。
        /// </summary>
        /// <param name="SqlString">Sql语句</param>
        /// <returns>DataRow</returns>
        public DataRow GetDataRow(String SqlString)
        {
            DataSet dataset = GetDataSet(SqlString);
            dataset.CaseSensitive = false;
            if (dataset.Tables[0].Rows.Count > 0)
            {
                return dataset.Tables[0].Rows[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 公有方法，获取数据，返回一个DataTable。
        /// </summary>
        /// <param name="SqlString">Sql语句</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(String SqlString)
        {
            DataSet dataset = GetDataSet(SqlString);
            dataset.CaseSensitive = false;
            return dataset.Tables[0];
        }

        /// <summary>
        /// 公有方法，执行Sql语句。
        /// </summary>
        /// <param name="SqlString">Sql语句</param>
        /// <returns>对Update、Insert、Delete为影响到的行数，其他情况为-1</returns>
        public int ExecuteSQL(String SqlString)
        {
            int count = -1;
            Open();
            try
            {
                SqlCommand cmd = new SqlCommand(SqlString, Connection);
                count = cmd.ExecuteNonQuery();
            }
            catch
            {
                count = -1;
            }
            finally
            {
                Close();
            }
            return count;
        }

        /// <summary>
        /// 公有方法，执行一组Sql语句。
        /// </summary>
        /// <param name="SqlStrings">Sql语句组</param>
        /// <returns>是否成功</returns>
        public bool ExecuteSQL(String[] SqlStrings)
        {
            bool success = true;
            Open();
            SqlCommand cmd = new SqlCommand();
            SqlTransaction trans = Connection.BeginTransaction();
            cmd.Connection = Connection;
            cmd.Transaction = trans;

            int i = 0;
            try
            {
                foreach (String str in SqlStrings)
                {
                    cmd.CommandText = str;
                    cmd.ExecuteNonQuery();
                    i++;
                }
                trans.Commit();
            }
            catch
            {
                success = false;
                trans.Rollback();
            }
            finally
            {
                Close();
            }
            return success;
        }

        /// <summary>
        /// 公有方法，在一个数据表中插入一条记录。
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="Cols">哈西表，键值为字段名，值为字段值</param>
        /// <returns>是否成功</returns>
        public bool Insert(String TableName, Hashtable Cols)
        {
            int Count = 0;

            if (Cols.Count <= 0)
            {
                return true;
            }

            String Fields = " (";
            String Values = " Values(";
            foreach (DictionaryEntry item in Cols)
            {
                if (Count != 0)
                {
                    Fields += ",";
                    Values += ",";
                }
                Fields += "[" + item.Key.ToString() + "]";
                Values += item.Value.ToString();
                Count++;
            }
            Fields += ")";
            Values += ")";

            String SqlString = "Insert into " + TableName + Fields + Values;

            String[] Sqls = { SqlString };
            return ExecuteSQL(Sqls);
        }

        /// <summary>
        /// 公有方法，更新一个数据表。
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="Cols">哈西表，键值为字段名，值为字段值</param>
        /// <param name="Where">Where子句</param>
        /// <returns>是否成功</returns>
        public bool Update(String TableName, Hashtable Cols, String Where)
        {
            int Count = 0;
            if (Cols.Count <= 0)
            {
                return true;
            }
            String Fields = " ";
            foreach (DictionaryEntry item in Cols)
            {
                if (Count != 0)
                {
                    Fields += ",";
                }
                Fields += "[" + item.Key.ToString() + "]";
                Fields += "=";
                Fields += item.Value.ToString();
                Count++;
            }
            Fields += " ";

            String SqlString = "Update " + TableName + " Set " + Fields + Where;

            String[] Sqls = { SqlString };
            return ExecuteSQL(Sqls);
        }

    }
}