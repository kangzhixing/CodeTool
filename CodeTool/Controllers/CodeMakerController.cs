using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeTool.common;
using CodeTool.Models.CoderMaker;
using JasonLib.Web;
using Mo = CodeTool.Models;
using JasonLib;
using JasonLib.Web.Mvc;
using JasonLib.Data;

namespace CodeTool.Controllers
{
    public class CodeMakerController : BaseController
    {
        //
        // GET: /Default/

        [ViewPage]
        [Description("Java代码生成器")]
        public ActionResult JavaCode()
        {
            return View();
        }

        public ActionResult GetCodeTypeSlt()
        {
            var type = Type.GetType(JlConfig.GetValue<string>("ReflectClass"));
            
            return new JlJsonResult()
            {
                Content = JlJson.ToJson(type.GetMethods().Where(m => m.Name.StartsWith("Ref")).Select(m => m.Name).ToList())
            };
        }

        public ActionResult GeneratCode()
        {
            var inModel = new CodeMakerGeneratCodeIn();
            UpdateModel(inModel);
            try
            {
                inModel.ClassName = HttpUtility.UrlDecode(inModel.ClassName);
                inModel.ConnectionString = HttpUtility.UrlDecode(inModel.ConnectionString);
                inModel.Package = HttpUtility.UrlDecode(inModel.Package);
                inModel.Table = HttpUtility.UrlDecode(inModel.Table);


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
		                    ISNULL(G.[VALUE],'') AS Description
	                    FROM  SYSCOLUMNS  A LEFT JOIN SYSTYPES B
	                    ON  A.XTYPE=B.XUSERTYPE
	                    INNER JOIN SYSOBJECTS D
	                    ON A.ID=D.ID  AND  D.XTYPE IN ('V','U') AND  D.NAME<>'DTPROPERTIES'
	                    LEFT JOIN SYSCOMMENTS E
	                    ON A.CDEFAULT=E.ID
	                    LEFT JOIN SYS.EXTENDED_PROPERTIES G
	                    ON A.ID=G.MAJOR_ID AND A.COLID = G.MINOR_ID
                    )
                    SELECT Name,DbType,Length,IsNullable,IsIdentity,Description FROM T WHERE TableName = '{0}'";
                sql = string.Format(sql, inModel.Table);

                var dataTable = new DataTable();
                JlDatabase.Fill(inModel.ConnectionString, sql, dataTable);

                var fieldDescriptions = dataTable.AsEnumerable().Select(row => new JlFieldDescription()
                {
                    Name = row.Field<string>("Name"),
                    DbType = row.Field<string>("DbType"),
                    Length = row.Field<int>("Length"),
                    IsNullable = Convert.ToBoolean(row.Field<string>("IsNullable")),
                    IsIdentity = Convert.ToBoolean(row.Field<string>("IsIdentity")),
                    Description = HttpUtility.HtmlEncode(row.Field<string>("Description"))
                }).ToList();

                var outModel = new CodeMakerGeneratCodeOut
                {
                    CodeMakerGeneratCodeIn = inModel,
                    FieldDescriptions = fieldDescriptions
                };

                #region 通过反射调用方法
                var type = Type.GetType(JlConfig.GetValue<string>("ReflectClass"));
                //声明创建当前类实例
                var model = Activator.CreateInstance(type);
                var method = type.GetMethod(inModel.Type);

                var result = method.Invoke(model, new object[] { outModel }).ToString();


                #endregion

                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(result)
                };
            }
            catch (Exception ex)
            {
                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(new { Message = ex.Message })
                };
            }


            //return View("/Views/CodeMaker/CodeMakerResult.cshtml", new Mo.CoderMaker.CodeMakerGeneratCodeOut { CodeMakerGeneratCodeIn = inModel, FieldDescriptions = fieldDescriptions });

        }

        public ActionResult GetTables()
        {
            ContentResult actionResult;

            //获得传入数据
            var inModel = new Mo.CoderMaker.CodeMakerGetTablesIn();

            try
            {
                UpdateModel(inModel);
                inModel.ConnectionString = HttpUtility.UrlDecode(inModel.ConnectionString);

                #region 参数验证
                {

                }
                #endregion

                var sql = @"SELECT name AS Name FROM SYSOBJECTS WHERE XTYPE IN ('V','U') AND NAME<>'DTPROPERTIES' ORDER BY Name ASC";

                var dataTable = new DataTable();
                JlDatabase.Fill(inModel.ConnectionString, sql, dataTable);

                var fields = dataTable.AsEnumerable().Select(row => new
                {
                    Name = row.Field<string>("Name")
                }).ToList();

                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(fields)
                };
            }
            catch (Exception exception)
            {
                //构造异常结果
                actionResult = new JlJsonResult()
                {
                    Content = JlJson.ToJson(new { Message = exception.Message })
                };
            }

            return actionResult;
        }



    }
}
