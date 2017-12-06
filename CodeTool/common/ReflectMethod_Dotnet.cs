using System;
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
            result.AppendLine(string.Format(@"using System;

namespace {0}
{{
    public class {1} {{", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "Entity"), inModel.CodeMakerGeneratCodeIn.ClassName));

            inModel.FieldDescriptions.ForEach(f =>
            {
                result.AppendLine(@"
        /// <summary>
        /// " + (string.IsNullOrWhiteSpace(f.Description) ? f.Name : f.Description.Replace("\n", "  ")) + @"
        /// </summary>
        public " + JlDbTypeMap.Map(f.DbType, f.IsNullable) + " " + f.Name + " { get; set; }");
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

            result.AppendLine(string.Format(@"using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using JasonLib.Data;
using System.Data;
using System.Linq;
using En = {3};

namespace {0}
{{

    public class {1}Dao{{
    {2}

    }}
}}", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "Dao"), inModel.CodeMakerGeneratCodeIn.ClassName, content, string.Format(inModel.CodeMakerGeneratCodeIn.Package, "Entity")));

            return result.ToString();
        }

        public string RefDao(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel);
        }

        public string RefDaoAdd(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_DotNet.DaoAdd(inModel));
        }

        public string RefDaoUpdate(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_DotNet.DaoUpdate(inModel));
        }

        public string RefDaoDelete(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_DotNet.DaoDelete(inModel));

        }

        public string RefDaoGet(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_DotNet.DaoGet(inModel));
        }

        public string RefDaoGetList(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_DotNet.DaoGetList(inModel));
        }

        public string RefDaoGetPageList(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_DotNet.DaoGetPageList(inModel));
        }

        public string RefDaoAll(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_DotNet.DaoAdd(inModel)
                + GenerateCode_DotNet.DaoUpdate(inModel)
                + GenerateCode_DotNet.DaoDelete(inModel)
                + GenerateCode_DotNet.DaoGet(inModel)
                + GenerateCode_DotNet.DaoGetList(inModel)
                + GenerateCode_DotNet.DaoGetPageList(inModel)); 
        }

        #endregion
    }

    public class GenerateCode_DotNet
    {
        public static string DaoAdd(CodeMakerGeneratCodeOut inModel)
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

            return @"

        #region 添加

" + codeStr.ToString() + @"


        #endregion";

        }

        public static string DaoUpdate(CodeMakerGeneratCodeOut inModel)
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
        public static bool UpdateBy{6}(string connectionString, dynamic wherePart) {{
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
                tableName,
                JlString.ToUpperFirst(f.Name)));
            });
            return @"

        #region 修改

" + codeStr.ToString() + @"


        #endregion";
        }

        public static string DaoDelete(CodeMakerGeneratCodeOut inModel)
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
        public static bool DeleteBy{4}(string connectionString, {0} {1}) {{
            var sql = ""DELETE FROM [{2}] WHERE [{3}] = @{3}"";
		
            var parameters = new List<SqlParameter>();

    {5}		
            var i = JlDatabase.ExecuteNonQuery(connectionString, sql, parameters.ToArray());
		
            return i > 0;
        }}", JlDbTypeMap.Map(f.DbType),
                JlString.ToLowerFirst(f.Name),
                tableName,
                f.Name,
                JlString.ToUpperFirst(f.Name),
                string.Format("        parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = {1} }});", f.Name, JlString.ToLowerFirst(f.Name))));
            });
            return @"

        #region 删除

" + codeStr.ToString() + @"


        #endregion";
        }

        public static string DaoGet(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();
            var field = new StringBuilder();
            var fieldSetModel = new StringBuilder();

            inModel.FieldDescriptions.ForEach(ff =>
            {
                field.AppendLine("                [" + ff.Name + "],");
                if (JlDbTypeMap.Map(ff.DbType) == "string")
                    fieldSetModel.AppendLine(string.Format("                    {0} = row[\"{0}\"].ToString(),", ff.Name, JlString.ToLowerFirst(className)));
                else
                    fieldSetModel.AppendLine(string.Format("                    {0} = JlConvert.TryTo{1}(row[\"{0}\"]),", ff.Name, JlString.ToUpperFirst(JlDbTypeMap.Map(ff.DbType))));
            });
            inModel.FieldDescriptions.ForEach(f =>
            {
                var fieldParameter = new StringBuilder();
                fieldParameter.AppendLine(string.Format("            parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = wherePart.{0} }});", f.Name, JlString.ToLowerFirst(className)));

                codeStr.AppendLine(string.Format(@"

        /// <summary>
        /// 通过{1}查询实体
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""wherePart"">删除属性</param>
        /// <returns>查询结果</returns>
        public static En.{4} GetBy{6}(string connectionString, dynamic wherePart) {{
            var sql = @""SELECT
{2}
                FROM [{0}] WITH (NOLOCK)
                WHERE [{1}] = @{1}"";
		
            var parameters = new List<SqlParameter>();

{3}		
            var dataTable = new DataTable();
            SlDatabase.Fill(connectionString, sql, dataTable, parameters.ToArray());

            if (dataTable.Rows.Count > 0)
            {{
                var row = dataTable.Rows[0];
                return new En.ConsumeType()
                {{
{5}
                }};
            }}
            else
            {{
                return null;
            }}
        }}
", tableName, f.Name,
                field.ToString().Substring(0, field.Length - 3),
                fieldParameter, className, fieldSetModel, JlString.ToUpperFirst(f.Name)));
            });
            return @"

        #region 查询

" + codeStr.ToString() + @"


        #endregion";
        }

        public static string DaoGetList(CodeMakerGeneratCodeOut inModel)
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
                    fieldSetModel.AppendLine(string.Format("                    {0} = row[\"{0}\"].ToString(),", f.Name, JlString.ToLowerFirst(className)));
                else
                    fieldSetModel.AppendLine(string.Format("                    {0} = JlConvert.TryTo{1}(row[\"{0}\"]),", f.Name, JlString.ToUpperFirst(JlDbTypeMap.Map(f.DbType))));
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
                }}).ToList();
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
                var fieldParameter = string.Format("            parameters.Add(new SqlParameter() {{ ParameterName = \"{0}\", Value = wherePart.{0} }});", f.Name, JlString.ToLowerFirst(className));

                var sqlWhere = string.Format("WHERE [{0}] = @{0}", f.Name);
                codeStr.AppendLine(string.Format(@"

        /// <summary>
        /// 查询实体列表（不存在时，返回null）
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""wherePart"">查询实体</param>
        /// <returns>查询结果集</returns>
        public static List<En.{1}> GetListBy{5}(string connectionString, dynamic wherePart) {{
            string sql = {0};

            var parameters = new List<SqlParameter>();
{2}
{4}
        }}", getSelectSql(field.ToString(), sqlWhere), className, fieldParameter, f.Name, fieldDataAccess, JlString.ToUpperFirst(f.Name)));

            });


            return @"

        #region 查询

" + codeStr.ToString() + @"


        #endregion";
        }

        public static string DaoGetPageList(CodeMakerGeneratCodeOut inModel)
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
                    fieldSetModel.AppendLine(string.Format("                    {0} = row[\"{0}\"].ToString(),", f.Name, JlString.ToLowerFirst(className)));
                else
                    fieldSetModel.AppendLine(string.Format("                    {0} = JlConvert.TryTo{1}(row[\"{0}\"]),", f.Name, JlString.ToUpperFirst(JlDbTypeMap.Map(f.DbType))));
            });

            Func<string, string, string> getSelectSql = (data, where) =>
            {
                return string.Format(@"
            @""WITH Virtual_T AS(
                SELECT
{0}                
                ROW_NUMBER() OVER ({{0}}) AS [RowNumber]
                FROM [{1}] WITH (NOLOCK){{1}})
            SELECT * FROM Virtual_T
            WHERE @PageSize * (@CurrentPage - 1) < RowNumber AND RowNumber <= @PageSize * @CurrentPage""",
                field.ToString().Substring(0, field.Length - 1),
                tableName);
            };

            var fieldDataAccess = string.Format(@"
            var list = new List<En.{0}>();

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable, parameters.ToArray());

            if (dataTable.Rows.Count > 0)
            {{
                list = dataTable.AsEnumerable().Select(row => new En.{0}()
                {{
{1}
                }}).ToList();
            }}
            return list;", className, fieldSetModel.ToString().Substring(0, fieldSetModel.Length - 3));

            var fieldDataAccess_Count = string.Format(@"
            var list = new List<En.{0}>();

            var dataTable = new DataTable();
            JlDatabase.Fill(connectionString, sql, dataTable, parameters.ToArray());

            //记录总数计算
            var sqlCount = string.Format(""SELECT COUNT(*) CNT FROM [{2}] {{0}} "", string.IsNullOrEmpty(sqlWhere) ? """"
                : "" WHERE("" + sqlWhere.Remove(sqlWhere.Length - 4) + "")"");
            totalCount = SlConvert.TryToInt32(JlDatabase.ExecuteScalar(connectionString, sqlCount));

            if (dataTable.Rows.Count > 0)
            {{
                list = dataTable.AsEnumerable().Select(row => new En.{0}()
                {{
{1}
                }}).ToList();
            }}
            return list;", className, fieldSetModel.ToString().Substring(0, fieldSetModel.Length - 3), tableName);

            codeStr.AppendLine(string.Format(@"

        #region 查询结果集


        /// <summary>
        /// 查询所有实体列表（不存在时，返回null）
        /// </summary>
        /// <param name=""connectionString"">连接字符串</param>
        /// <param name=""pageSize"">每页行数</param>
        /// <param name=""currentPage"">当前页码</param>
        /// <param name=""sqlWhere"">判断条件语句</param>
        /// <param name=""sqlOrder"">排序语句</param>
        /// <returns>查询结果集</returns>
        public static List<En.{1}> GetPageListByConditionAndOrder(string connectionString, int pageSize, int currentPage, 
            string sqlWhere, string sqlOrder) {{
            string sql = {0};

            // 条件查询部分
            sql = string.Format(sql, sqlOrder, string.IsNullOrEmpty(sqlWhere) ? """" : "" WHERE ("" + sqlWhere.Substring(0, sqlWhere.Length - 4) + "")"");
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter() {{ ParameterName = ""PageSize"", Value = pageSize }});
            parameters.Add(new SqlParameter() {{ ParameterName = ""CurrentPage"", Value = currentPage }});
{2}
        }}", getSelectSql(field.ToString(), null), className, fieldDataAccess));

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
        public static List<En.{1}> GetPageListByConditionAndOrder(string connectionString, int pageSize, int currentPage, 
            string sqlWhere, string sqlOrder, out int totalCount) {{
            string sql = {0};

            // 条件查询部分
            sql = string.Format(sql, sqlOrder, string.IsNullOrEmpty(sqlWhere) ? """" : "" WHERE ("" + sqlWhere.Substring(0, sqlWhere.Length - 4) + "")"");
            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter() {{ ParameterName = ""PageSize"", Value = pageSize }});
            parameters.Add(new SqlParameter() {{ ParameterName = ""CurrentPage"", Value = currentPage }});
{2}
        }}

        #endregion
", getSelectSql(field.ToString(), null), className, fieldDataAccess_Count));

            return codeStr.ToString();

        }
    }

}