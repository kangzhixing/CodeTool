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
using System.Collections.Generic;

namespace CodeTool.Controllers
{
    public class CodeMakerController : BaseController
    {
        //
        // GET: /Default/

        [ViewPage]
        [Description(".Net代码生成器")]
        public ActionResult DotnetCode()
        {
            return View();
        }

        [ViewPage]
        [Description("Java代码生成器")]
        public ActionResult JavaCode()
        {
            return View();
        }

        public ActionResult GetCodeTypeSlt(string lang)
        {
            var type = Type.GetType(JlConfig.GetValue<string>("ReflectClass") + "_" + lang);

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
                inModel.Lang = HttpUtility.UrlDecode(inModel.Lang);
                inModel.ClassName = HttpUtility.UrlDecode(inModel.ClassName);
                inModel.ConnectionString = HttpUtility.UrlDecode(inModel.ConnectionString);
                inModel.Package = HttpUtility.UrlDecode(inModel.Package);
                inModel.Table = HttpUtility.UrlDecode(inModel.Table);
                inModel.DbType = HttpUtility.UrlDecode(inModel.DbType);

                var databaseColumns = CommonMethod.GetDatabaseColumns(inModel.ConnectionString, inModel.Table, (JlDatabaseType)Enum.Parse(typeof(JlDatabaseType), inModel.DbType));

                var outModel = new CodeMakerGeneratCodeOut
                {
                    CodeMakerGeneratCodeIn = inModel,
                    FieldDescriptions = databaseColumns
                };

                #region 通过反射调用方法

                var type = Type.GetType(JlConfig.GetValue<string>("ReflectClass") + "_" + inModel.Lang);
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

                var databaseTables = CommonMethod.GetDatabaseTables(inModel.ConnectionString, (JlDatabaseType)Enum.Parse(typeof(JlDatabaseType), inModel.DbType));

                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(databaseTables)
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
