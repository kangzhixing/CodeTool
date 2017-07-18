﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CodeTool.Models.CoderMaker;

namespace CodeTool.common
{
    public class ReflectMethod_Dotnet
    {
        #region 反射调用方法

        public string RefEntity(CodeMakerGeneratCodeOut inModel)
        {
            var result = new StringBuilder();
            result.AppendLine(string.Format(@"
using System;

namespace {0}
{{
    public class {1} {{", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "Entity"), inModel.CodeMakerGeneratCodeIn.ClassName));

            inModel.FieldDescriptions.ForEach(f =>
            {
                result.AppendLine(@"
        public " + JlDbTypeMap.Map(f.DbType, f.IsNullable) + " " + f.Name + " { get; set; }" + (string.IsNullOrWhiteSpace(f.Description) ? "" : " //" + f.Description));
            });
            result.AppendLine(@"
    }
}");

            return result.ToString();
        }

        //Dao通用代码
        private static string DaoBaseCode(CodeMakerGeneratCodeOut inModel, string content = null)
        {
            var result = new StringBuilder();

            result.AppendLine(string.Format(@"
using System;
using En = {0};
using System.Data.SqlClient;
using System.Collections.Generic;
using JasonLib.Data;
using System.Data;

namespace {1}
{{

    public class {2}Dao{{
    {3}

    }}
}}", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "Entity"), string.Format(inModel.CodeMakerGeneratCodeIn.Package, "Dao"), inModel.CodeMakerGeneratCodeIn.ClassName, content));

            return result.ToString();
        }

        public string RefDao(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel);
        }

        public string RefDaoAdd(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var field = new StringBuilder();
            var fieldValue = new StringBuilder();
            var fieldParameter = new StringBuilder();
            inModel.FieldDescriptions.Where(f => !f.IsIdentity).ToList().ForEach(f =>
            {
                field.AppendLine("                [" + f.Name + "],");
                fieldValue.AppendLine("                @" + f.Name + ",");
                fieldParameter.AppendLine(string.Format("            parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = wherePart.{0} }});", f.Name, JlString.ToLowerFirst(className)));
            });


            var codeStr = string.Format(@"
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""wherePart"">添加实体</param>
        /// <returns>添加成功与否</returns>
        public static bool Add(string connectionString, dynamic wherePart) {{
            var sql = @""INSERT INTO [{1}]( 
{2}
            ) VALUES ( 
{3})"";
		
            var parameters = new List<SqlParameter>();

{4}		
            var i = JlDatabase.ExecuteNonQuery(connectionString, sql, parameters.ToArray());
		
            return i > 0;
        }}", className, tableName, field.ToString().Substring(0, field.Length - 3), fieldValue.ToString().Substring(0, fieldValue.Length - 3), fieldParameter);
            return DaoBaseCode(inModel, codeStr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        public string RefDaoUpdate(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();
            inModel.FieldDescriptions.ForEach(f =>
            {
                var field = new StringBuilder();
                var fieldParameter = new StringBuilder();
                inModel.FieldDescriptions.ForEach(ff =>
                {
                    if (!ff.IsIdentity)
                    {
                        field.AppendLine("               [" + ff.Name + "] = @" + ff.Name + ", ");
                    }
                    fieldParameter.AppendLine(string.Format("            parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = wherePart.{0} }});", ff.Name, JlString.ToLowerFirst(className)));
                });

                codeStr.AppendLine(string.Format(@"
        /// <summary>
        /// 修改实体
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""wherePart"">修改实体</param>
        /// <returns>修改成功与否</returns>
        public static bool UpdateBy{3}(string connectionString, dynamic wherePart) {{
            var sql = @""UPDATE [{5}] SET 
{2}
               WHERE [{3}] = @{3}"";

            var parameters = new List<SqlParameter>();

{4}		
            var i = JlDatabase.ExecuteNonQuery(connectionString, sql, parameters.ToArray());
		
            return i > 0;
        }}", className,
                JlString.ToLowerFirst(className),
                field.ToString().Substring(0, field.Length - 4),
                f.Name,
                fieldParameter,
                tableName));
            });
            return DaoBaseCode(inModel, codeStr.ToString());
        }

        public string RefDaoDelete(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();
            inModel.FieldDescriptions.ForEach(f =>
            {
                var field = new StringBuilder();
                var fieldParameter = new StringBuilder();

                fieldParameter.AppendLine();

                codeStr.AppendLine(string.Format(@"
        /// <summary>
        /// 通过{3}删除实体
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""wherePart"">删除属性</param>
        /// <returns>删除成功与否</returns>
        public static bool DeleteBy{3}(string connectionString, {0} {1}) {{
            var sql = ""DELETE FROM [{2}] WHERE [{3}] = @{3}"";
		
            var parameters = new List<SqlParameter>();

    {4}		
            var i = JlDatabase.ExecuteNonQuery(connectionString, sql, parameters.ToArray());
		
            return i > 0;
        }}", JlDbTypeMap.Map(f.DbType),
                JlString.ToLowerFirst(f.Name),
                tableName,
                f.Name,
                string.Format("        parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = {1} }});", f.Name, JlString.ToLowerFirst(f.Name))));
            });
            return DaoBaseCode(inModel, codeStr.ToString());
        }

        public string RefDaoGet(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();
            inModel.FieldDescriptions.ForEach(f =>
            {
                var field = new StringBuilder();
                var fieldParameter = new StringBuilder();
                fieldParameter.AppendLine(string.Format("            parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = wherePart.{0} }});", f.Name, JlString.ToLowerFirst(className)));
                inModel.FieldDescriptions.ForEach(ff =>
                {
                    field.AppendLine("                [" + ff.Name + "],");
                });

                codeStr.AppendLine(string.Format(@"
        /// <summary>
        /// 通过{1}查询实体
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""wherePart"">删除属性</param>
        /// <returns>查询结果</returns>
        public static bool GetBy{1}(string connectionString, dynamic wherePart) {{
            var sql = @""SELECT
{2}
                FROM [{0}] WITH (NOLOCK)
                WHERE [{1}] = @{1}"";
		
            var parameters = new List<SqlParameter>();

{3}		
            var i = JlDatabase.ExecuteNonQuery(connectionString, sql, parameters.ToArray());
		
            return i > 0;
        }}", tableName, f.Name,
                field.ToString().Substring(0, field.Length - 3),
                fieldParameter));
                //    }}", className, JlString.ToLowerFirst(className), field.ToString().Substring(0, field.Length - 3), fieldValue.ToString().Substring(0, fieldValue.Length - 3), fieldParameter));
            });
            return DaoBaseCode(inModel, codeStr.ToString());
        }

        /// <summary>
        /// RefDaoGetList
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        public string RefDaoGetList(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();

            var field = new StringBuilder();
            var fieldSetModel = new StringBuilder();

            inModel.FieldDescriptions.ForEach(f =>
            {
                field.AppendLine("            [" + f.Name + "],");
                if (JlDbTypeMap.Map(f.DbType) == "string")
                    fieldSetModel.AppendLine(string.Format("                {0} = row[\"{0}\"].ToString(),", f.Name, JlString.ToLowerFirst(className)));
                else
                    fieldSetModel.AppendLine(string.Format("                {0} = JlConvert.TryTo{1}(row[\"{0}\"]),", f.Name, JlString.ToUpperFirst(JlDbTypeMap.Map(f.DbType))));
            });

            Func<string, string, string> getSelectSql = (data, where) =>
            {
                return string.Format(@"@""SELECT
{0}
            FROM [{1}] WITH (NOLOCK){2}""",
                field.ToString().Substring(0, field.Length - 3),
                tableName,
                string.IsNullOrEmpty(where) ? "" : " " + where);
            };

            var fieldDataAccess = string.Format(@"
        var list = new List<En.{0}>();

        var dataTable = new DataTable();
        JlDatabase.Fill(connectionString, sql, dataTable);

        if (dataTable.Rows.Count > 0)
        {{
            list = dataTable.AsEnumerable().Select(row => new En.{0}()
            {{
{1}
            }});
        }}
        return list;", className, fieldSetModel.ToString().Substring(0, fieldSetModel.Length - 2));

            codeStr.AppendLine(string.Format(@"
        /// <summary>
        /// 查询所有实体列表（不存在时，返回null）
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <returns>查询结果集</returns>
        public static List<En.{1}> GetAll(string connectionString) {{
            string sql = {0};
{2}
        }}", getSelectSql(field.ToString(), null), className, fieldDataAccess));

            inModel.FieldDescriptions.ForEach(f =>
            {
                var fieldParameter = string.Format("        parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = wherePart.{0} }});", f.Name, JlString.ToLowerFirst(className));

                var sqlWhere = string.Format("WHERE [{0}] = @{0}", f.Name);
                codeStr.AppendLine(string.Format(@"
        /// <summary>
        /// 查询实体列表（不存在时，返回null）
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""wherePart"">查询实体</param>
        /// <returns>查询结果集</returns>
        public static List<En.{1}> GetListBy{3}(string connectionString, dynamic wherePart) {{
            string sql = {0};

            var parameters = new List<SqlParameter>();
{2}
{4}
    }}", getSelectSql(field.ToString(), sqlWhere), className, fieldParameter, f.Name, fieldDataAccess));

            });


            return DaoBaseCode(inModel, codeStr.ToString());
        }

        /// <summary>
        /// RefDaoGetPageList
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        public string RefDaoGetPageList(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();

            var field = new StringBuilder();
            var fieldSetModel = new StringBuilder();

            inModel.FieldDescriptions.ForEach(f =>
            {
                field.AppendLine("                [" + f.Name + "],");
                if (JlDbTypeMap.Map(f.DbType) == "string")
                    fieldSetModel.AppendLine(string.Format("                {0} = row[\"{0}\"].ToString(),", f.Name, JlString.ToLowerFirst(className)));
                else
                    fieldSetModel.AppendLine(string.Format("                {0} = JlConvert.TryTo{1}(row[\"{0}\"]),", f.Name, JlString.ToUpperFirst(JlDbTypeMap.Map(f.DbType))));
            });

            Func<string, string, string> getSelectSql = (data, where) =>
            {
                return string.Format(@"
            @""WITH Virtual_T AS(
                SELECT
{0}                
                ROW_NUMBER() OVER (%s) AS [RowNumber]
                FROM [{1}] WITH (NOLOCK)%s)
            SELECT * FROM Virtual_T
            WHERE @PageSize * (@CurrentPage - 1) < RowNumber AND RowNumber <= @PageSize * @CurrentPage""",
                field.ToString().Substring(0, field.Length - 1),
                tableName);
            };

            var fieldDataAccess = string.Format(@"
            var list = new List<En.{0}>();

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable);

            if (dataTable.Rows.Count > 0)
            {{
                list = dataTable.AsEnumerable().Select(row => new En.{0}()
                {{
    {1}
                }});
            }}
            return list;", className, fieldSetModel.ToString().Substring(0, fieldSetModel.Length - 2));

            codeStr.AppendLine(string.Format(@"
        /// <summary>
        /// 查询所有实体列表（不存在时，返回null）
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""pageSize"">每页行数</param>
        /// <param name=""currentPage"">当前页码</param>
        /// <param name=""sqlWhere"">判断条件语句</param>
        /// <param name=""sqlOrder"">排序语句</param>
        /// <returns>查询结果集</returns>
        public static List<En.{1}> GetPageListByConditionAndOrder(String connectionString, int pageSize, int currentPage, 
            string sqlWhere, string sqlOrder) {{
            string sql = {0};

            // 条件查询部分
            sql = string.Format(sql, sqlOrder, sqlWhere.Length == 0 ? """" : "" WHERE ("" + sqlWhere.Substring(0, sqlWhere.Length - 4) + "")"");
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter() {{ ParameterName = ""PageSize"", Value = pageSize }});
            parameters.Add(new SqlParameter() {{ ParameterName = ""CurrentPage"", Value = currentPage }});
{2}
        }}", getSelectSql(field.ToString(), null), className, fieldDataAccess));


            return DaoBaseCode(inModel, codeStr.ToString());
        }

        #endregion
    }
}