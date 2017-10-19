using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using JasonLib;
using WebGrease.Css.Extensions;
using JasonLib.Data;
using System.Data;
using System.Web;
using System.Data.Common;

namespace CodeTool.common
{
    public class CommonMethod
    {
        public static Dictionary<string, MethodInfo> GetAllPageMethod()
        {
            var parentType = Type.GetType(JlConfig.GetValue<string>("BaseController"));
            var controllers = Assembly.GetAssembly(parentType).GetTypes().Where(t => t.BaseType.Name == "BaseController").ToArray();

            var pageClass = new Dictionary<string, MethodInfo>();
            controllers.ForEach(c =>
            {
                c.GetMethods().Where(t =>
                    t.GetCustomAttributes(typeof(ViewPageAttribute), true).Length > 0).ForEach(m =>
                    {
                        pageClass.Add(GetMethodDescription(m).ToLower(), m);
                    });
            });

            return pageClass;
        }

        public static string GetUrlByControllerMethod(MethodInfo method)
        {
            var parentType = method.DeclaringType;
            var result = new StringBuilder();
            var dir = parentType.FullName.Split('.').ToList();
            for (var i = dir.IndexOf("Controllers") + 1; i < dir.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(dir[i]))
                    result.Append("/" + dir[i]);
            }

            return result.ToString().Substring(0, result.Length - 10) + "/" + method.Name;
        }

        public static string GetMethodDescription(MethodInfo method)
        {
            var descriptionAttributes = method.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (descriptionAttributes.Length > 0)
            {
                return ((DescriptionAttribute)descriptionAttributes.First()).Description;
            }
            else
            {
                return string.Empty;
            }
        }

        public static List<JlFieldDescription> GetDatabaseColumns(string connectionString, string tableName, JlDatabaseType dbType)
        {
            switch (dbType)
            {
                case JlDatabaseType.MySql:
                    {
                        return GetDatabaseColumns_MySql(connectionString, tableName);
                    }
                case JlDatabaseType.SqlServer:
                    {
                        return GetDatabaseColumns_SqlServer(connectionString, tableName);
                    }
                default: return null;
            }
        }

        public static List<string> GetDatabaseTables(string connectionString, JlDatabaseType dbType)
        {
            switch (dbType)
            {
                case JlDatabaseType.MySql:
                    {
                        return GetDatabaseTables_MySql(connectionString);
                    }
                case JlDatabaseType.SqlServer:
                    {
                        return GetDatabaseTables_SqlServer(connectionString);
                    }
                default: return null;
            }
        }

        private static List<string> GetDatabaseTables_MySql(string connectionString)
        {
            var dbName = connectionString.Split(new[] { "database=" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(';')[0];
            var sql = @"SELECT TABLE_NAME Name FROM information_schema.TABLES WHERE TABLE_SCHEMA = '" + dbName + "' AND TABLE_TYPE = 'BASE TABLE'";

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable, databaseType: JlDatabaseType.MySql);

            return dataTable.AsEnumerable().Select(row => row["Name"].ToString()).ToList();
        }

        private static List<string> GetDatabaseTables_SqlServer(string connectionString)
        {
            var sql = @"SELECT name AS Name FROM SYSOBJECTS WHERE XTYPE IN ('V','U') AND NAME<>'DTPROPERTIES' ORDER BY Name ASC";

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable);

            return dataTable.AsEnumerable().Select(row => row["Name"].ToString()).ToList();
        }

        private static List<JlFieldDescription> GetDatabaseColumns_SqlServer(string connectionString, string tableName)
        {
            var sql = @"
                    WITH T AS
                    (
	                    SELECT
		                    D.name AS TableName,
		                    A.name AS Name,
		                    B.name AS DbType,
		                    COLUMNPROPERTY(A.ID,A.NAME,'PRECISION') AS Length,
		                    (CASE WHEN A.ISNULLABLE=1 THEN 'true'ELSE 'false' END) AS IsNullable,
							(CASE WHEN A.COLSTAT=1 THEN 'true'ELSE 'false' END) AS IsIdentity,
		                    ISNULL(G.[VALUE],'') AS Description,
                            colorder
	                    FROM SYSCOLUMNS A LEFT JOIN SYSTYPES B
	                    ON A.XTYPE=B.XUSERTYPE
	                    INNER JOIN SYSOBJECTS D
	                    ON A.ID=D.ID AND D.XTYPE IN ('V','U') AND D.NAME<>'DTPROPERTIES'
	                    LEFT JOIN SYSCOMMENTS E
	                    ON A.CDEFAULT=E.ID
	                    LEFT JOIN SYS.EXTENDED_PROPERTIES G
	                    ON A.ID=G.MAJOR_ID AND A.COLID = G.MINOR_ID
                    )
                    SELECT Name,DbType,Length,IsNullable,IsIdentity,Description FROM T WHERE TableName = '{0}' ORDER BY colorder";
            sql = string.Format(sql, tableName);

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable);

            return dataTable.AsEnumerable().Select(row => new JlFieldDescription()
            {
                Name = row["Name"].ToString(),
                DbType = row["DbType"].ToString(),
                Length = JlConvert.TryToInt(row["Length"]),
                IsNullable = JlConvert.TryToBoolean(row["IsNullable"].ToString()),
                IsIdentity = JlConvert.TryToBoolean(row["IsIdentity"].ToString()),
                Description = HttpUtility.HtmlEncode(row["Description"].ToString())
            }).ToList();
        }

        private static List<JlFieldDescription> GetDatabaseColumns_MySql(string connectionString, string tableName)
        {
            var dbName = connectionString.Split(new[] { "database=" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(';')[0];
            var sql = @"
                    SELECT COLUMN_NAME Name, COLUMN_COMMENT Description, DATA_TYPE DbType, IS_NULLABLE IsNullable, CHARACTER_MAXIMUM_LENGTH Length, Extra, COLUMN_KEY
                    FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '" + dbName + "' AND TABLE_NAME like '{0}'";
            sql = string.Format(sql, tableName);

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable, databaseType: JlDatabaseType.MySql);

            return dataTable.AsEnumerable().Select(row => new JlFieldDescription()
            {
                Name = row["Name"].ToString(),
                DbType = row["DbType"].ToString(),
                Length = JlConvert.TryToInt(row["Length"]),
                IsNullable = JlConvert.TryToBoolean(row["IsNullable"].ToString().ToLower() == "yes"),
                IsIdentity = JlConvert.TryToBoolean(row["Extra"].ToString().Contains("auto_increment")),
                ColumnKey = row["COLUMN_KEY"].ToString(),
                Description = HttpUtility.HtmlEncode(row["Description"].ToString())
            }).ToList();
        }
    }
}