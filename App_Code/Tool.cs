/*
 * 名称：工具
 * 版本：1.0.0
 * 更新：2015年8月1日 22:11:16
 * 作者：xuqingkai.com
 * 邮箱：id@xuqingkai.com
 * 网站：www.xuqingkai.com
 * 版权：开源（MIT LICENE）
 * 
 * 2015-08-29 16:40:23
 * 优化了Include方法，支持逗号分割，或多参数传入
 * 增加接口：订单查询，退单，销单
*/
using System;
namespace SH
{
	/// <summary>
	/// 工具
	/// </summary>
	public partial class Tool : System.Web.UI.Page
	{
		#region 数据库操作

		/// <summary>
		/// oleDbConnection字符串
		/// </summary>
		private string _oleDbConnectionString;

		/// <summary>
		/// 表名
		/// </summary>
		private string _table = "";

		/// <summary>
		/// 字段
		/// </summary>
		private string _field = "";

		/// <summary>
		/// 条件
		/// </summary>
		private string _where = "";

		/// <summary>
		/// 排序
		/// </summary>
		private string _order = "";

		/// <summary>
		/// 联合查询
		/// </summary>
		private System.Collections.ArrayList _join = new System.Collections.ArrayList();

		/// <summary>
		/// 每页条数
		/// </summary>
		private int _pageSize = 0;

		/// <summary>
		/// 总条数
		/// </summary>
		private int _pageTotal = 0;

		/// <summary>
		/// SQLServer数据库（OLEDB）
		/// </summary>
		/// <param name="db">数据库名</param>
		/// <param name="id">用户</param>
		/// <param name="pw">密码</param>
		/// <returns></returns>
		public Tool(string db, string id, string pw)
		{
			OleDbConnectString("127.0.0.1", db, id, pw);
		}

		/// <summary>
		/// 连接数据库
		/// </summary>
		/// <param name="ip">服务器IP</param>
		/// <param name="db">数据库名</param>
		/// <param name="id">用户</param>
		/// <param name="pw">密码</param>
		/// <returns></returns>
		public Tool(string ip = null, string db = null, string id = null, string pw = null)
		{
			string database = (ip + ";" + db + ";" + id + ";" + pw).Trim(';');
			if ("127.0.0.1/::1".Contains(System.Web.HttpContext.Current.Request.ServerVariables["Local_Addr"]))
			{
				database = System.Configuration.ConfigurationManager.AppSettings["SH.Tool.LocalDataBase"] + "";
			}
			if (database.Length == 0) { database = System.Configuration.ConfigurationManager.AppSettings["SH.Tool.DataBase"] + ""; }
			if (database.ToLower().StartsWith("provider") == false)
			{
				if (database.Length == 0)
				{
					database = System.Web.HttpContext.Current.Server.MapPath("/") + "data.accdb";
					if (System.IO.File.Exists(database)) { OleDbConnectString(database); }
				}
				else if (database.ToLower().EndsWith(".accdb"))
				{
					OleDbConnectString(database);
				}
				else if (database.Trim(';').Split(';').Length == 4)
				{
					string[] array = database.Split(';');
					OleDbConnectString(array[0], array[1], array[2], array[3]);
				}
				else if (database.Split('&').Length == 4 && database.Split('=').Length == 5)
				{
					System.Collections.Specialized.NameValueCollection nvc = System.Web.HttpUtility.ParseQueryString(database);
					OleDbConnectString(nvc["ip"], nvc["db"], nvc["id"], nvc["pw"]);
				}
			}
			else
			{
				_oleDbConnectionString = database;
			}

		}

		/// <summary>
		/// ACCESS数据库（OLEDB）
		/// </summary>
		/// <param name="filePath">文件路径</param>
		/// <returns></returns>
		private string OleDbConnectString(string filePath)
		{
			if (!filePath.Contains(":")) { filePath = System.Web.HttpContext.Current.Server.MapPath("/") + filePath; }
			_oleDbConnectionString = "Provider=Microsoft." + (filePath.ToLower().EndsWith(".accdb") ? "Ace.Oledb.12.0" : "Jet.Oledb.4.0") + ";";
			_oleDbConnectionString += "Data Source=" + filePath + ";";
			return _oleDbConnectionString;
		}

		/// <summary>
		/// SQLServer数据库（OLEDB）
		/// </summary>
		/// <param name="db">数据库名</param>
		/// <param name="id">用户</param>
		/// <param name="pw">密码</param>
		/// <returns></returns>
		public string OleDbConnectString(string db, string id, string pw)
		{
			return OleDbConnectString("127.0.0.1", db, id, pw);
		}

		/// <summary>
		/// SQLServer数据库（OLEDB）
		/// </summary>
		/// <param name="ip">服务器IP</param>
		/// <param name="db">数据库名</param>
		/// <param name="id">用户</param>
		/// <param name="pw">密码</param>
		/// <returns></returns>
		public string OleDbConnectString(string ip, string db, string id, string pw)
		{
			_oleDbConnectionString = "Provider=SQLOLEDB;";
			_oleDbConnectionString += "User ID=" + id + ";";
			_oleDbConnectionString += "Password=" + pw + ";";
			_oleDbConnectionString += "Initial Catalog=" + db + ";";
			_oleDbConnectionString += "Data Source=" + ip + ";";
			return _oleDbConnectionString;
		}

		/// <summary>
		/// 判断是不是Sqlserver
		/// </summary>
		/// <returns></returns>
		public bool IsSqlserver() { return _oleDbConnectionString.ToLower().Replace(" ", "").Contains("provider=sqloledb;"); }


		/// <summary>
		/// 指定表名
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns></returns>
		public Tool Table(string table)
		{
			if (_oleDbConnectionString == null) { Alert(1, "数据库连接不存在"); }
			_table = table;
			return this;
		}

		/// <summary>
		/// 指定联合查询表名和条件
		/// </summary>
		/// <param name="table">表名</param>
		/// <param name="on">联合条件</param>
		/// <returns></returns>
		public Tool Join(string table, string on, string alias = null)
		{
			_join.Add(table + "\r" + on + "\r" + alias); return this;
		}

		/// <summary>
		/// 设定查询字段
		/// </summary>
		/// <param name="field">字段名，以逗号分隔</param>
		/// <returns></returns>
		public Tool Field(string field) { _field = (_field + "," + field).Trim(','); return this; }

		/// <summary>
		/// 查询字段
		/// </summary>
		/// <param name="field">字段名，以逗号分隔</param>
		/// <returns></returns>
		public Tool Find(string field) { return Field(field); }

		/// <summary>
		/// 查询条件
		/// </summary>
		/// <param name="where">条件语句</param>
		/// <returns></returns>
		public Tool Where(string where) { _where += (_where.Length == 0 ? "" : " AND ") + where; return this; }

		/// <summary>
		/// 排序字段
		/// </summary>
		/// <param name="order">字段名，以逗号分隔</param>
		/// <returns></returns>
		public Tool Order(string order) { _order += (_order.Length == 0 ? "" : " ,") + order; return this; }

		/// <summary>
		/// 数据记录总数
		/// </summary>
		/// <returns></returns>
		public int PageTotal() { return _pageTotal; }


		/// <summary>
		/// 设为分页显示
		/// </summary>
		/// <param name="pageSize">每页显示条数</param>
		/// <returns></returns>
		public Tool PageSize(int pageSize) { _pageSize = pageSize; return this; }

		/// <summary>
		/// sql清理
		/// </summary>
		private void SqlClear()
		{
			_join.Clear(); _field = ""; _where = ""; _order = "";
		}

		/// <summary>
		/// 执行ExecuteNonQuery，返回受影响的记录条数
		/// </summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="oleDbConnectionString">数据库链接字符串</param>
		/// <returns></returns>
		public int ExecuteNonQuery(string sql)
		{
			int result = 0;
			using (System.Data.OleDb.OleDbConnection oleDbConnection = new System.Data.OleDb.OleDbConnection(_oleDbConnectionString))
			{
				System.Data.OleDb.OleDbCommand oleDbCommand = new System.Data.OleDb.OleDbCommand(sql, oleDbConnection);
				try
				{
					oleDbConnection.Open();
					result = oleDbCommand.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					System.Web.HttpContext.Current.Response.Write("<!-- " + sql + " -->" + ex.Message);
					System.Web.HttpContext.Current.Response.End();
				}
			}
			SqlClear();
			return result;
		}

		/// <summary>
		/// 事务
		/// </summary>
		/// <param name="sqlArrayList">sql语句</param>
		/// <param name="oleDbConnectionString">数据库链接字符串</param>
		/// <returns></returns>
		public int ExecuteNonQuery(System.Collections.ArrayList sqlArrayList)
		{
			int result = 0;
			using (System.Data.OleDb.OleDbConnection oleDbConnection = new System.Data.OleDb.OleDbConnection(_oleDbConnectionString))
			{
				System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand();
				cmd.Connection = oleDbConnection;
				System.Data.OleDb.OleDbTransaction transaction = null;
				try
				{
					oleDbConnection.Open();
					transaction = oleDbConnection.BeginTransaction();
					cmd.Transaction = transaction;
					foreach (string sql in sqlArrayList) { cmd.CommandText = sql; result += cmd.ExecuteNonQuery(); }
					transaction.Commit();
				}
				catch
				{
					transaction.Rollback(); result = -1;
				}
			}
			return result;
		}

		/// <summary>
		/// 执行ExecuteScalar，返回第一个字段的值
		/// </summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="oleDbConnectionString">数据库链接字符串</param>
		/// <returns></returns>
		public object ExecuteScalar(string sql)
		{
			object result = null;
			using (System.Data.OleDb.OleDbConnection oleDbConnection = new System.Data.OleDb.OleDbConnection(_oleDbConnectionString))
			{
				System.Data.OleDb.OleDbCommand oleDbCommand = new System.Data.OleDb.OleDbCommand(sql, oleDbConnection);
				try
				{
					oleDbConnection.Open();
					result = oleDbCommand.ExecuteScalar();
					oleDbConnection.Close();
				}
				catch (Exception ex)
				{
					System.Web.HttpContext.Current.Response.Write("<!-- " + sql + " -->" + ex.Message);
					System.Web.HttpContext.Current.Response.End();
				}
			}
			SqlClear();
			return result;
		}

		/// <summary>
		/// 执行OleDbDataAdapter，返回DataTable
		/// </summary>
		/// <param name="sql">SQL语句</param>
		/// <returns></returns>
		public System.Data.DataTable DataAdapter(string sql, System.Data.OleDb.OleDbParameter[] parameters = null)
		{
			System.Data.DataTable dataTable = new System.Data.DataTable();
			using (System.Data.OleDb.OleDbConnection oleDbConnection = new System.Data.OleDb.OleDbConnection(_oleDbConnectionString))
			{
				System.Data.OleDb.OleDbDataAdapter oleDbDataAdapter = null;
				System.Data.OleDb.OleDbCommand oleDbCommand = new System.Data.OleDb.OleDbCommand();

				if (parameters != null)
				{
					oleDbCommand.Connection = oleDbConnection;
					oleDbCommand.CommandText = sql;
					//oleDbCommandcmd.Transaction = 事务;
					oleDbCommand.CommandType = System.Data.CommandType.Text;//cmdType;
					foreach (System.Data.OleDb.OleDbParameter parameter in parameters)
					{
						if ((parameter.Direction == System.Data.ParameterDirection.InputOutput || parameter.Direction == System.Data.ParameterDirection.Input) &&
							 (parameter.Value == null))
						{
							parameter.Value = DBNull.Value;
						}
						oleDbCommand.Parameters.Add(parameter);
					}
					oleDbDataAdapter = new System.Data.OleDb.OleDbDataAdapter(oleDbCommand);
				}
				else
				{
					oleDbDataAdapter = new System.Data.OleDb.OleDbDataAdapter(sql, oleDbConnection);
				}
				try
				{
					oleDbConnection.Open();
					oleDbDataAdapter.Fill(dataTable);
				}
				catch (System.Data.OleDb.OleDbException ex)
				{
					System.Web.HttpContext.Current.Response.Write("<!-- " + sql + " -->" + ex.Message);
					System.Web.HttpContext.Current.Response.End();
				}
			}
			SqlClear();
			return dataTable;
		}

		/// <summary>
		/// 多行数据集合
		/// </summary>
		/// <param name="top">条数（每页条数）</param>
		/// <param name="pageTotal">默认小于0不分页，等于0所有条数分页，其他代表指定条数分页</param>
		/// <returns></returns>
		public System.Data.DataTable DataTable(int top = 0, int pageTotal = -1)
		{
			string join = "", field = _field, where = _where, order = _order;
			join = "[" + _table + "]";
			if (_join.Count > 0)
			{
				join = join + " AS t";
				int i = 0;
				foreach (string items in _join)
				{
					string[] item = items.Split('\r');
					if (item[0].ToUpper().Contains(" AS ") == false) 
					{
						i++;
						if (item[2].Length == 0) { item[2] = "t" + i; }
						item[0] = "[" + item[0] + "] AS " + item[2]; 
					}
					join = "(" + join + " LEFT JOIN " + item[0] + " ON " + item[1] + ")";
				}
				if (field == "") { field = "t.*"; }

			}
			if (field == "") { field = "*"; }
			if (where != "") { where = " WHERE " + where; }
			if (order != "") { order = " ORDER BY " + order; }
			System.Data.DataTable dataTable = new System.Data.DataTable();

			//无分页
			if (pageTotal < 0)
			{
				string sql = "SELECT";
				if (top > 0) { sql += " TOP " + top + " "; }
				sql += " " + field + " FROM " + join + where;
				dataTable = DataAdapter(sql);
			}
			else
			{
				_pageSize = top; if (_pageSize <= 0) { _pageSize = 1; }
				int page = 0; Int32.TryParse(System.Web.HttpContext.Current.Request.QueryString["page"] + "", out page);
				int start = _pageSize * (page > 0 ? page - 1 : 0);
				int end = start + _pageSize;

				if (IsSqlserver())
				{
					_pageTotal = (int)ExecuteScalar("SELECT COUNT(1) FROM " + join + where);
					if (order == "")
					{
						order = "ID DESC";
						if (_join.Count > 0) { order = "t." + order; }
						order = " ORDER BY " + order;
					}
					string sql = "WITH Row_Number_Table AS (SELECT Row_Number() OVER (" + order + ") AS Row_Number_SortID," + field + " FROM " + join + " " + where + ") ";
					sql += "SELECT * FROM Row_Number_Table WHERE (Row_Number_SortID BETWEEN " + (start + 1) + " AND " + end + ")";
					dataTable = DataAdapter(sql);
				}
				else
				{
					dataTable = DataAdapter("SELECT " + field + " FROM " + join + where + order);
					_pageTotal = dataTable.Rows.Count;
					if (end > dataTable.Rows.Count) { end = dataTable.Rows.Count; }
					System.Data.DataTable dataTableClone = dataTable.Clone();
					for (int j = start; j < end; j++) { dataTableClone.ImportRow(dataTable.Rows[j]); }
					dataTable = dataTableClone;
				}
			}
			return dataTable;
		}

		/// <summary>
		/// 多行数据集合
		/// </summary>
		/// <param name="top">条数（每页条数）</param>
		/// <param name="pageTotal">默认小于0不分页，等于0所有条数分页，其他代表指定条数分页</param>
		/// <returns></returns>
		public System.Data.DataRowCollection DataRows(int top = 0, int pageTotal = -1)
		{
			return DataTable(top, pageTotal).Rows;
		}

		/// <summary>
		/// 多行数据列表
		/// </summary>
		/// <param name="top">条数（每页条数）</param>
		/// <param name="pageTotal">默认小于0不分页，等于0所有条数分页，其他代表指定条数分页</param>
		/// <returns></returns>
		public System.Collections.Generic.List<dynamic> DataList(int top = 0, int pageTotal = -1)
		{
			System.Collections.Generic.List<dynamic> list = new System.Collections.Generic.List<dynamic>();
			foreach (System.Data.DataRow dataRow in DataRows(top, pageTotal)) { list.Add(DataRowToModel(dataRow)); }
			return list;
		}

		/// <summary>
		/// 多列数据列表
		/// </summary>
		/// <param name="top">条数（每页条数）</param>
		/// <param name="pageTotal">默认小于0不分页，等于0所有条数分页，其他代表指定条数分页</param>
		/// <returns></returns>
		public System.Collections.Generic.List<T> DataList<T>(int top = 0, int pageTotal = -1) where T : class, new()
		{
			System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();
			foreach (System.Data.DataRow dataRow in DataRows(top, pageTotal)) { list.Add(DataRowToModel<T>(dataRow)); }
			return list;
		}

		/// <summary>
		/// 单行数据
		/// </summary>
		/// <returns></returns>
		public System.Data.DataRow DataRow()
		{
			System.Data.DataTable dataTable = DataTable(1);
			System.Data.DataRowCollection dataRows = dataTable.Rows;
			System.Data.DataRow dataRow = dataTable.NewRow();
			if (dataRows.Count > 0) { dataRow = dataRows[0]; } else { dataRow = null; }
			return dataRow;
		}

		/// <summary>
		/// 单条数据虚拟实体
		/// </summary>
		/// <returns></returns>
		public dynamic DataModel(int noData = 0)
		{
			if (noData > 0)
			{
				System.Data.DataTable dataTable = Where("1=2").DataTable(1);
				dynamic model = new System.Dynamic.ExpandoObject();
				foreach (System.Data.DataColumn dc in dataTable.Columns)
				{
					((System.Collections.Generic.IDictionary<string, object>)model).Add(dc.ColumnName, "");
				}
				return model;
			}
			return DataRowToModel(DataRow());
		}

		/// <summary>
		/// 单条数据实体
		/// </summary>
		/// <returns></returns>
		public T DataModel<T>() where T : class, new()
		{
			return DataRowToModel<T>(DataRow());
		}

		/// <summary>
		/// 第一行第一列的值
		/// </summary>
		/// <returns></returns>
		public object DataColumn()
		{
			string join = "", field = _field, where = _where, order = _order;
			join = "[" + _table + "]";
			if (_join.Count > 0)
			{
				join = join + " AS t";
				int i = 0;
				foreach (string items in _join)
				{
					string[] item = items.Split('\r');
					if (item[0].ToUpper().Contains(" AS ") == false)
					{
						i++;
						if (item[2].Length == 0) { item[2] = "t" + i; }
						item[0] = "[" + item[0] + "] AS " + item[2];
					}
					join = "(" + join + " LEFT JOIN " + item[0] + " ON " + item[1] + ")";
				}
				if (field == "") { field = "t.*"; }
			}
			if (field == "") { field = "*"; }
			if (where != "") { where = " WHERE " + where; }
			if (order != "") { order = " ORDER BY " + order; }
			string sql = "SELECT TOP 1 " + field + " FROM " + join + where;
			return ExecuteScalar(sql);
		}

		/// <summary>
		/// 分页
		/// </summary>
		/// <param name="template">地址栏模版，如：/news-{cid}-{page}.html，不设置则为请求参数式分页</param>
		/// <returns></returns>
		public string DataPage(object template = null, string pageClass = "current", string[] pageText = null)
		{
			System.Collections.ArrayList list = DataPageList(pageClass, pageText);

			//生成页码
			string result = "", url = template + "";
			System.Collections.Specialized.NameValueCollection request = System.Web.HttpContext.Current.Request.QueryString;
			if (url.Length > 0)
			{
				foreach (string key in request)
				{
					if ((key + "").ToLower() != "page")
					{
						url = url.Replace(("{" + key + "}").ToLower(), request[key + ""]);
					}
				}
				foreach (string[] p in list)
				{
					result += "　<a href=\"" + url.Replace("{page}", p[0]) + "\"" + (p[2].Length > 0 ? " class=\"" + p[2] + "\"" : "") + ">" + p[1] + "</a>";
				}
			}
			else
			{
				foreach (string key in request)
				{
					if (key.ToString().ToLower() != "page") { url += key + "=" + request[key + ""] + "&"; }
				}
				url = System.Web.HttpContext.Current.Request.FilePath + "?" + url;
				foreach (string[] p in list)
				{
					result += "　<a href=\"" + url + "page=" + p[0] + "\"" + p[2] + ">" + p[1] + "</a>";
				}
			}
			return result;
		}

		/// <summary>
		/// 分页数组
		/// </summary>
		/// <param name="pageClass">当前页样式</param>
		/// <param name="pageText">页码文字</param>
		/// <returns></returns>
		public System.Collections.ArrayList DataPageList(string pageClass = "current", string[] pageText = null)
		{
			//起止页码
			int page = 0;
			Int32.TryParse(System.Web.HttpContext.Current.Request.QueryString["Page"] + "", out page);
			if (page < 1) { page = 1; }
			int pageSize = _pageSize;
			if (pageSize < 1) { pageSize = _pageTotal; }
			int maxPage = 0; if (pageSize > 0) { maxPage = (Int32)(Math.Ceiling((double)_pageTotal / pageSize)); }
			if (page > maxPage) { page = maxPage; }
			int startPage = page - 4; if (startPage < 1) { startPage = 1; }
			int endPage = page + 4; if (endPage > maxPage) { endPage = maxPage; }
			//实际页码
			System.Collections.ArrayList list = new System.Collections.ArrayList();
			if (pageText == null) { pageText = "首页,上页,下页,末页".Split(','); }
			if (startPage > 1) { list.Add(("1\r" + pageText[0] + "\r").Split('\r')); }
			if (page > 1) { list.Add(((page - 1) + "\r" + pageText[1] + "\r").Split('\r')); }
			for (int i = startPage; i <= endPage; i++) { list.Add((i + "\r" + i + "\r" + (i == page ? pageClass : "")).Split('\r')); }
			if (page < endPage) { list.Add(((page + 1) + "\r" + pageText[2] + "\r").Split('\r')); }
			if (page < maxPage) { list.Add((maxPage + "\r" + pageText[3] + "\r").Split('\r')); }
			return list;

		}

		/// <summary>
		/// 插入
		/// </summary>
		/// <param name="field">字段名</param>
		/// <param name="value">内容</param>
		/// <returns></returns>
		public int Insert(string field, string value = "")
		{
			if (value.Length == 0) { value = field; field = ""; }
			if (field.Length == 0)
			{
				field = _where;
			}
			else if (_where.Length > 0)
			{
				field += "," + _where;
			}
			string sql = "INSERT INTO [" + _table + "] (" + field.Trim(',') + ") VALUES (" + value + ")";
			return ExecuteNonQuery(sql);

		}

		/// <summary>
		/// 更新
		/// </summary>
		/// <param name="value">内容</param>
		/// <param name="where">条件</param>
		/// <returns></returns>
		public int Update(string value, string where)
		{
			where += "";
			string sql = "UPDATE [" + _table + "] SET " + value;
			if (where.Length == 0)
			{
				where = _where;
			}
			else if (_where.Length > 0)
			{
				where += " AND " + _where;
			}
			//Debug("["+where+"]");
			if (where.Length > 0) { sql += " WHERE " + where; }
			return ExecuteNonQuery(sql);

		}

		/// <summary>
		/// 删除
		/// </summary>
		/// <param name="where">条件</param>
		/// <returns></returns>
		public int Delete(string where = "")
		{
			string sql = "DELETE FROM [" + _table + "]";
			if (where.Length == 0)
			{
				where = _where;
			}
			else if (_where.Length > 0)
			{
				where += " AND " + _where;
			}
			if (where.Length > 0) { sql += " WHERE " + where; }
			return ExecuteNonQuery(sql);

		}

		/// <summary>
		/// 获取配置信息
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public string Cfg(int id, string field = "Contents")
		{
			return Table("Config").Field(field).Where("ID=" + id).DataColumn().ToString();
		}

		/// <summary>
		/// 获取单页信息
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public string Info(int id, string field = "Contents")
		{
			return Table("Info").Field(field).Where("ID=" + id).DataColumn().ToString();
		}

		#endregion

		/// <summary>
		/// 响应输出
		/// </summary>
		/// <param name="id"></param>
		/// <param name="message"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static string Alert(int ret, object message = null, string result = null, string callback = null)
		{
			System.Web.HttpContext.Current.Response.Clear();
			string json = "";
			json += ",\"ret\":\"" + ret + "\"";
			json += ",\"message\":\"" + message + "\"";
			if (result == null) { result = "null"; }
			else if (result.StartsWith("{") == false) { result = "\"" + result + "\""; }
			json += ",\"result\":" + result + "";
			result = "{" + json.Trim(',') + "}";
			result = (result + "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
			if (callback != null) { result = callback + "(" + result + ");"; }
			System.Web.HttpContext.Current.Response.Write(result);
			System.Web.HttpContext.Current.Response.End();
			return null;
		}

		/// <summary>
		/// 获取或设置Web.Config的appSettings
		/// </summary>
		/// <param name="key">键名</param>
		/// <param name="value">键值</param>
		/// <returns></returns>
		public static string AppSettings(string key, string value = null)
		{
			if (value == null) { return System.Configuration.ConfigurationManager.AppSettings["SH.Weixin.Token"] + ""; }

			System.Configuration.Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
			System.Configuration.AppSettingsSection appSettings = (System.Configuration.AppSettingsSection)config.GetSection("appSettings");
			if (appSettings.Settings[key] != null)
			{
				appSettings.Settings[key].Value = value;
			}
			else
			{
				appSettings.Settings.Add(key, value);
			}
			config.Save();
			config = null;
			return value;
		}

		/// <summary>
		/// BASE64编码
		/// </summary>
		/// <param name="source">要编码的明文</param>
		/// <returns></returns>
		public static string Base64Encode(object source, string charset = "UTF-8")
		{
			return Convert.ToBase64String(System.Text.Encoding.GetEncoding(charset).GetBytes(source + ""));
		}

		/// <summary>
		/// Base64解码
		/// </summary>
		/// <param name="result">待解密的密文</param>
		/// <returns></returns>
		public static string Base64Decode(object source, string charset = "UTF-8")
		{
			return System.Text.Encoding.GetEncoding(charset).GetString(Convert.FromBase64String(source + ""));
		}

		/// <summary>
		/// 设置缓存
		/// </summary>
		/// <param name="key">键名</param>
		/// <param name="value">键值</param>
		/// <param name="seconds">过期秒数</param>
		public static string Cache(string key, object value = null, int seconds = 0)
		{
			System.Web.Caching.Cache cache = System.Web.HttpRuntime.Cache;
			string result = "";
			if (value == null)
			{
				result = cache[key] + "";
			}
			else
			{
				result = value + "";
				if (seconds > 0)
				{
					cache.Insert(key, value, null, DateTime.Now.AddSeconds(seconds), System.Web.Caching.Cache.NoSlidingExpiration);
				}
				else
				{
					cache.Insert(key, value);
				}
			}
			return result;
		}

		/// <summary>
		/// 清除（然后输出，并终止后续后续程序）
		/// </summary>
		/// <param name="obj">要输出的对象</param>
		public static void Clear(object obj = null)
		{
			System.Web.HttpContext.Current.Response.Clear();
			if (obj != null)
			{
				System.Web.HttpContext.Current.Response.Write(obj);
				System.Web.HttpContext.Current.Response.End();
			}
		}

		/// <summary>
		/// 清除缓存
		/// </summary>
		public static void ClearCache()
		{
			System.Web.HttpContext.Current.Response.Buffer = true;
			System.Web.HttpContext.Current.Response.ExpiresAbsolute = DateTime.Now.AddSeconds(-1);
			System.Web.HttpContext.Current.Response.Expires = 0;
			System.Web.HttpContext.Current.Response.CacheControl = "no-cache";
			System.Web.HttpContext.Current.Response.AppendHeader("Pragma", "No-Cache");
		}

		/// <summary>
		/// 清除页面缓存
		/// </summary>
		public static void ClearPageCache()
		{
			System.Web.HttpContext.Current.Response.Buffer = true;
			System.Web.HttpContext.Current.Response.ExpiresAbsolute = DateTime.Now.AddSeconds(-1);
			System.Web.HttpContext.Current.Response.Expires = 0;
			System.Web.HttpContext.Current.Response.CacheControl = "no-cache";
			System.Web.HttpContext.Current.Response.AppendHeader("Pragma", "No-Cache");
		}

		/// <summary>
		/// COOKIE操作
		/// </summary>
		/// <param name="key">Cookie名</param>
		/// <param name="obj">Cookie值，为空则代表取值</param>
		/// <param name="seconds">过期秒数</param>
		/// <returns></returns>
		public static string Cookie(string key, object obj = null, int seconds = 0)
		{
			//2592000 = 60 * 60 * 24 * 30;
			String result = obj + "";
			if (obj == null)
			{
				result = System.Web.HttpContext.Current.Request.Cookies[key] == null ? "" : System.Web.HttpContext.Current.Request.Cookies[key].Value;
			}
			else
			{
				System.Web.HttpContext.Current.Response.Cookies[key].Value = obj + "";
				if (seconds > 0)
				{
					System.Web.HttpContext.Current.Response.Cookies[key].Expires = DateTime.Now.AddSeconds(seconds);
				}
			}
			return result;
		}

		/// <summary>
		/// 数据行转动态实体
		/// </summary>
		/// <param name="dataRow">数据行</param>
		/// <returns></returns>
		public static dynamic DataRowToModel(System.Data.DataRow dataRow)
		{
			dynamic model = null;
			if (dataRow != null)
			{
				model = new System.Dynamic.ExpandoObject();
				foreach (System.Data.DataColumn dc in dataRow.Table.Columns)
				{
					((System.Collections.Generic.IDictionary<string, object>)model).Add(dc.ColumnName, dataRow[dc]);
				}
			}
			return model;
		}

		/// <summary>
		/// 数据行转实体
		/// </summary>
		/// <param name="dataRow">数据行</param>
		/// <returns></returns>
		public static T DataRowToModel<T>(System.Data.DataRow dataRow) where T : class, new()
		{
			T model = null;
			if (dataRow != null)
			{
				model = new T();
				foreach (var p in model.GetType().GetProperties())
				{
					Type type = Type.GetType(p.PropertyType.ToString().Replace("System.Nullable`1[", "").TrimEnd(']'));
					if (type.ToString().StartsWith("System.") && dataRow.Table.Columns.Contains(p.Name.ToString()))
					{
						p.SetValue(model, dataRow[p.Name.ToString()] == DBNull.Value ? null : Convert.ChangeType(dataRow[p.Name.ToString()], type), null);
					}
				}
			}
			return model;
		}

		/// <summary>
		/// 调试
		/// </summary>
		/// <param name="content">内容</param>
		/// <param name="name">生成文件名</param>
		/// <returns></returns>
		public static string Debug(object content, object name = null)
		{
			if (name == null)
			{
				System.Web.HttpContext.Current.Response.Clear();
				System.Web.HttpContext.Current.Response.Write(content);
				System.Web.HttpContext.Current.Response.End();
			}
			else
			{
				string filename = name + "";
				if (filename.Length == 0) { filename = System.DateTime.Now.ToString("yyyyMMddHHmmssffffff"); }
				if (!filename.Contains(".")) { filename += ".txt"; }
				filename = System.Web.HttpContext.Current.Server.MapPath(filename);
				System.IO.File.AppendAllText(filename, content + "\n\n", System.Text.Encoding.UTF8);
			}
			return null;
		}

		/// <summary>
		/// 转数字
		/// </summary>
		/// <param name="obj">要处理的对象</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public static decimal Decimal(object obj, decimal defaultValue = 0)
		{
			decimal.TryParse(obj + "", out defaultValue);
			return defaultValue;
		}

		/// <summary>
		/// 301重定向
		/// </summary>
		/// <param name="domainName">唯一用来显示的域名</param>
		public static void Domain(string domainName)
		{
			string serverIP = ServerVariables("local_ADDR"), customIP = ServerVariables("REMOTE_ADDR");
			String host = System.Web.HttpContext.Current.Request.Url.Host.ToString().ToLower();
			if (serverIP != customIP && host != (domainName + "").ToLower())
			{
				System.Web.HttpContext.Current.Response.StatusCode = 301;
				System.Web.HttpContext.Current.Response.Status = "301 Moved Permanently";
				System.Web.HttpContext.Current.Response.AddHeader("Location", "http://" + domainName + System.Web.HttpContext.Current.Request.RawUrl);
				System.Web.HttpContext.Current.Response.End();
			}
		}

		/// <summary>
		/// 强制下载
		/// </summary>
		/// <param name="filePath">文件名</param>
		/// <param name="contentType">文件MIME类型</param>
		public static void Download(string filePath, string contentType = null)
		{
			string fileName = filePath.Replace("\\", "/").Replace(" ", "");
			if (fileName.Contains("/")) { fileName = fileName.Substring(fileName.LastIndexOf("/") + 1); }
			if (contentType == null && fileName.Contains("."))
			{
				contentType = "application/octet-stream";
				System.Collections.Generic.Dictionary<string, string> dict = new System.Collections.Generic.Dictionary<string, string>();
				dict.Add(".html", "text/html");
				dict.Add(".htm", "text/html");
				dict.Add(".ico", "image/x-icon");
				dict.Add(".bmp", "image/bmp");
				dict.Add(".gif", "image/gif");
				dict.Add(".jpg", "image/jpeg");
				dict.Add(".png", "image/png");
				dict.Add(".js", "application/x-javascript");
				dict.Add(".mp3", "audio/mpeg");
				dict.Add(".mp4", "video/mp4");
				dict.Add(".pdf", "application/pdf");
				dict.Add(".doc", "application/msword");
				dict.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
				dict.Add(".xls", "application/vnd.ms-excel");
				dict.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
				dict.Add(".ppt", "application/vnd.ms-powerpoint");
				dict.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
				dict.Add(".txt", "text/plain");
				dict.Add(".xml", "text/xml");
				string ext = fileName.Substring(fileName.LastIndexOf('.')).ToLower();
				if (dict.ContainsKey(ext)) { contentType = dict[ext]; }
			}
			System.Web.HttpContext.Current.Response.Clear();
			filePath = System.Web.HttpContext.Current.Server.MapPath(filePath);
			if (System.IO.File.Exists(filePath))
			{
				System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
				System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
				//不指明Content-Length用Flush的话不会显示下载进度   
				System.Web.HttpContext.Current.Response.AddHeader("Content-Length", fileInfo.Length.ToString());
				System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.Unicode;
				System.Web.HttpContext.Current.Response.ContentType = contentType;
				System.Web.HttpContext.Current.Response.TransmitFile(filePath, 0, fileInfo.Length);
			}
			else
			{
				System.Web.HttpContext.Current.Response.Write(fileName + " is not found");
			}
			System.Web.HttpContext.Current.Response.End();
		}

		/// <summary>
		/// 输出当前参数值，并终止后续程序
		/// </summary>
		public static void End(object obj = null)
		{
			if (obj != null)
			{
				System.Web.HttpContext.Current.Response.Write(obj);

			}
			System.Web.HttpContext.Current.Response.End();
		}

		/// <summary>
		/// 相对路径，不带域名和参数
		/// </summary>
		/// <returns></returns>
		public static string FilePath() { return System.Web.HttpContext.Current.Request.FilePath; }

		/// <summary>
		/// 过滤SQL
		/// </summary>
		/// <param name="sql">sql文</param>
		public static string Filter(object sql)
		{
			return (sql + "").Replace("'", "''");
		}

		/// <summary>
		/// GET值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <returns></returns>
		public static string Get(string key = null)
		{
			return key == null ? System.Web.HttpContext.Current.Request.QueryString.ToString() : System.Web.HttpContext.Current.Request.QueryString[key];
		}

		/// <summary>
		/// GET整数值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public static int GetID(string key = "id", double defaultID = 0)
		{
			Double.TryParse(System.Web.HttpContext.Current.Request.QueryString[key] + "", out defaultID);
			return (int)Math.Floor(defaultID);
		}

		/// <summary>
		/// GET同参数名的值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <returns></returns>
		public static string[] Gets(string key)
		{
			return System.Web.HttpContext.Current.Request.QueryString.GetValues(key);
		}

		/// <summary>
		/// HTTP请求
		/// </summary>
		/// <param name="url">地址</param>
		/// <param name="data">数据</param>
		/// <param name="contentType">编码或数据类型</param>
		/// <param name="files">上传或下载文件</param>
		/// <returns></returns>
		public static string Http(string url = null, string data = null, string contentType = null, string files = null)
		{
			string result = ""; byte[] bytes;
			if (data == null && files == null)//如果不是向外POST数据
			{
				if (url == null)//接收POST过来的数据
				{
					bytes = new byte[System.Web.HttpContext.Current.Request.InputStream.Length];
					System.Web.HttpContext.Current.Request.InputStream.Read(bytes, 0, bytes.Length);
					result = System.Text.Encoding.GetEncoding(contentType).GetString(bytes);
				}
				else//GET指定的地址,此时的contentType代表了编码，如UTF-8等
				{
					System.Net.WebClient WebClientObj = new System.Net.WebClient();
					bytes = WebClientObj.DownloadData(url);
					if (files == null)
					{
						if (contentType == null) { contentType = "UTF-8"; }
						result = System.Text.Encoding.GetEncoding(contentType).GetString(bytes);
					}
					else
					{
						if (files.Length == 0) { files = DateTime.Now.ToString("yyyyMMddHHmmssffffff.") + contentType; }
						if (files.Contains(":") == false) { files = System.Web.HttpContext.Current.Server.MapPath(files); }
						System.IO.File.WriteAllBytes(files, bytes);
					}
				}
			}
			else//向外POST数据
			{
				System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors) { return true; });
				System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
				System.IO.Stream requestStream = null;
				request.Method = "POST";
				if (contentType == null) { contentType = "application/x-www-form-urlencoded"; }
				string charset = "UTF-8";
				if (contentType.ToLower().Contains("charset=")) { charset = contentType.Substring(contentType.ToLower().IndexOf("charset=") + 8); }
				if (contentType.ToLower().StartsWith("multipart/form-data"))
				{
					//如果只是上传文件，可以把data参数当作files参数来用
					if (files == null && data != null) { files = data; data = null; }
					string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
					request.ContentType = contentType + "; boundary=" + boundary;
					request.KeepAlive = true;
					//request.Headers.Add("sign", "1234");
					request.Credentials = System.Net.CredentialCache.DefaultCredentials;
					requestStream = request.GetRequestStream();
					byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

					data = data + "";
					if (data.Length > 0)
					{
						foreach (string input in data.Split('&'))
						{
							requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
							string inputName = input.Substring(0, input.IndexOf('='));
							string inputValue = input.Substring(input.IndexOf('=') + 1);
							byte[] inputBytes = System.Text.Encoding.GetEncoding(charset).GetBytes("Content-Disposition: form-data; name=\"" + inputName + "\"\r\n\r\n" + inputValue);
							requestStream.Write(inputBytes, 0, inputBytes.Length);
						}
					}

					files = files + "";
					if (files.Length > 0)
					{
						foreach (string file in files.Split('&'))
						{
							string fileInputName = file.Substring(0, file.IndexOf('='));
							string filepath = System.Web.HttpContext.Current.Server.MapPath(file.Substring(file.IndexOf('=') + 1));
							if (System.IO.File.Exists(filepath))
							{
								string filename = filepath.Replace("\\", "/");
								if (filename.Contains("/")) { filename = filename.Substring(filename.LastIndexOf("/") + 1); }
								requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
								//此处可不指定Content-Type
								string fileData = "Content-Disposition: form-data; name=\"" + fileInputName + "\"; filename=\"" + filename + "\"\r\nContent-Type: {2}\r\n\r\n";
								byte[] fileByte = System.Text.Encoding.GetEncoding(charset).GetBytes(fileData);
								requestStream.Write(fileByte, 0, fileByte.Length);

								System.IO.FileStream fileStream = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
								byte[] fileBytes = new byte[Math.Min(4096, (int)fileStream.Length)]; int fileBytesRead = 0;
								while ((fileBytesRead = fileStream.Read(fileBytes, 0, fileBytes.Length)) != 0) { requestStream.Write(fileBytes, 0, fileBytesRead); }
								fileStream.Close();
							}
						}
					}
					byte[] footer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
					requestStream.Write(footer, 0, footer.Length);
				}
				else
				{
					request.ContentType = contentType;//"text/plain","text/html","text/xml","text/javascript","application/javascript","application/json"
					bytes = System.Text.Encoding.GetEncoding(charset).GetBytes(data);
					//request.ContentLength = bytes.Length;
					requestStream = request.GetRequestStream();
					requestStream.Write(bytes, 0, bytes.Length);
				}
				requestStream.Close();
				try
				{
					System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
					result = new System.IO.StreamReader(request.GetResponse().GetResponseStream(), System.Text.Encoding.GetEncoding(charset)).ReadToEnd();
				}
				catch
				{
				}

			}
			return result;
		}

		public static string HtmlEncode(object html) { return System.Web.HttpUtility.HtmlEncode(html); }

		/// <summary>
		/// 包含页面
		/// </summary>
		/// <param name="filePath">页面文件路径</param>
		/// <returns></returns>
		public static string Include(params string[] files)
		{
			string result = null;
			foreach (string filePath in String.Join(",", files).Replace("|",",").Split(','))
			{
				if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(filePath)))
				{
					System.IO.TextWriter stringWriter = new System.IO.StringWriter();
					System.Web.HttpContext.Current.Server.Execute(filePath, stringWriter);
					result += stringWriter.ToString();
				}
			}
			return result;
      }

		/// <summary>
		/// 转整数
		/// </summary>
		/// <param name="obj">要转换的字符串或者数字等</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public static int Int(object obj, double defaultValue = 0)
		{
			Double.TryParse(obj + "", out defaultValue);
			return (int)Math.Floor(defaultValue);
		}

		/// <summary>
		/// 反射执行某方法
		/// </summary>
		/// <param name="methodName">方法</param>
		public static void Invoke<T>(string methodName) where T : new()
		{
			Type type = new T().GetType();
			object obj = System.Activator.CreateInstance(type);
			System.Reflection.MethodInfo method = type.GetMethod(methodName);
			method.Invoke(obj, null);
		}

		/// <summary>
		/// IP
		/// </summary>
		/// <returns></returns>
		public static string IP()
		{
			var request = System.Web.HttpContext.Current.Request;
			string ip = (request.ServerVariables["HTTP_X_FORWARDED_FOR"] + ",").Split(',')[0];
			if (IsIP(ip) == false) { ip = request.ServerVariables["REMOTE_ADDR"] + ""; }
			if (IsIP(ip) == false) { ip = "0.0.0.0"; }
			return ip;

		}

		/// <summary>
		/// 是否中文
		/// </summary>
		/// <param name="obj">字符串</param>
		/// <returns></returns>
		public static bool IsChinese(object obj)
		{
			return IsMatch(obj + "", "^[\u4E00-\u9FA5]+$");
		}

		/// <summary>
		/// Email格式验证
		/// </summary>
		/// <param name="obj">字符串</param>
		public static bool IsEmail(object obj)
		{
			return IsMatch(obj + "", "^[\\w-]+(\\.[\\w-]+)*@[\\w-]+(\\.[\\w-]+)+$");
		}

		/// <summary>
		/// IP格式判断
		/// </summary>
		/// <returns></returns>
		public static bool IsIP(object strIP)
		{
			bool result = true;
			string[] ip = (strIP + ".").Split('.');
			if (ip.Length == 5)
			{
				for (int i = 0; i < 4; i++)
				{
					int n; if (!Int32.TryParse(ip[i], out n)) { n = -1; }
					if (ip[i].Length == 0 || n < 0 || n > 255) { result = false; break; }
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		/// <summary>
		/// 正则表达式验证
		/// </summary>
		/// <param name="str">字符串</param>
		/// <param name="patern">表达式</param>
		/// <returns></returns>
		public static bool IsMatch(object str, string patern)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(str + "", patern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// 手机号验证
		/// </summary>
		/// <param name="obj">字符串</param>
		public static bool IsMobile(object obj)
		{
			return IsMatch(obj + "", "^1(3|4|5|8)\\d{9}$");
		}

		/// <summary>
		/// 是否手机访问
		/// </summary>
		/// <param name="domain">手机站地址，一旦指定手机访问时将转向该地址</param>
		public static bool IsMobileVisit(string domain = null)
		{
			String user_agent = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
			bool result = false;
			foreach (String key in "iPhone,iPod,Android,ios".Split(','))
			{
				if (user_agent.Contains(key))
				{
					result = true;
					if (domain != null)
					{
						System.Web.HttpContext.Current.Response.Redirect(domain);
					}
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// 昵称验证
		/// </summary>
		/// <param name="obj">字符串</param>
		public static bool IsNickName(object obj)
		{
			return IsMatch(obj + "", "^[\u4E00-\u9FA5a-zA-Z0-9]+$");
		}

		/// <summary>
		/// 数字验证
		/// </summary>
		/// <param name="obj">字符串</param>
		public static bool IsNumeric(object obj)
		{
			return IsMatch(obj + "", "^\\d{1,10}$");
		}

		/// <summary>
		/// 是不是POST请求 
		/// </summary>
		/// <returns></returns>
		public static bool IsPost() { return System.Web.HttpContext.Current.Request.HttpMethod == "POST"; }

		/// <summary>
		/// 用户名验证，默认4-18位字母开头且和数字的字符串
		/// </summary>
		/// <param name="s">待验证字符串</param>
		/// <param name="a">最低长度</param>
		/// <param name="b">最大长度</param>
		/// <returns></returns>
		public static bool IsUserName(object obj, int a = 4, int b = 18)
		{
			return IsMatch(obj, "^[a-zA-Z][a-zA-Z0-9]{" + (a - 1) + "," + (b - 1) + "}$");
		}

		/// <summary>
		/// 把各种对象转为JSON字符串
		/// </summary>
		/// <param name="obj">对象</param>
		/// <param name="object">时间格式，默认不转换，数字转为时间戳，字符串为转后的时间格式</param>
		/// <returns></returns>
		public static string Json(object obj, object dateTimeFormat = null)
		{
			string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
			if (dateTimeFormat != null)
			{
				json = System.Text.RegularExpressions.Regex.Replace(json, "\"" + @"\\/Date\((\d+)\)\\/" + "\"", match =>
				{
					if (dateTimeFormat.GetType().ToString() == "System.Int32") { return match.Groups[1].Value.Substring(0, match.Groups[1].Value.Length - 3); }
					DateTime dt = new DateTime(1970, 1, 1).AddMilliseconds(long.Parse(match.Groups[1].Value)).ToLocalTime();
					return "\"" + dt.ToString(dateTimeFormat.ToString()) + "\"";
				});
			}
			json = json.Replace("\r", "").Replace("\n", "").Replace("\t", "");
			return json;
		}

		/// <summary>
		/// JSON字符串转为DICT对象
		/// </summary>
		/// <param name="strJson">JSON字符串</param>
		/// <returns></returns>
		public static dynamic JsonToDict(string strJson)
		{
			dynamic result = null;
			try
			{
				result = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<dynamic>(strJson);
			}
			finally { }
			return result;
		}

		/// <summary>
		/// 地图上两点距离
		/// </summary>
		/// <param name="lngA">A点经度</param>
		/// <param name="latA">A点纬度</param>
		/// <param name="lngB">B点经度</param>
		/// <param name="latB">B点纬度</param>
		/// <param name="radius">地球半径，默认为百度的</param>
		/// <returns></returns>
		public static string MapDistance(object lngA, object latA, object lngB, object latB, double radius = 6370996.81)
		{
			double lng0 = 0; Double.TryParse(lngA + "", out lng0);
			double lat0 = 0; Double.TryParse(latA + "", out lat0);
			double lng1 = 0; Double.TryParse(lngB + "", out lng1);
			double lat1 = 0; Double.TryParse(latB + "", out lat1);
			//radius = 6378136.49;腾讯地图半径
			double temp = 0; string result = "";

			if (lng0 > 0 && lat0 > 0)
			{
				temp = 180 / Math.PI; lng0 = lng0 / temp; lat0 = lat0 / temp; lng1 = lng1 / temp; lat1 = lat1 / temp;
				temp = Math.Cos(lat0) * Math.Cos(lng0) * Math.Cos(lat1) * Math.Cos(lng1);
				temp += Math.Cos(lat0) * Math.Sin(lng0) * Math.Cos(lat1) * Math.Sin(lng1);
				temp += Math.Sin(lat0) * Math.Sin(lat1);
				temp = radius * Math.Acos(temp);//乘以地球半径，这是百度地图采用的值
				result = temp + "";
			}
			else
			{
				result = "COS(" + latA + "/180*PI())*COS(" + lngA + "/180*PI())*COS(" + latB + "/180*PI())*COS(" + lngB + "/180*PI())";
				result += "+COS(" + latA + "/180*PI())*SIN(" + lngA + "/180*PI())*COS(" + latB + "/180*PI())*SIN(" + lngB + "/180*PI())";
				result += "+SIN(" + latA + "/180*PI())*SIN(" + latB + "/180*PI())";
				result = radius + "*ACOS(" + result + ")";
			}
			return result;
		}

		/// <summary>
		/// 文件路径
		/// </summary>
		/// <param name="path">文件的相对路径</param>
		/// <returns></returns>
		public static string MapPath(string path) { return System.Web.HttpContext.Current.Server.MapPath(path); }

		/// <summary>
		/// MD5
		/// </summary>
		/// <param name="obj">要加密的明文</param>
		/// <param name="charset">字符编码</param>
		/// <param name="key">密码</param>
		/// <returns></returns>
		public static string MD5(object obj, string charset = null, object key = null)
		{
			byte[] bytes;
			charset = charset + ""; if (charset.Length == 0) { charset = "UTF-8"; }
			if (key == null)
			{
				System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
				bytes = md5.ComputeHash(System.Text.Encoding.GetEncoding(charset).GetBytes(obj + ""));
			}
			else
			{
				bytes = System.Text.Encoding.GetEncoding(charset).GetBytes(key + "");
				System.Security.Cryptography.HMACMD5 hmacMD5 = new System.Security.Cryptography.HMACMD5(bytes);
				bytes = hmacMD5.ComputeHash(System.Text.Encoding.GetEncoding(charset).GetBytes(obj + ""));
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			foreach (byte b in bytes) { stringBuilder.Append(b.ToString("x2")); }
			return stringBuilder.ToString();
		}

		/// <summary>
		/// POST值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <returns></returns>
		public static string Post(string key = null)
		{
			return key == null ? System.Web.HttpContext.Current.Request.Form.ToString() : System.Web.HttpContext.Current.Request.Form[key];
		}

		/// <summary>
		/// POST整数值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public static int PostID(string key = "id", double defaultID = 0)
		{
			Double.TryParse(System.Web.HttpContext.Current.Request.Form[key] + "", out defaultID);
			return (int)Math.Floor(defaultID);
		}

		/// <summary>
		/// POST同参名的值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <returns></returns>
		public static string[] Posts(string key)
		{
			return System.Web.HttpContext.Current.Request.Form.GetValues(key);
		}

		/// <summary>
		/// 随机种子
		/// </summary>
		private static Random _random = new Random((int)DateTime.Now.Ticks);

		/// <summary>
		/// 随机数
		/// </summary>
		/// <param name="min">最小值</param>
		/// <param name="max">最大值</param>
		/// <param name="t">过期秒数</param>
		/// <returns></returns>
		public static int Random(int min, int max)
		{
			return _random.Next(min, max);
		}

		/// <summary>
		/// 请求URL：重写后的相对根路径，不带域名，带参数
		/// </summary>
		/// <returns></returns>
		public static string RawUrl() { return System.Web.HttpContext.Current.Request.RawUrl; }

		/// <summary>
		/// 转向
		/// </summary>
		/// <param name="url">页面地址，默认为来源页，空字符串为首页</param>
		public static void Redirect(string url = null)
		{
			url += "";
			if (url.Length == 0) { url = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]; }
			if (url.Length == 0) { url = "/"; }
			if (url.StartsWith("?")) { url = System.Web.HttpContext.Current.Request.Url.AbsolutePath + url; }
			System.Web.HttpContext.Current.Response.Redirect(url);
		}

		/// <summary>
		/// 来源页
		/// </summary>
		/// <param name="defaultUrl">备用地址，当来源页不存在时启用</param>
		/// <returns></returns>
		public static string Referer(string defaultUrl = "/", string key = "referer")
		{
			string url = System.Web.HttpContext.Current.Request.QueryString[key] + "";
			if (url.Length == 0) { url = System.Web.HttpContext.Current.Request.Form[key] + ""; }
			if (url.Length == 0) { url = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] + ""; }
			if (url.Length == 0) { url = defaultUrl; }
			return url;
		}

		/// <summary>
		/// 字符串替换
		/// </summary>
		/// <param name="obj0">要做替换的字符串</param>
		/// <param name="obj1">被替换掉的字符串</param>
		/// <param name="obj2">被替换后的字符串</param>
		/// <returns></returns>
		public static string Replace(Object obj0, Object obj1, Object obj2)
		{

			return System.Text.RegularExpressions.Regex.Replace(obj0 + "", obj1 + "", obj2 + "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

		}

		/// <summary>
		/// Request整数值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <returns></returns>
		public static int RequestID(string key = "id", double id = 0)
		{
			string result = System.Web.HttpContext.Current.Request.QueryString[key];
			if (result == null) { result = System.Web.HttpContext.Current.Request.Form[key]; }
			Double.TryParse(result + "", out id);
			return (int)Math.Floor(id);
		}

		/// <summary>
		/// Request同参名的值
		/// </summary>
		/// <param name="key">参数名</param>
		/// <returns></returns>
		public static string[] Requests(string key)
		{
			string[] result = System.Web.HttpContext.Current.Request.QueryString.GetValues(key);
			if (result == null) { result = System.Web.HttpContext.Current.Request.Form.GetValues(key); }
			return result;
		}

		/// <summary>
		/// 服务器变量
		/// </summary>
		/// <param name="keyName">为空，输出全部信息</param>
		/// <returns></returns>
		public static string ServerVariables(string keyName = null)
		{
			System.Collections.Specialized.NameValueCollection serverVariables = System.Web.HttpContext.Current.Request.ServerVariables;
			string result = "";
			if (keyName == null)
			{
				foreach (string key in serverVariables) { result += "【" + key + "】：" + serverVariables[key] + "<br />"; };
				System.Web.HttpContext.Current.Response.Clear();
				System.Web.HttpContext.Current.Response.Write(result);
				System.Web.HttpContext.Current.Response.End();

			}
			else
			{
				result = serverVariables[keyName];
			}
			return result;
		}

		public static string ServerVersion(string type = null)
		{
			type = (type + "").ToUpper();
			string result = "";
			if (type == "FRAMEWORK" || type.Length == 0)
			{
				result = Environment.Version.Major + "." + Environment.Version.Minor;
				result += "." + Environment.Version.Build + "." + Environment.Version.Revision;
			}
			if (type == "OS") { result = Environment.OSVersion + ""; }
			return result;
		}

		/// <summary>
		/// SESSION操作
		/// </summary>
		/// <param name="key">SESSION名</param>
		/// <param name="obj">SESSION值</param>
		/// <returns></returns>
		public static string Session(string key, object obj = null)
		{
			String result = "";
			if (obj == null)
			{
				result = System.Web.HttpContext.Current.Session[key] + "";
			}
			else
			{
				result = obj + "";
				System.Web.HttpContext.Current.Session[key] = result;
			}
			return result;
		}

		/// <summary>
		/// SHA1模式加密
		/// </summary>
		/// <param name="str">待加密文</param>
		/// <param name="charset">编码</param>
		/// <param name="key">密码</param>
		/// <returns></returns>
		public static string SHA1(object str, string charset = null, object key = null)
		{
			byte[] bytes;
			charset = charset + ""; if (charset.Length == 0) { charset = "UTF-8"; }
			if (key == null)
			{
				System.Security.Cryptography.SHA1CryptoServiceProvider sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
				bytes = sha1.ComputeHash(System.Text.Encoding.GetEncoding(charset).GetBytes(str + ""));
			}
			else
			{
				bytes = System.Text.Encoding.GetEncoding(charset).GetBytes(key + "");
				System.Security.Cryptography.HMACSHA1 hmacSHA1 = new System.Security.Cryptography.HMACSHA1(bytes);
				bytes = hmacSHA1.ComputeHash(System.Text.Encoding.GetEncoding(charset).GetBytes(str + ""));
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder("");
			foreach (byte b in bytes) { stringBuilder.Append(b.ToString("x2")); }
			return stringBuilder.ToString();
		}

		/// <summary>
		/// 【方法名】：签名、验签请求数据<para />
		/// 【data】：待签名数据，null表示验证签名<para />
		/// 【signType】：签名方式，默认MD5，可选SHA1<para />
		/// 【signCharset】：编码格式，默认UTF-8<para />
		/// 【signKey】：签名密钥，如不指定，则从web.config中获取SH.Tool.SignKey的值（如果设置了话）<para />
		/// 【signHmaxKey】：HMAC算法扰乱码，如不指定，则从web.config中获取SH.Tool.SignHmaxKey的值（如果设置了话）<para />
		/// 【timeout】：验签超时秒数，默认5<para />
		/// 【返回值】：验签失败返回null，其余均返回签名结果<para />
		/// </summary>
		public static string Sign(object data = null, string signType = "", string signCharset = "", string signKey = "", string signHmaxKey = "", int timeout = 5)
		{
			string signResult = ""; string signData = data + "";
			if (data == null)
			{
				System.Collections.ArrayList arrayList = new System.Collections.ArrayList();
				System.Collections.Specialized.NameValueCollection request = System.Web.HttpContext.Current.Request.QueryString;
				if (request.Count == 0) { request = System.Web.HttpContext.Current.Request.Form; }
				int signTimestamp = 0; Int32.TryParse(request["signTimestamp"] + "", out signTimestamp);
				if (signTimestamp > 0)
				{
					int timestamp = (int)(System.DateTime.Now - System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1))).TotalSeconds;
					timestamp = timestamp - signTimestamp;
					if (timestamp > timeout)
					{
						System.Web.HttpContext.Current.Response.Clear();
						System.Web.HttpContext.Current.Response.Write("{\"ret\":1,\"message\":\"timestamp超时\"}");
						System.Web.HttpContext.Current.Response.End();
					}
				}
				signResult = request["signResult"] + "";
				signType = request["signType"] + "";
				signCharset = request["signCharset"] + "";
				arrayList.Sort();
				foreach (String key in request) { if (key.ToLower() != "signresult") { signData += "&" + key + "=" + request[key]; } }
				signData = signData.TrimStart('&');
			}
			signType = signType.ToUpper(); if (signType.Length == 0) { signType = "MD5"; }
			signCharset = signCharset.ToUpper(); if (signCharset.Length == 0) { signCharset = "UTF-8"; }

			if (signHmaxKey.Length == 0) { signHmaxKey = System.Configuration.ConfigurationManager.AppSettings["SH.Tool.SignHmaxKey"] + ""; }

			if (signKey.Length == 0) { signKey = System.Configuration.ConfigurationManager.AppSettings["SH.Tool.SignKey"] + ""; }
			if (signKey.Length == 0) { signKey = "123456"; }

			signData = signData + "&signKey=" + signKey;
			byte[] bytes;
			if (signType == "SHA1")
			{
				if (signHmaxKey.Length == 0)
				{
					System.Security.Cryptography.SHA1CryptoServiceProvider sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
					bytes = sha1.ComputeHash(System.Text.Encoding.GetEncoding(signCharset).GetBytes(signData));
				}
				else
				{
					bytes = System.Text.Encoding.GetEncoding(signCharset).GetBytes(signHmaxKey + "");
					System.Security.Cryptography.HMACSHA1 hmacSHA1 = new System.Security.Cryptography.HMACSHA1(bytes);
					bytes = hmacSHA1.ComputeHash(System.Text.Encoding.GetEncoding(signCharset).GetBytes(signData));
				}
			}
			else
			{
				if (signHmaxKey.Length == 0)
				{
					System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
					bytes = md5.ComputeHash(System.Text.Encoding.GetEncoding(signCharset).GetBytes(signData));
				}
				else
				{
					bytes = System.Text.Encoding.GetEncoding(signCharset).GetBytes(signHmaxKey + "");
					System.Security.Cryptography.HMACMD5 hmacMD5 = new System.Security.Cryptography.HMACMD5(bytes);
					bytes = hmacMD5.ComputeHash(System.Text.Encoding.GetEncoding(signCharset).GetBytes(signData));
				}
			}

			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder("");
			foreach (byte b in bytes) { stringBuilder.Append(b.ToString("x2")); }
			string signature = stringBuilder.ToString();
			//signResult不为空，则代表有传入值，即要验证，
			if (signResult.Length > 0 && signResult != signature) { signature = null; }
			return signature;
		}

		/// <summary>
		/// RSA：公钥加密，私钥解密，私钥签名，公钥验签
		/// </summary>
		/// <param name="obj">密钥文件或密钥文本</param>
		/// <param name="password">密码</param>
		/// <param name="rsaType">返回类型，如同时存在私钥和公钥，则指定返回类型，0私钥，1公钥</param>
		/// <returns></returns>
		public static System.Security.Cryptography.RSACryptoServiceProvider RSA(string obj, object password = null, int rsaType = 0)
		{
			if (obj == null)
			{
				string help = "一般来说：公钥加密，私钥解密，私钥签名，公钥验签，以实现只有本人能接收加密过的消息和发布带签名的消息，反之是没有意义的";
				help += "<br />Convert.ToBase64String(publicRSA.Encrypt(System.Text.Encoding.GetEncoding(charset).GetBytes(content), false));";
				help += "<br />System.Text.Encoding.GetEncoding(charset).GetString(privateRSA.Decrypt(Convert.FromBase64String(content), false));";
				help += "<br />Convert.ToBase64String(privateRSA.SignData(System.Text.Encoding.GetEncoding(charset).GetBytes(content), signType));";
				help += "<br />publicRSA.VerifyData(System.Text.Encoding.GetEncoding(charset).GetBytes(content), signType, Convert.FromBase64String(signData));";
			}

			string filePath = (obj + "").ToLower(); string content = null;
			if (filePath.EndsWith(".xml"))
			{
				if (filePath.Contains(":") == false) { filePath = System.Web.HttpContext.Current.Server.MapPath(filePath); }
				content = System.IO.File.ReadAllText(filePath, System.Text.Encoding.ASCII);
				System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
				rsa.FromXmlString(content);
				return rsa;
			}
			else if (filePath.EndsWith(".pfx") || filePath.EndsWith(".p12"))
			{
				if (filePath.Contains(":") == false) { filePath = System.Web.HttpContext.Current.Server.MapPath(filePath); }
				System.Security.Cryptography.X509Certificates.X509Certificate2 rsa = null;
				if ((password + "").Length == 0)
				{
					rsa = new System.Security.Cryptography.X509Certificates.X509Certificate2(filePath);
				}
				else
				{
					rsa = new System.Security.Cryptography.X509Certificates.X509Certificate2(filePath, password + "", System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
				}

				if (rsaType < 1)
				{
					return (System.Security.Cryptography.RSACryptoServiceProvider)rsa.PrivateKey;
				}
				else
				{
					return (System.Security.Cryptography.RSACryptoServiceProvider)rsa.PublicKey.Key;
				}
			}
			else if (filePath.EndsWith(".cer"))
			{
				if (filePath.Contains(":") == false) { filePath = System.Web.HttpContext.Current.Server.MapPath(filePath); }
				System.Security.Cryptography.X509Certificates.X509Certificate2 rsa = new System.Security.Cryptography.X509Certificates.X509Certificate2(filePath);
				return (System.Security.Cryptography.RSACryptoServiceProvider)rsa.PublicKey.Key;
			}
			else if (filePath.EndsWith(".pem") || filePath.EndsWith(".txt"))
			{
				if (filePath.Contains(":") == false) { filePath = System.Web.HttpContext.Current.Server.MapPath(filePath); }
				content = System.IO.File.ReadAllText(filePath, System.Text.Encoding.ASCII);
				content = content.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\r", "");
				content = content.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "");
			}
			else
			{
				content = obj + "";
			}

			if (content.Length > 0)
			{
				byte[] keyByte = Convert.FromBase64String(content);
				System.Security.Cryptography.RSAParameters param = new System.Security.Cryptography.RSAParameters();
				if (keyByte.Length == 162)
				{
					byte[] modulus = new byte[128]; Array.Copy(keyByte, 29, modulus, 0, 128); param.Modulus = modulus;
					byte[] exponent = new byte[3]; Array.Copy(keyByte, 159, exponent, 0, 3); param.Exponent = exponent;
					rsaType = 1;
				}
				else if (keyByte.Length == 294)
				{
					byte[] modulus = new byte[256]; Array.Copy(keyByte, 33, modulus, 0, 256); param.Modulus = modulus;
					byte[] exponent = new byte[3]; Array.Copy(keyByte, 291, exponent, 0, 3); param.Exponent = exponent;
					rsaType = 1;
				}
				else if (keyByte.Length > 600 && keyByte.Length < 620)
				{
					int i = 11;
					byte[] modulus = new byte[128]; Array.Copy(keyByte, i, modulus, 0, modulus.Length); param.Modulus = modulus;

					i += modulus.Length; i += 2;
					byte[] exponent = new byte[3]; Array.Copy(keyByte, i, exponent, 0, 3); param.Exponent = exponent;

					i += 3; i += 4; if ((int)keyByte[i] == 0) { i++; }
					byte[] d = new byte[128]; Array.Copy(keyByte, i, d, 0, d.Length); param.D = d;

					i += d.Length; i += ((int)keyByte[i + 1] == 64 ? 2 : 3);
					byte[] p = new byte[64]; Array.Copy(keyByte, i, p, 0, p.Length); param.P = p;

					i += p.Length; i += ((int)keyByte[i + 1] == 64 ? 2 : 3);
					byte[] q = new byte[64]; Array.Copy(keyByte, i, q, 0, q.Length); param.Q = q;

					i += q.Length; i += ((int)keyByte[i + 1] == 64 ? 2 : 3);
					byte[] dp = new byte[64]; Array.Copy(keyByte, i, dp, 0, dp.Length); param.DP = dp;

					i += dp.Length; i += ((int)keyByte[i + 1] == 64 ? 2 : 3);
					byte[] dq = new byte[64]; Array.Copy(keyByte, i, dq, 0, dq.Length); param.DQ = dq;

					i += dq.Length; i += ((int)keyByte[i + 1] == 64 ? 2 : 3);
					byte[] inverseQ = new byte[64]; Array.Copy(keyByte, i, inverseQ, 0, inverseQ.Length); param.InverseQ = inverseQ;
				}
				else if (keyByte.Length > 1100)
				{
					Debug(keyByte.Length);
					int i = 12;
					byte[] modulus = new byte[256]; Array.Copy(keyByte, i, modulus, 0, modulus.Length); param.Modulus = modulus;

					i += modulus.Length; i += 2;
					byte[] exponent = new byte[3]; Array.Copy(keyByte, i, exponent, 0, 3); param.Exponent = exponent;

					i += 3; i += 4; if ((int)keyByte[i] == 0) { i++; }
					byte[] d = new byte[256]; Array.Copy(keyByte, i, d, 0, d.Length); param.D = d;

					i += d.Length; i += ((int)keyByte[i + 2] == 128 ? 3 : 4);
					byte[] p = new byte[128]; Array.Copy(keyByte, i, p, 0, p.Length); param.P = p;

					i += p.Length; i += ((int)keyByte[i + 2] == 128 ? 3 : 4);
					byte[] q = new byte[128]; Array.Copy(keyByte, i, q, 0, q.Length); param.Q = q;

					i += q.Length; i += ((int)keyByte[i + 2] == 128 ? 3 : 4);
					byte[] dp = new byte[128]; Array.Copy(keyByte, i, dp, 0, dp.Length); param.DP = dp;

					i += dp.Length; i += ((int)keyByte[i + 2] == 128 ? 3 : 4);
					byte[] dq = new byte[128]; Array.Copy(keyByte, i, dq, 0, dq.Length); param.DQ = dq;

					i += dq.Length; i += ((int)keyByte[i + 2] == 128 ? 3 : 4);
					byte[] inverseQ = new byte[128]; Array.Copy(keyByte, i, inverseQ, 0, inverseQ.Length); param.InverseQ = inverseQ;
				}
				else
				{
					System.Web.HttpContext.Current.Response.Clear();
					System.Web.HttpContext.Current.Response.Write("RSA ERROR,keyByte.Length:" + keyByte.Length);
					System.Web.HttpContext.Current.Response.End();
				}
				System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
				rsa.ImportParameters(param);
				return rsa;
			}
			return null;
		}

		public static string RSAEncrypt(System.Security.Cryptography.RSACryptoServiceProvider publicRSA, string content, string charset = "UTF-8")
		{
			string result = ""; int keySize = publicRSA.KeySize / 8; int maxSize = keySize - 11;
			using (System.IO.MemoryStream source = new System.IO.MemoryStream(System.Text.Encoding.GetEncoding(charset).GetBytes(content)))
			using (System.IO.MemoryStream target = new System.IO.MemoryStream())
			{
				Byte[] buffer = new Byte[maxSize]; int size = source.Read(buffer, 0, maxSize);
				while (size > 0)
				{
					Byte[] temp = new Byte[size]; Array.Copy(buffer, 0, temp, 0, size);
					Byte[] encrypt = publicRSA.Encrypt(temp, false); target.Write(encrypt, 0, encrypt.Length);
					size = source.Read(buffer, 0, maxSize);
				}
				result = Convert.ToBase64String(target.ToArray(), Base64FormattingOptions.None);
			}
			return result;
		}

		/// <summary>
		/// 私钥解密
		/// </summary>
		/// <param name="content">待解密密文</param>
		/// <returns></returns>
		public static string RSADecrypt(System.Security.Cryptography.RSACryptoServiceProvider privateRSA, string content, string charset = "UTF-8")
		{
			string result = ""; int keySize = privateRSA.KeySize / 8;
			using (System.IO.MemoryStream source = new System.IO.MemoryStream(Convert.FromBase64String(content)))
			using (System.IO.MemoryStream target = new System.IO.MemoryStream())
			{
				Byte[] buffer = new Byte[keySize]; int size = source.Read(buffer, 0, keySize);
				while (size > 0)
				{
					Byte[] temp = new Byte[size]; Array.Copy(buffer, 0, temp, 0, size);
					Byte[] decrypt = privateRSA.Decrypt(temp, false); target.Write(decrypt, 0, decrypt.Length);
					size = source.Read(buffer, 0, keySize);
				}
				result = System.Text.Encoding.GetEncoding(charset).GetString(target.ToArray());
			}
			return result;
		}

		/// <summary>
		/// 私钥签名
		/// </summary>
		/// <param name="rsa">RSA对象</param>
		/// <param name="content">待签数据</param>
		/// <param name="signType">签名方式</param>
		/// <param name="charset">编码</param>
		/// <returns></returns>
		public static string RSASign(System.Security.Cryptography.RSACryptoServiceProvider privateRSA, string content, string signType = "SHA1", string charset = "UTF-8")
		{
			return Convert.ToBase64String(privateRSA.SignData(System.Text.Encoding.GetEncoding(charset).GetBytes(content), signType));
		}

		/// <summary>
		/// 公钥验签
		/// </summary>
		/// <param name="rsa">RSA对象</param>
		/// <param name="content">待签数据</param>
		/// <param name="signData">签名结果</param>
		/// <param name="signType">签名方式</param>
		/// <param name="charset">编码</param>
		/// <returns></returns>
		public static bool RSAVerify(System.Security.Cryptography.RSACryptoServiceProvider publicRSA, string content, string signData, string signType = "SHA1", string charset = "UTF-8")
		{
			return publicRSA.VerifyData(System.Text.Encoding.GetEncoding(charset).GetBytes(content), signType, Convert.FromBase64String(signData));
		}

		/// <summary>
		/// 站点地址，带协议、域名、端口，不带文件路径及参数
		/// </summary>
		/// <returns></returns>
		public string SiteUrl()
		{
			string url = System.Web.HttpContext.Current.Request.Url.ToString();
			return url.Substring(0, url.IndexOf('/', 10));
		}

		/// <summary>
		/// 字符串分割
		/// </summary>
		/// <param name="strAll">要被分割的字符串</param>
		/// <param name="strChar">用来分割的字符串</param>
		/// <returns></returns>
		public static string[] Split(string strAll, string strChar)
		{
			return (new System.Text.RegularExpressions.Regex(strChar)).Split(strAll);
		}

		/// <summary>
		/// 缩略图
		/// </summary>
		/// <param name="filePath">源图路径</param>
		/// <param name="width">最大宽度</param>
		/// <param name="height">最大高度</param>
		/// <param name="fileName">输出文件名，仅限jpg,png,gif，否则返回null</param>
		/// <returns></returns>
		public string Thumb(string filePath, double width = 0, double height = 0, string fileName = null)
		{
			filePath = filePath + "";
			if (filePath.Contains(":") == false) { filePath = System.Web.HttpContext.Current.Server.MapPath(filePath); }
			System.Drawing.Image image = System.Drawing.Image.FromFile(filePath, true);

			double sourceWidth = image.Width; double targetWidth = sourceWidth; width = Math.Abs(width);
			double sourceHeight = image.Height; double targetHeight = sourceHeight; height = Math.Abs(height);

			if (targetWidth > width && width > 0)//太宽
			{
				targetHeight = targetHeight * (width / targetWidth);
				targetWidth = width;
			}

			if (targetHeight > height && height > 0)//太高
			{
				targetWidth = targetWidth * (height / targetHeight);
				targetHeight = height;
			}
			if (width == 0) { width = targetWidth; }
			if (height == 0) { height = targetHeight; }

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)width, (int)height);
			System.Drawing.Graphics fromImage = System.Drawing.Graphics.FromImage(bitmap);
			fromImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			fromImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
			fromImage.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			fromImage.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
			fromImage.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			fromImage.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
			System.Drawing.Rectangle source = new System.Drawing.Rectangle(0, 0, (int)sourceWidth, (int)sourceHeight);
			System.Drawing.Rectangle target = new System.Drawing.Rectangle((int)((width - targetWidth) / 2), (int)((height - targetHeight) / 2), (int)targetWidth, (int)targetHeight);
			fromImage.DrawImage(image, target, source, System.Drawing.GraphicsUnit.Pixel);
			System.Drawing.Image newImage = (System.Drawing.Image)bitmap.Clone();
			bitmap.Dispose(); image.Dispose();


			if (fileName != null) { filePath = fileName; }
			if (filePath.Contains(":") == false) { filePath = System.Web.HttpContext.Current.Server.MapPath(filePath); }
			fileName = filePath.Replace(System.Web.HttpContext.Current.Server.MapPath("/"), "/").Replace("\\", "/");

			string path = "";
			foreach (string v in fileName.Substring(0, fileName.LastIndexOf("/")).Split('/'))
			{
				path += v + "/";
				if (path.Length > 1 && v.Length > 0)
				{
					string realPath = System.Web.HttpContext.Current.Server.MapPath(path);
					if (!System.IO.Directory.Exists(realPath)) { System.IO.Directory.CreateDirectory(realPath); }
				}
			}

			switch (fileName.ToLower().Substring(fileName.LastIndexOf('.')))
			{
				case ".jpg":
					newImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg); break;
				case ".png":
					newImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png); break;
				case ".gif":
					newImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Gif); break;
				default:
					fileName = null; break;
			}
			return fileName;
		}

		/// <summary>
		/// 时间戳
		/// </summary>
		/// <returns></returns>
		public static int TimeStamp()
		{
			return (int)(System.DateTime.Now - System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1))).TotalSeconds;
		}

		/// <summary>
		/// 时间转时间戳
		/// </summary>
		/// <param name="dateTime">时间</param>
		/// <returns></returns>
		public static int TimeStamp(DateTime dateTime)
		{
			return (int)(dateTime - System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1))).TotalSeconds;
		}

		/// <summary>
		/// 字符串转时间戳
		/// </summary>
		/// <param name="dateTime">时间</param>
		/// <returns></returns>
		public static int TimeStamp(string dateTime)
		{
			if (dateTime == null) { dateTime = System.DateTime.Now.ToString(); }
			System.DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
			System.DateTime endTime = startTime;
			System.DateTime outTime; if (DateTime.TryParse(dateTime, out outTime)) { endTime = outTime; }
			return (int)(endTime - startTime).TotalSeconds;
		}

		/// <summary>
		/// 时间戳转时间
		/// </summary>
		/// <param name="seconds">秒数</param>
		/// <returns></returns>
		public static DateTime TimeStamp(int seconds)
		{
			return System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)).Add(new TimeSpan(long.Parse(seconds + "0000000")));
		}

		/// <summary>
		/// 读写TXT文本文件
		/// </summary>
		/// <param name="filename">文件名</param>
		/// <param name="content">文本内容</param>
		/// <returns></returns>
		public static string Txt(string filename, object content = null)
		{
			if (!filename.ToLower().EndsWith(".txt"))
			{
				content = filename;
				filename = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
			}
			if (content != null)
			{
				System.IO.File.AppendAllText(System.Web.HttpContext.Current.Server.MapPath(filename), content + "", System.Text.Encoding.UTF8);
			}
			else
			{
				content = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath(filename));
			}
			return content + "";
		}

		/// <summary>
		/// 输出
		/// </summary>
		/// <param name="obj">要输出的对象</param>
		public static void Write(object obj = null)
		{
			System.Web.HttpContext.Current.Response.Write(obj);
		}

		/// <summary>
		///  上传
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static System.Collections.ArrayList Upload(string field = "file", string dir = "/Upload/{yyyy}/{MM}/{dd}", int max = 0, string ext = "media,image,file")
		{
			System.Collections.ArrayList list = new System.Collections.ArrayList();
			if (System.Web.HttpContext.Current.Request.HttpMethod != "POST") { return list; }

			int ret = 0; string message = "";
			System.Web.HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;
			if (files.Count < 1)
			{
				ret = 1; message = "请选择上传文件";
			}
			else
			{
				max = max * 1024 * 1024;

				ext = ext.Replace(".", "").Trim(',');
				ext = ext.Replace("media", "audio,video,flash");
				ext = ext.Replace("audio", "mp3");
				ext = ext.Replace("video", "mp4");
				ext = ext.Replace("flash", "swf,flv");
				ext = ext.Replace("image", "gif,jpg,png,bmp");
				ext = ext.Replace("file", "doc,docx,xls,xlsx,ppt,pptx,zip,rar");
				ext = "," + ext + ',';

				for (int i = 0; i < files.Count; i++)
				{
					if (files.AllKeys[i].Length == 0 || files.AllKeys[i].ToLower() != field.ToLower()) { continue; }
					System.Web.HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[i];
					if (file.InputStream.Length == 0) { ret = 1; message = "无法上传空文件"; break; }

					if (file.InputStream.Length > max && max > 0) { ret = 1; message = "文件太大(" + max + ")"; break; }

					string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower() + "";
					if (fileExt.Length == 0 || ext.Contains("," + fileExt.Substring(1) + ",") == false)
					{
						ret = 1; message = "文件类型错误(" + fileExt + ")"; break;
					}
					dir = dir.Replace("{yyyy}", DateTime.Now.ToString("yyyy"));
					dir = dir.Replace("{MM}", DateTime.Now.ToString("MM"));
					dir = dir.Replace("{dd}", DateTime.Now.ToString("dd"));
					dir = dir.TrimEnd('/');
					string path = "";
					foreach (string v in dir.Split('/'))
					{
						path += v + "/";
						if (path.Length > 1 && v.Length > 0)
						{
							string realPath = System.Web.HttpContext.Current.Server.MapPath(path);
							if (!System.IO.Directory.Exists(realPath)) { System.IO.Directory.CreateDirectory(realPath); }
						}
					}
					string url = dir + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + i + "_" + DateTime.Now.ToString("fff") + fileExt;
					list.Add(url);
					file.SaveAs(System.Web.HttpContext.Current.Server.MapPath(url));
				}
				if (ret > 0)
				{
					foreach (string file in list) { System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath(file)); }
					list.Clear();
					System.Web.HttpContext.Current.Response.Clear();
					System.Web.HttpContext.Current.Response.Write("{\"ret\":" + ret + ",\"message\":\"" + message + "\"}");
					System.Web.HttpContext.Current.Response.End();
				}
			}
			return list;
		}

		/// <summary>
		/// 绝对URL：真实的执行文件路径，带域名，不带参数
		/// </summary>
		/// <param name="url">地址，为空则为当前页</param>
		/// <returns></returns>
		public static string Uri(string url = null)
		{
			if (url == null) { url = System.Web.HttpContext.Current.Request.Url.ToString(); }
			url += "?"; url = url.Substring(0, url.IndexOf("?"));
			return url;
		}

		/// <summary>
		/// 当前URL：真实的全路径，带域名和参数
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string Url() { return System.Web.HttpContext.Current.Request.Url.ToString(); }

		/// <summary>
		/// Url解码
		/// </summary>
		/// <param name="url">Url地址</param>
		/// <param name="charset">编码格式</param>
		/// <returns></returns>
		public static string UrlDecode(object url, string charset = "UTF-8") { return System.Web.HttpUtility.UrlDecode(url + "", System.Text.Encoding.GetEncoding(charset)); }

		/// <summary>
		/// Url编码
		/// </summary>
		/// <param name="url">地址，为空则为当前页</param>
		/// <param name="charset">编码格式</param>
		/// <returns></returns>
		public static string UrlEncode(object url = null, string charset = "UTF-8")
		{
			if (url == null) { url = System.Web.HttpContext.Current.Request.Url.ToString(); }
			return System.Web.HttpUtility.UrlEncode(url + "", System.Text.Encoding.GetEncoding(charset));
		}

		/// <summary>
		/// 验证码
		/// </summary>
		/// <param name="length">验证码长度</param>
		/// <returns></returns>
		public static string VerifyCode(string sessionName = "VerifyCode", int length = 4)
		{
			string[] chars = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,T,U,W,X,Y,Z".Split(',');
			string code = ""; for (int i = 0; i < length; i++) { code += chars[Random(0, chars.Length - 1)]; }
			System.Web.HttpContext.Current.Session[sessionName] = code;
			return code;
		}

		/// <summary>
		/// 验证码图片
		/// </summary>
		/// <param name="sessionName"></param>
		/// <param name="length"></param>
		public static string VerifyImage(string sessionName = "VerifyCode", int length = 4)
		{
			System.Web.HttpContext.Current.Response.Buffer = true;
			System.Web.HttpContext.Current.Response.ExpiresAbsolute = DateTime.Now.AddSeconds(-1);
			System.Web.HttpContext.Current.Response.Expires = 0;
			System.Web.HttpContext.Current.Response.CacheControl = "no-cache";
			System.Web.HttpContext.Current.Response.AppendHeader("Pragma", "No-Cache");

			string code = VerifyCode(sessionName, length);
			System.Drawing.Bitmap image = new System.Drawing.Bitmap(code.Length * 14 + 4, 20);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image);
			System.Drawing.Font font = new System.Drawing.Font("Arial ", 10);//, System.Drawing.FontStyle.Bold);
			System.Drawing.Brush black = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
			System.Drawing.Brush red = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(166, 8, 8));

			graphics.Clear(System.Drawing.ColorTranslator.FromHtml("#99C1CB"));//背景色

			char[] ch = code.ToCharArray();
			for (int i = 0; i < ch.Length; i++)
			{
				graphics.DrawString(ch[i].ToString(), font, ch[i] >= 0 && ch[i] <= 9 ? red : black, 3 + (i * 14), 3);
			}
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
			System.Web.HttpContext.Current.Response.Cache.SetNoStore();
			System.Web.HttpContext.Current.Response.ClearContent();
			System.Web.HttpContext.Current.Response.ContentType = "image/Jpeg";
			System.Web.HttpContext.Current.Response.BinaryWrite(ms.ToArray());
			graphics.Dispose();
			image.Dispose();
			System.Web.HttpContext.Current.Response.End();
			return null;
		}

		/// <summary>
		/// 生成EXCEL文档
		/// </summary>
		/// <param name="obj">文档内容，一般为HTML里的Table代码</param>
		/// <param name="fileName">文件名，可为空</param>
		public static void Xls(object obj, string fileName = null)
		{
			System.Web.HttpContext.Current.Response.Clear();
			if (fileName == null) { fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls"; }
			System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
			System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.Unicode;
			System.Web.HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
			System.Web.HttpContext.Current.Response.BinaryWrite(new byte[] { 0xFF, 0xFE }); //防止中文乱码
			System.Web.HttpContext.Current.Response.Write(obj);
			System.Web.HttpContext.Current.Response.End();
		}


		public static string Timer(int seconds)
		{
			System.Version v = new System.Version();
			string filePath = System.Web.HttpContext.Current.Server.MapPath(DateTime.Now.ToString("HHmmss") + ".txt");
			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Interval = seconds * 1000;
			timer.Elapsed += new System.Timers.ElapsedEventHandler(delegate(object source, System.Timers.ElapsedEventArgs e)
			{
				System.IO.File.AppendAllText(filePath, filePath, System.Text.Encoding.UTF8);
			});
			timer.AutoReset = true;//是否重复执行
			timer.Enabled = true;
			return null;
		}

		public delegate int DelegateEventHandler(int id = 0, string title = null);

		public static int Delegate(int id, DelegateEventHandler callback = null)
		{
			id = id * id;
			if (callback != null)
			{
				DelegateEventHandler mothod = new DelegateEventHandler(callback);
				id = mothod(id);
			}
			return id;
		}

		public static string ChinaDateTime(DateTime dateTime, string formatText = "")
		{
			System.Globalization.ChineseLunisolarCalendar clc = new System.Globalization.ChineseLunisolarCalendar();
			DateTime.TryParse("1983-10-20", out dateTime);
			int year = clc.GetYear(dateTime); int month = clc.GetMonth(dateTime); int day = clc.GetDayOfMonth(dateTime);
			int leapMonth = clc.GetLeapMonth(year);
			return year + "-" + month + "-" + day + "-" + leapMonth;
		}

		public static string ChinaDateTime(string formatText = "") { return ChinaDateTime(DateTime.Now, formatText); }

		public static string IsClass<T>(T className)
		{
			return className.ToString();
		}
		

	}
}