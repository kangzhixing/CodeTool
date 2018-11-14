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
            var cache = JlHttpCache.Current;
            var pages = cache.Get<Dictionary<string, MethodInfo>>("pages");
            if (pages == null)
            {
                var parentType = Type.GetType(JlConfig.GetValue<string>("BaseController"));
                var controllers = Assembly.GetAssembly(parentType).GetTypes().Where(t => t.BaseType.Name == "BaseController").ToArray();

                pages = new Dictionary<string, MethodInfo>();
                controllers.ForEach(c =>
                {
                    c.GetMethods().Where(t => t.GetCustomAttributes(typeof(ViewPageAttribute), true).Length > 0).ForEach(m =>
                    {
                        pages.Add(GetMethodDescription(m).ToLower(), m);
                    });
                });
                cache.Set("pages", pages);
            }

            return pages;
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
                case JlDatabaseType.PostgreSql:
                    {
                        return GetDatabaseColumns_PostgreSql(connectionString, tableName);
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
                case JlDatabaseType.PostgreSql:
                    {
                        return GetDatabaseTables_PostgreSql(connectionString);
                    }
                default: return null;
            }
        }

        /// <summary>
        /// 将DataTable转换为List<object>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="hasHeader"></param>
        /// <returns></returns>
        public static List<object> ConvertDataTableToList(DataTable dt, bool hasHeader = false)
        {
            var result = new List<object>();

            if (hasHeader)
            {
                var rowHeader = new List<object>();
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    rowHeader.Add(dt.Columns[i].ToString());
                }
                result.Add(rowHeader);
            }

            for (var j = 0; j < dt.Rows.Count && j < 100; j++)
            {
                var row = new List<object>();
                for (var k = 0; k < dt.Columns.Count; k++)
                {
                    switch (dt.Columns[k].DataType.Name.ToLower())
                    {
                        case "string":
                            row.Add(dt.Rows[j][k].ToString());
                            break;
                        case "bool":
                            row.Add(JlConvert.TryToBool(dt.Rows[j][k]));
                            break;
                        case "int32":
                        case "decimal":
                            row.Add(JlConvert.TryToDouble(dt.Rows[j][k]));
                            break;
                        default:
                            row.Add(dt.Rows[j][k].ToString());
                            break;
                    }
                }
                result.Add(row);
            }

            return result;
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

        private static List<string> GetDatabaseTables_PostgreSql(string connectionString)
        {
            return GetDatabaseTableDic_PostgreSql(connectionString).Keys.ToList();
        }

        private static Dictionary<string, string> GetDatabaseTableDic_PostgreSql(string connectionString)
        {
            var result = new Dictionary<string, string>();

            var sql = @"select relname as Name,cast(obj_description(relfilenode,'pg_class') as varchar) as comment from pg_class c 
where  relkind = 'r' and relname not like 'pg_%' and relname not like 'sql_%' order by relname";

            var ds = new DataSet();
            JlDatabase.Fill(connectionString, sql, ds, null, JlDatabaseType.PostgreSql);
            ds.Tables[0].AsEnumerable().ForEach(row =>
            {
                result.Add(row["Name"].ToString(), row["comment"].ToString());
            });
            return result;
        }

        private static List<JlFieldDescription> GetDatabaseColumns_SqlServer(string connectionString, string tableName)
        {
            var sql = @"
SELECT 
    Name=C.name,
    DbType=T.name,
    PrimaryKey=ISNULL(IDX.PrimaryKey,N''),
    IsIdentity=CASE WHEN C.is_identity=1 THEN N'true'ELSE N'false' END,
    Length=C.max_length,
    IsNullable=CASE WHEN C.is_nullable=1 THEN N'true'ELSE N'false' END,
    Description=ISNULL(PFD.[value],N'')
FROM sys.columns C
    INNER JOIN sys.objects O
        ON C.[object_id]=O.[object_id]
            AND O.type='U'
            AND O.is_ms_shipped=0
    INNER JOIN sys.types T
        ON C.user_type_id=T.user_type_id
    LEFT JOIN sys.default_constraints D
        ON C.[object_id]=D.parent_object_id
            AND C.column_id=D.parent_column_id
            AND C.default_object_id=D.[object_id]
    LEFT JOIN sys.extended_properties PFD
        ON PFD.class=1 
            AND C.[object_id]=PFD.major_id 
            AND C.column_id=PFD.minor_id
--             AND PFD.name='Caption'  -- 字段说明对应的描述名称(一个字段可以添加多个不同name的描述)
    LEFT JOIN sys.extended_properties PTB
        ON PTB.class=1 
            AND PTB.minor_id=0 
            AND C.[object_id]=PTB.major_id
--             AND PFD.name='Caption'  -- 表说明对应的描述名称(一个表可以添加多个不同name的描述) 
    LEFT JOIN                       -- 索引及主键信息
    (
        SELECT 
            IDXC.[object_id],
            IDXC.column_id,
            Sort=CASE INDEXKEY_PROPERTY(IDXC.[object_id],IDXC.index_id,IDXC.index_column_id,'IsDescending')
                WHEN 1 THEN 'DESC' WHEN 0 THEN 'ASC' ELSE '' END,
            PrimaryKey=CASE WHEN IDX.is_primary_key=1 THEN N'PRI'ELSE N'' END,
            IndexName=IDX.Name
        FROM sys.indexes IDX
        INNER JOIN sys.index_columns IDXC
            ON IDX.[object_id]=IDXC.[object_id]
                AND IDX.index_id=IDXC.index_id
        LEFT JOIN sys.key_constraints KC
            ON IDX.[object_id]=KC.[parent_object_id]
                AND IDX.index_id=KC.unique_index_id
        INNER JOIN  -- 对于一个列包含多个索引的情况,只显示第1个索引信息
        (
            SELECT [object_id], Column_id, index_id=MIN(index_id)
            FROM sys.index_columns
            GROUP BY [object_id], Column_id
        ) IDXCUQ
            ON IDXC.[object_id]=IDXCUQ.[object_id]
                AND IDXC.Column_id=IDXCUQ.Column_id
                AND IDXC.index_id=IDXCUQ.index_id
    ) IDX
        ON C.[object_id]=IDX.[object_id]
            AND C.column_id=IDX.column_id 
			where O.name = '{0}'
            order by c.column_id";
            sql = string.Format(sql, tableName);

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable);

            return dataTable.AsEnumerable().Select(row => new JlFieldDescription()
            {
                Name = JlString.ReplaceUnderline(row["Name"].ToString()),
                DbType = row["DbType"].ToString(),
                Length = JlConvert.TryToInt(row["Length"]),
                IsNullable = JlConvert.TryToBool(row["IsNullable"].ToString()),
                IsIdentity = JlConvert.TryToBool(row["IsIdentity"].ToString()),
                Description = HttpUtility.HtmlEncode(row["Description"].ToString()),
                ColumnKey = row["PrimaryKey"].ToString()
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
                Name = JlString.ReplaceUnderline(row["Name"].ToString()),
                DbType = row["DbType"].ToString(),
                Length = JlConvert.TryToInt(row["Length"]),
                IsNullable = JlConvert.TryToBool(row["IsNullable"].ToString().ToLower() == "yes"),
                IsIdentity = JlConvert.TryToBool(row["Extra"].ToString().Contains("auto_increment")),
                ColumnKey = row["COLUMN_KEY"].ToString(),
                Description = HttpUtility.HtmlEncode(row["Description"].ToString())
            }).ToList();
        }

        private static List<JlFieldDescription> GetDatabaseColumns_PostgreSql(string connectionString, string tableName)
        {
            var dbName = connectionString.Split(new[] { "database=" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(';')[0];
            var sql = @"
select ordinal_position as Colorder,column_name as Name,data_type as DbType,
coalesce(character_maximum_length,numeric_precision,-1) as Length,numeric_scale as Scale,
case is_nullable when 'NO' then 0 else 1 end as IsNullable,column_default as DefaultVal,
case  when position('nextval' in column_default)>0 then 1 else 0 end as IsIdentity, 
case when b.pk_name is null then 0 else 1 end as IsPK,c.DeText Description
from information_schema.columns 
left join (
    select pg_attr.attname as colname,pg_constraint.conname as pk_name from pg_constraint  
    inner join pg_class on pg_constraint.conrelid = pg_class.oid 
    inner join pg_attribute pg_attr on pg_attr.attrelid = pg_class.oid and  pg_attr.attnum = pg_constraint.conkey[1] 
    inner join pg_type on pg_type.oid = pg_attr.atttypid
    where pg_class.relname = '{0}' and pg_constraint.contype='p' 
) b on b.colname = information_schema.columns.column_name
left join (
    select attname,description as DeText from pg_class
    left join pg_attribute pg_attr on pg_attr.attrelid= pg_class.oid
    left join pg_description pg_desc on pg_desc.objoid = pg_attr.attrelid and pg_desc.objsubid=pg_attr.attnum
    where pg_attr.attnum>0 and pg_attr.attrelid=pg_class.oid and pg_class.relname='{0}'
)c on c.attname = information_schema.columns.column_name
where table_schema='public' and table_name='{0}' order by ispk desc,isidentity desc,ordinal_position asc";
            sql = string.Format(sql, tableName);

            var ds = new DataSet();
            JlDatabase.Fill(connectionString, sql, ds, null, JlDatabaseType.PostgreSql);

            return ds.Tables[0].AsEnumerable().Select(row => new JlFieldDescription()
            {
                Name = row["Name"].ToString(),
                DbType = row["DbType"].ToString(),
                Length = JlConvert.TryToInt(row["Length"]),
                IsNullable = JlConvert.TryToBool(row["IsNullable"].ToString() == "1"),
                IsIdentity = JlConvert.TryToBool(row["IsIdentity"].ToString() == "1"),
                ColumnKey = row["IsPK"].ToString() == "1" ? "PRI" : "",
                Description = HttpUtility.HtmlEncode(row["Description"].ToString())
            }).ToList();
        }
    }
}