using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CodeTool.Models.CoderMaker;
using JasonLib.Data;

namespace CodeTool.common
{
    public class ReflectMethod_Java
    {
        #region 反射调用方法

        public string RefEntity(CodeMakerGeneratCodeOut inModel)
        {
            var result = new StringBuilder();
            var getterAndSetter = new StringBuilder();

            result.AppendLine(string.Format(@"package {0};

import java.util.*;
import java.math.*;

public class {1} {{
", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "entity"), inModel.CodeMakerGeneratCodeIn.ClassName));

            foreach (var f in inModel.FieldDescriptions)
            {
                if (!string.IsNullOrEmpty(f.Description))
                    result.AppendLine("    //" + f.Description.Replace("\n", "  "));
                result.AppendLine(@"    private " + JlDbTypeMap.Map4J(f.DbType, f.IsNullable, inModel.databaseType) + " " + JlString.ToLowerFirst(f.Name) + @";
");

                getterAndSetter.AppendLine(string.Format(@"    public {2} get{1}() {{
        return {0};
    }}
    public void set{1}({2} {0}) {{
        this.{0} = {0};
    }}
", JlString.ToLowerFirst(f.Name), JlString.ToUpperFirst(f.Name), JlDbTypeMap.Map4J(f.DbType, f.IsNullable, inModel.databaseType)));

            }
            result.Append(getterAndSetter);
            result.AppendLine("}");

            return result.ToString();
        }

        public string RefCloneObject(CodeMakerGeneratCodeOut inModel)
        {
            var result = new StringBuilder();

            foreach (var f in inModel.FieldDescriptions)
            {
                result.AppendLine(string.Format(@"model.set{0}(vo.get{0}());", JlString.ToUpperFirst(f.Name)));
            }

            return result.ToString();
        }

        //Dao通用代码
        private static string DaoBaseCode(CodeMakerGeneratCodeOut inModel, string content = null)
        {
            var result = new StringBuilder();

            result.AppendLine(string.Format(@"package {0};

import java.io.IOException;
import java.util.*;
import java.math.*;

import javax.sql.rowset.CachedRowSet;

import {2}.{1};
import com.Fang.framework.*;

public class {1}Dao{{
{3}


}}", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "dao"), inModel.CodeMakerGeneratCodeIn.ClassName, string.Format(inModel.CodeMakerGeneratCodeIn.Package, "entity"), content));

            return result.ToString();
        }

        public string RefDao(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel);
        }

        public string RefDaoAdd(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_Java.DaoAdd(inModel));
        }

        public string RefDaoUpdate(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_Java.DaoUpdate(inModel));
        }

        public string RefDaoDelete(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_Java.DaoDelete(inModel));
        }

        public string RefDaoGet(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_Java.DaoGet(inModel));
        }

        public string RefDaoGetList(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_Java.DaoGetList(inModel));
        }

        public string RefDaoGetPageList(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_Java.DaoGetPageList(inModel));
        }

        public string RefDaoAll(CodeMakerGeneratCodeOut inModel)
        {
            return DaoBaseCode(inModel, GenerateCode_Java.DaoAdd(inModel)
                + GenerateCode_Java.DaoUpdate(inModel)
                + GenerateCode_Java.DaoDelete(inModel)
                + GenerateCode_Java.DaoGet(inModel)
                + GenerateCode_Java.DaoGetList(inModel)
                + GenerateCode_Java.DaoGetPageList(inModel));
        }

        public string RefMybatisMapper(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var field_Basic = new StringBuilder();
            var field_Add = new StringBuilder();
            var fieldValue = new StringBuilder();
            var fieldParams = new StringBuilder();
            var field_SqlContent = new StringBuilder();
            var field = new JlFieldDescription();
            if (inModel.FieldDescriptions.Any(f => f.ColumnKey == "PRI"))
            {
                field = inModel.FieldDescriptions.First(f => f.ColumnKey == "PRI");
            }
            else
            {
                field = inModel.FieldDescriptions.FirstOrDefault();
            }
            var codeStr = new StringBuilder();
            codeStr.AppendLine(string.Format(
@"package {0};

import {1};
import java.util.List;

public interface {2}Mapper{{

    {7}By{3}({4} {5});

    List<{2}> selectAll();

    int deleteBy{3}({4} {5});

    int updateBy{3}({2} {6});

    int updateBy{3}Selective({2} {6});

    int insert({2} {6});
    
}}",
            string.Format(inModel.CodeMakerGeneratCodeIn.Package, "dao"),
            string.Format(inModel.CodeMakerGeneratCodeIn.Package, "bean.") + className,
            className,
            JlString.ToUpperFirst(field.Name),
            JlDbTypeMap.Map4J(field.DbType, false, inModel.databaseType),
            JlString.ToLowerFirst(field.Name),
            JlString.ToLowerFirst(className),
            inModel.FieldDescriptions.Any(f => f.ColumnKey == "PRI") ? className + " select" : "List<" + className + "> selectList"));

            return codeStr.ToString();
        }

        public string RefMybatisMapperXml(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var field_Basic = new StringBuilder();
            var field_Add = new StringBuilder();
            var fieldValue = new StringBuilder();
            var fieldParams = new StringBuilder();
            var field_SqlContent = new StringBuilder();

            var codeStr = new StringBuilder();
            codeStr.AppendLine(
@"<!DOCTYPE mapper PUBLIC "" -//mybatis.org//DTD Mapper 3.0//EN"" ""http://mybatis.org/dtd/mybatis-3-mapper.dtd"">
<mapper namespace=""" + string.Format(inModel.CodeMakerGeneratCodeIn.Package, "dao.") + className + "Mapper\">");

            inModel.FieldDescriptions.ToList().ForEach(f =>
            {
                field_Basic.AppendLine(string.Format("    <{0} column=\"" + JlString.ToUpperFirst(f.Name) + "\" jdbcType=\"" + JlDbTypeMap.Map4Mybatis(f.DbType).ToUpper() + "\" property=\"" + JlString.ToLowerFirst(f.Name) + "\" />",
                    f.ColumnKey == "PRI" ? "id" : "result"));

                if (fieldParams.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None).Last().Length > 100)
                {
                    fieldParams.Append("\r\n    ");
                }
                fieldParams.Append(f.Name + ", ");
            });

            field_SqlContent.Append(GenerateCode_Java.MybatisSelect(inModel));
            field_SqlContent.Append(GenerateCode_Java.MybatisDelete(inModel));
            field_SqlContent.Append(GenerateCode_Java.MybatisInsert(inModel));
            field_SqlContent.Append(GenerateCode_Java.MybatisUpdate(inModel));

            codeStr.AppendLine(string.Format(
@"  <resultMap id=""BaseResultMap"" type=""{0}"">
{1}  </resultMap>
  <sql id=""Base_Column_List"">
    {2}
  </sql>
{3}</mapper>", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "bean.") + className, field_Basic, fieldParams.ToString().Substring(0, fieldParams.Length - 2), field_SqlContent));

            return codeStr.ToString();
        }

        #endregion
    }

    public class GenerateCode_Java
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
                field.AppendLine("            + \"[" + f.Name + "], \"");
                fieldValue.AppendLine("            + \"@" + f.Name + ", \"");
                if (JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType) == "Date")
                    fieldParameter.AppendLine(string.Format("        parameters.put(\"{0}\", new java.sql.Date({1}.get{0}().getTime()));", f.Name, JlString.ToLowerFirst(className)));
                else
                    fieldParameter.AppendLine(string.Format("        parameters.put(\"{0}\", {1}.get{0}());", f.Name, JlString.ToLowerFirst(className)));
            });


            var codeStr = string.Format(@"
    /**
    * 添加实体
    * 
    * @param connectionString 连接字符串
    * @param {1} 添加实体
    * 
    * @return 添加成功与否
    */
    public static boolean Add(String connectionString, {0} {1}) throws Exception {{
        String sql = ""INSERT INTO [{5}]( ""
{2}
            + "") VALUES ( ""
{3})"";
		
        HashMap<String, Object> parameters = new HashMap<String, Object>();

{4}		
        int i = SlDatabase.executeNonQuery(connectionString, sql, parameters);
		
        return i > 0;
    }}", className, JlString.ToLowerFirst(className), field.ToString().Substring(0, field.Length - 5) + "\"", fieldValue.ToString().Substring(0, fieldValue.Length - 5), fieldParameter, tableName);

            return codeStr.ToString();
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
                        field.AppendLine("            + \"[" + ff.Name + "] = @" + ff.Name + ", \"");
                    }
                    fieldParameter.AppendLine(JlDbTypeMap.Map4J(ff.DbType, false, inModel.databaseType) == "Date"
                        ? string.Format("        parameters.put(\"{0}\", new java.sql.Date({1}.get{0}().getTime()));", ff.Name, JlString.ToLowerFirst(className))
                        : string.Format("        parameters.put(\"{0}\", {1}.get{0}());", ff.Name, JlString.ToLowerFirst(className)));
                });

                codeStr.AppendLine(string.Format(@"
    /**
    * 通过{3}修改实体
    * 
    * @param connectionString 连接字符串
    * @param {1} 修改实体
    * 
    * @return 修改成功与否
    */
    public static boolean UpdateBy{6}(String connectionString, {0} {1}) throws Exception {{
        String sql = ""UPDATE [{5}] SET ""
{2}
        + ""WHERE [{3}] = @{3}"";
		
        HashMap<String, Object> parameters = new HashMap<String, Object>();

{4}		
        int i = SlDatabase.executeNonQuery(connectionString, sql, parameters);
		
        return i > 0;
    }}", className,
                JlString.ToLowerFirst(className),
                field.ToString().Substring(0, field.Length - 5) + " \"",
                f.Name,
                fieldParameter,
                tableName,
                JlString.ToUpperFirst(f.Name)));
            });
            return codeStr.ToString();
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
    /**
    * 通过{3}删除实体
    * 
    * @param connectionString 连接字符串
    * @param {1} 删除属性
    * 
    * @return 修改成功与否
    */
    public static boolean DeleteBy{4}(String connectionString, {0} {1}) throws Exception {{
        String sql = ""DELETE FROM [{2}] WHERE [{3}] = @{3}"";
		
        HashMap<String, Object> parameters = new HashMap<String, Object>();

{5}		
        int i = SlDatabase.executeNonQuery(connectionString, sql, parameters);
		
        return i > 0;
    }}", JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType),
                JlString.ToLowerFirst(f.Name),
                tableName,
                f.Name,
                JlString.ToUpperFirst(f.Name),
                JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType) == "Date"
                        ? string.Format("        parameters.put(\"{0}\", new java.sql.Date({1}));", f.Name, JlString.ToLowerFirst(f.Name))
                        : string.Format("        parameters.put(\"{0}\", {1});", f.Name, JlString.ToLowerFirst(f.Name))));
            });
            return codeStr.ToString();
        }

        public static string DaoGet(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();
            var field = new StringBuilder();
            var fieldSetModel = new StringBuilder();
            inModel.FieldDescriptions.ForEach(f =>
            {
                field.AppendLine("            + \"[" + f.Name + "], \"");
                fieldSetModel.AppendLine(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType) == "Date"
                    ? string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(new Date(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType)) + "(\"" + f.Name + "\").getTime()));")
                    : string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType)) + "(\"" + f.Name + "\"));"));
            });
            inModel.FieldDescriptions.ForEach(f =>
            {
                var fieldParameter = new StringBuilder();
                fieldParameter.AppendLine(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType) == "Date"
                    ? string.Format("        parameters.put(\"{0}\", new java.sql.Date({1}.getTime()));", f.Name, JlString.ToLowerFirst(f.Name))
                    : string.Format("        parameters.put(\"{0}\", {1});", f.Name, JlString.ToLowerFirst(f.Name)));

                codeStr.AppendLine(string.Format(@"
    /**
    * 通过{1}查询实体
    * 
    * @param connectionString 连接字符串
    * @param {6} {4}
    * 
    * @return 查询结果
    */
    public static {5} GetBy{10}(String connectionString, {9} {8}) throws Exception {{
        String sql = ""SELECT ""
{2}
            + ""FROM [{0}] WITH (NOLOCK) ""
            + ""WHERE [{1}] = @{1}"";
		
        HashMap<String, Object> parameters = new HashMap<String, Object>();

{3}		
        CachedRowSet crs = SlDatabase.fill(connectionString, sql, parameters);

        while (crs.next()) {{
            {5} model = new {5}();
{7}
            return model;
        }}
        return null;
    }}", tableName, f.Name,
                field.ToString().Substring(0, field.Length - 5) + " \"",
                fieldParameter,
                f.Description,
                className,
                JlString.ToLowerFirst(className),
                fieldSetModel,
                JlString.ToLowerFirst(f.Name),
                JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType),
                JlString.ToUpperFirst(f.Name)));
            });

            return codeStr.ToString();
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
                field.AppendLine("            \"[" + f.Name + "], \" +");
                fieldSetModel.AppendLine(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType) == "Date"
                    ? string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(new Date(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType)) + "(\"" + f.Name + "\").getTime()));")
                    : string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType)) + "(\"" + f.Name + "\"));"));
            });

            Func<string, string, string> getSelectSql = (data, where) =>
            {
                return string.Format(@"""SELECT ""+
{0}
            ""FROM [{1}] WITH (NOLOCK){2}""",
                field.ToString().Substring(0, field.Length - 7) + " \" +",
                tableName,
                string.IsNullOrEmpty(where) ? "" : " " + where);
            };

            var fieldDataAccess = string.Format(@"
        List<{0}> list = new ArrayList<{0}>();

        CachedRowSet crs = SlDatabase.fill(connectionString, sql);

        while (crs.next()) {{
            {0} model = new {0}();
{1}
            
            list.add(model);
        }}
        return list;", className, fieldSetModel.ToString().Substring(0, fieldSetModel.Length - 2));

            codeStr.AppendLine(string.Format(@"
    /**
    * 查询所有实体列表（不存在时，返回null）
    * 
    * @param connectionString 连接字符串
    * 
    * @return 查询结果集
    */
    public static List<{1}> GetAll(String connectionString) throws Exception {{
        String sql = {0};

{2}
    }}", getSelectSql(field.ToString(), null), className, fieldDataAccess));

            inModel.FieldDescriptions.ForEach(f =>
            {
                var fieldParameter = JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType) == "Date"
                    ? string.Format("        parameters.put(\"{0}\", new java.sql.Date(wherePart.get{1}().getTime()));", f.Name, JlString.ToUpperFirst(f.Name))
                    : string.Format("        parameters.put(\"{0}\", wherePart.get{1}());", f.Name, JlString.ToUpperFirst(f.Name));

                var sqlWhere = string.Format("WHERE [{0}] = @{0}", f.Name);
                codeStr.AppendLine(string.Format(@"
    /**
    * 通过{3}查询实体列表（不存在时，返回null）
    * 
    * @param connectionString 连接字符串
    * @param wherePart 条件部分
    * 
    * @return 查询结果集
    */
    public static List<{1}> GetListBy{5}(String connectionString, {1} wherePart) throws Exception {{
        String sql = {0};

        HashMap<String, Object> parameters = new HashMap<String, Object>();
{2}
{4}
    }}", getSelectSql(field.ToString(), sqlWhere), className, fieldParameter, f.Name, fieldDataAccess, JlString.ToUpperFirst(f.Name)));

            });


            return codeStr.ToString();
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
                field.AppendLine("                \"[" + f.Name + "], \" +");
                fieldSetModel.AppendLine(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType) == "Date"
                    ? string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(new Date(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType)) + "(\"" + f.Name + "\").getTime()));")
                    : string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType, false, inModel.databaseType)) + "(\"" + f.Name + "\"));"));
            });

            Func<string, string, string> getSelectSql = (data, where) =>
            {
                return string.Format(@"
            ""WITH Virtual_T AS(""+
                ""SELECT ""+
{0}                ""ROW_NUMBER() OVER (%s) AS [RowNumber] "" + 
                ""FROM [{1}] WITH (NOLOCK)%s) "" +
            ""SELECT * FROM Virtual_T "" +
            ""WHERE @PageSize * (@CurrentPage - 1) < RowNumber AND RowNumber <= @PageSize * @CurrentPage""",
                field,
                tableName);
            };

            var fieldDataAccess = string.Format(@"
        List<{0}> list = new ArrayList<{0}>();

        CachedRowSet crs = SlDatabase.fill(connectionString, sql);

        while (crs.next()) {{
            {0} model = new {0}();
{1}
            
            list.add(model);
        }}
        return list;", className, fieldSetModel.ToString().Substring(0, fieldSetModel.Length - 2));

            codeStr.AppendLine(string.Format(@"
    /**
    * 通过ConditionAndOrder查询一页实体列表（不存在时，返回null）
    * 
    * @param connectionString 连接字符串
    * @param pageSize 每页行数
    * @param currentPage 当前页码
    * @param sqlWhere 判断条件语句
    * @param sqlOrder 排序语句
    * 
    * @return 查询结果集
    */
    public static List<{1}> GetPageListByConditionAndOrder(String connectionString, int pageSize, int currentPage, 
        String sqlWhere, String sqlOrder) throws Exception {{
        String sql = {0};

        // 条件查询部分
        sql = String.format(sql, sqlOrder, sqlWhere.isEmpty() ? """" : "" WHERE ("" + sqlWhere.substring(0, sqlWhere.length() - 4) + "")"");
        HashMap<String, Object> parameters = new HashMap<String, Object>();

        parameters.put(""PageSize"", pageSize);
        parameters.put(""CurrentPage"", currentPage);
{2}
    }}", getSelectSql(field.ToString(), null), className, fieldDataAccess));

            return codeStr.ToString();
        }

        public static string MybatisSelect(CodeMakerGeneratCodeOut inModel)
        {
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var result = new StringBuilder();

            if (inModel.FieldDescriptions.Any())
            {
                var field = new JlFieldDescription();
                if (inModel.FieldDescriptions.Any(f => f.ColumnKey == "PRI"))
                {
                    field = inModel.FieldDescriptions.First(f => f.ColumnKey == "PRI");
                }
                else
                {
                    field = inModel.FieldDescriptions.FirstOrDefault();
                }

                result.AppendLine(string.Format(
   @"  <select id=""{6}By{1}"" parameterType=""{4}"" resultMap=""BaseResultMap"">
    select
    <include refid=""Base_Column_List"" />
    from {0}
    where {5} = #{{{2},jdbcType={3}}}
  </select>
  <select id=""selectAll"" resultMap=""BaseResultMap"">
    select
    <include refid=""Base_Column_List"" />
    from {0}
  </select>", tableName, JlString.ToUpperFirst(field.Name), JlString.ToLowerFirst(field.Name), JlDbTypeMap.Map4Mybatis(field.DbType).ToUpper(), JlDbTypeMap.Map4J(field.DbType, false, inModel.databaseType), field.Name,
  inModel.FieldDescriptions.Any(f => f.ColumnKey == "PRI")?"select": "selectList"));
            }
            return result.ToString();
        }

        public static string MybatisUpdate(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var result = new StringBuilder();
            var field_UpdateParams = new StringBuilder();
            var field_UpdateParamsSelective = new StringBuilder();

            if (!inModel.FieldDescriptions.Any())
            {
                return string.Empty;
            }
            inModel.FieldDescriptions.Where(f => f.ColumnKey != "PRI").ToList().ForEach(f =>
            {
                field_UpdateParams.Append(string.Format("{2} = #{{{0},jdbcType={1}}}, ", JlString.ToLowerFirst(f.Name), JlDbTypeMap.Map4Mybatis(f.DbType).ToUpper(), f.Name));
                if (field_UpdateParams.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None).Last().Length > 100)
                {
                    field_UpdateParams.Append("\r\n        ");
                }

                field_UpdateParamsSelective.AppendLine(string.Format(
@"      <if test=""{0} != null"">
        {1} = #{{{0},jdbcType={2}}},
      </if>", JlString.ToLowerFirst(f.Name), f.Name, JlDbTypeMap.Map4Mybatis(f.DbType).ToUpper()));
            });

            var field = new JlFieldDescription();
            if (inModel.FieldDescriptions.Count(f => f.ColumnKey == "PRI") == 1)
            {
                field = inModel.FieldDescriptions.First(f => f.ColumnKey == "PRI");
            }
            else
            {
                field = inModel.FieldDescriptions.FirstOrDefault();
            }

            result.AppendLine(string.Format(
@"  <update id=""updateBy{7}"" parameterType=""{4}"">
    update {0}
    set {6}
    where {1} = #{{{2},jdbcType={3}}}
  </update>
  <update id=""updateBy{7}Selective"" parameterType=""{4}"">
    update {0}
    <set>
{5}    </set>
    where {1} = #{{{2},jdbcType={3}}}
  </update>", tableName, field.Name, JlString.ToLowerFirst(field.Name), JlDbTypeMap.Map4Mybatis(field.DbType).ToUpper(), string.Format(inModel.CodeMakerGeneratCodeIn.Package, "bean.") + className,
                field_UpdateParamsSelective, field_UpdateParams.ToString().TrimEnd().Substring(0, field_UpdateParams.ToString().TrimEnd().Length - 1), JlString.ToUpperFirst(field.Name)));
            return result.ToString();
        }

        public static string MybatisInsert(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var result = new StringBuilder();
            var field_Params = new StringBuilder();
            var field_InsertParams = new StringBuilder();

            if (!inModel.FieldDescriptions.Any())
            {
                return string.Empty;
            }
            inModel.FieldDescriptions.Where(f => !f.IsIdentity).ToList().ForEach(f =>
            {
                if (field_Params.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None).Last().Length > 100)
                {
                    field_Params.Append("\r\n    ");
                }
                field_Params.Append(f.Name + ", ");

                if (field_InsertParams.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None).Last().Length > 100)
                {
                    field_InsertParams.Append("\r\n    ");
                }
                field_InsertParams.Append(string.Format("#{{{0},jdbcType={1}}}, ", JlString.ToLowerFirst(f.Name), JlDbTypeMap.Map4Mybatis(f.DbType).ToUpper()));
            });

            result.AppendLine(string.Format(
@"  <insert id=""insert"" parameterType=""{3}"">
    insert into {0}(
    {1}
    ) values (
    {2})
  </insert>", tableName, field_Params.ToString().Substring(0, field_Params.Length - 2), field_InsertParams.ToString().TrimEnd().Substring(0, field_InsertParams.ToString().TrimEnd().Length - 1), string.Format(inModel.CodeMakerGeneratCodeIn.Package, "bean.") + className));
            return result.ToString();
        }

        public static string MybatisDelete(CodeMakerGeneratCodeOut inModel)
        {
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var result = new StringBuilder();

            if (!inModel.FieldDescriptions.Any())
            {
                return string.Empty;
            }
            var field = new JlFieldDescription();
            if (inModel.FieldDescriptions.Count(f => f.ColumnKey == "PRI") == 1)
            {
                field = inModel.FieldDescriptions.First(f => f.ColumnKey == "PRI");
            }
            else
            {
                field = inModel.FieldDescriptions.FirstOrDefault();
            }

            result.AppendLine(string.Format(
@"  <delete id=""deleteBy{1}"" parameterType=""{4}"">
    delete from {0}
    where {5} = #{{{2},jdbcType={3}}}
  </delete>", tableName, JlString.ToUpperFirst(field.Name), JlString.ToLowerFirst(field.Name), JlDbTypeMap.Map4Mybatis(field.DbType).ToUpper(), JlDbTypeMap.Map4J(field.DbType, false, inModel.databaseType), field.Name));

            return result.ToString();
        }

    }
}