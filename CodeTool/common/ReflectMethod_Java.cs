﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CodeTool.Models.CoderMaker;

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

public class {1} {{", string.Format(inModel.CodeMakerGeneratCodeIn.Package, "entity"), inModel.CodeMakerGeneratCodeIn.ClassName));


            foreach (var f in inModel.FieldDescriptions)
            {
                result.AppendLine(@"
    private " + JlDbTypeMap.Map4J(f.DbType, f.IsNullable) + " " + f.Name + ";" + (string.IsNullOrWhiteSpace(f.Description) ? "" : " //" + f.Description));

                getterAndSetter.AppendLine(string.Format(@"
    public {2} get{3}() {{
        return {0};
    }}
    public void set{3}({2} {1}) {{
        this.{0} = {1};
    }}", f.Name, JlString.ToLowerFirst(f.Name), JlDbTypeMap.Map4J(f.DbType, f.IsNullable), JlString.ToUpperFirst(f.Name)));

            }
            result.Append(getterAndSetter);
            result.AppendLine("}");

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
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var field = new StringBuilder();
            var fieldValue = new StringBuilder();
            var fieldParameter = new StringBuilder();
            inModel.FieldDescriptions.Where(f => !f.IsIdentity).ToList().ForEach(f =>
            {
                field.AppendLine("            + \"[" + f.Name + "], \"");
                fieldValue.AppendLine("            + \"@" + f.Name + ", \"");
                if (JlDbTypeMap.Map4J(f.DbType) == "Date")
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
        int i = JlDatabase.executeNonQuery(connectionString, sql, parameters);
		
        return i > 0;
    }}", className, JlString.ToLowerFirst(className), field.ToString().Substring(0, field.Length - 5) + "\"", fieldValue.ToString().Substring(0, fieldValue.Length - 5), fieldParameter, tableName);
            return DaoBaseCode(inModel, codeStr);
        }

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
                        field.AppendLine("            + \"[" + ff.Name + "] = @" + ff.Name + ", \"");
                    }
                    fieldParameter.AppendLine(JlDbTypeMap.Map4J(ff.DbType) == "Date"
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
        int i = JlDatabase.executeNonQuery(connectionString, sql, parameters);
		
        return i > 0;
    }}", className,
                JlString.ToLowerFirst(className),
                field.ToString().Substring(0, field.Length - 5) + " \"",
                f.Name,
                fieldParameter,
                tableName,
                JlString.ToUpperFirst(f.Name)));
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
        int i = JlDatabase.executeNonQuery(connectionString, sql, parameters);
		
        return i > 0;
    }}", JlDbTypeMap.Map4J(f.DbType),
                JlString.ToLowerFirst(f.Name),
                tableName,
                f.Name,
                JlString.ToUpperFirst(f.Name),
                JlDbTypeMap.Map4J(f.DbType) == "Date"
                        ? string.Format("        parameters.put(\"{0}\", new java.sql.Date({1}));", f.Name, JlString.ToLowerFirst(f.Name))
                        : string.Format("        parameters.put(\"{0}\", {1});", f.Name, JlString.ToLowerFirst(f.Name))));
            });
            return DaoBaseCode(inModel, codeStr.ToString());
        }

        public string RefDaoGet(CodeMakerGeneratCodeOut inModel)
        {
            var className = inModel.CodeMakerGeneratCodeIn.ClassName;
            var tableName = inModel.CodeMakerGeneratCodeIn.Table;
            var codeStr = new StringBuilder();
            var field = new StringBuilder();
            var fieldSetModel = new StringBuilder();
            inModel.FieldDescriptions.ForEach(f =>
            {
                field.AppendLine("            + \"[" + f.Name + "], \"");
                fieldSetModel.AppendLine(JlDbTypeMap.Map4J(f.DbType) == "Date"
                    ? string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(new Date(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType)) + "(\"" + f.Name + "\").getTime()));")
                    : string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType)) + "(\"" + f.Name + "\"));"));
            });
            inModel.FieldDescriptions.ForEach(f =>
            {
                var fieldParameter = new StringBuilder();
                fieldParameter.AppendLine(JlDbTypeMap.Map4J(f.DbType) == "Date"
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
        CachedRowSet crs = JlDatabase.fill(connectionString, sql, parameters);

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
                JlDbTypeMap.Map4J(f.DbType),
                JlString.ToUpperFirst(f.Name)));
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
                field.AppendLine("            \"[" + f.Name + "], \" +");
                fieldSetModel.AppendLine(JlDbTypeMap.Map4J(f.DbType) == "Date"
                    ? string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(new Date(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType)) + "(\"" + f.Name + "\").getTime()));")
                    : string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType)) + "(\"" + f.Name + "\"));"));
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

        CachedRowSet crs = JlDatabase.fill(connectionString, sql);

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
                var fieldParameter = JlDbTypeMap.Map4J(f.DbType) == "Date"
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
                field.AppendLine("                \"[" + f.Name + "], \" +");
                fieldSetModel.AppendLine(JlDbTypeMap.Map4J(f.DbType) == "Date"
                    ? string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(new Date(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType)) + "(\"" + f.Name + "\").getTime()));")
                    : string.Format("            model.set" + JlString.ToUpperFirst(f.Name) + "(crs.get" + JlString.ToUpperFirst(JlDbTypeMap.Map4J(f.DbType)) + "(\"" + f.Name + "\"));"));
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

        CachedRowSet crs = JlDatabase.fill(connectionString, sql);

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


            return DaoBaseCode(inModel, codeStr.ToString());
        }

        #endregion
    }
}