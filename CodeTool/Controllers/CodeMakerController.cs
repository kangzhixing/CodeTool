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
using System.IO;
using NPOI.OpenXml4Net.OPC;

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

        public void DownloadAllFile()
        {
            var inModel = new CodeMakerGeneratCodeIn();
            UpdateModel(inModel);

            inModel.Lang = HttpUtility.UrlDecode(inModel.Lang);
            inModel.ClassName = HttpUtility.UrlDecode(inModel.ClassName);
            inModel.ConnectionString = HttpUtility.UrlDecode(inModel.ConnectionString);
            inModel.Package = HttpUtility.UrlDecode(inModel.Package);
            inModel.DbType = HttpUtility.UrlDecode(inModel.DbType);
            inModel.Extension = HttpUtility.UrlDecode(inModel.Extension);

            var databaseTables = CommonMethod.GetDatabaseTables(inModel.ConnectionString, (JlDatabaseType)Enum.Parse(typeof(JlDatabaseType), inModel.DbType));

            var zipDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + JlConfig.GetValue<string>("SaveFilePath") + "zip\\";
            var zipFileName = JlGuid.NewGuid();
            var pathName = zipDir + zipFileName + "\\";

            if (databaseTables.Any())
            {
                if (!Directory.Exists(zipDir))
                {
                    Directory.CreateDirectory(zipDir);
                }

                Directory.CreateDirectory(pathName);
            }

            databaseTables.ForEach(table =>
            {
                var databaseColumns = CommonMethod.GetDatabaseColumns(inModel.ConnectionString, table, (JlDatabaseType)Enum.Parse(typeof(JlDatabaseType), inModel.DbType));

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

                //下载文件到文件夹内
                var fileName = string.Empty;

                if (inModel.Lang.ToLower() == "dotnet")
                {
                    if (inModel.Type.ToLower().StartsWith("dao"))
                    {
                        fileName = table + "Dao.cs";
                    }
                    else
                    {
                        fileName = table + ".cs";
                    }
                }
                else
                {
                    if (inModel.Type.ToLower().EndsWith("xml"))
                    {
                        fileName = table + "Mapper.xml";
                    }
                    else if (inModel.Type.ToLower().EndsWith("mapper"))
                    {
                        fileName = table + "Mapper.java";
                    }
                    else if (inModel.Type.ToLower().StartsWith("dao"))
                    {
                        fileName = table + "Dao.java";
                    }
                    else
                    {
                        fileName = table + ".java";
                    }
                }

                var fullName = pathName + fileName;
                if (!System.IO.File.Exists(fullName))
                {
                    System.IO.File.WriteAllText(fullName, result);
                }
            });

            if (databaseTables.Any())
            {
                ZipUtil.ZipDirectory(pathName, zipDir, zipFileName);
                new DirectoryInfo(pathName).Delete(true);
            }
            FileStream fis = new FileStream(zipDir + zipFileName + ".zip", FileMode.Open);
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/x-zip-compressed";
            Response.AddHeader("content-disposition", "attachment;filename=" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip");
            byte[] pReadByte = new byte[0];
            BinaryReader r = new BinaryReader(fis);
            r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开
            pReadByte = r.ReadBytes((int)r.BaseStream.Length);
            Response.BinaryWrite(pReadByte);
            Response.Flush();
            Response.End();

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

        public void DownloadFile(string content, string fileName, string extension)
        {
            fileName = string.IsNullOrEmpty(extension) ? DateTime.Now.ToString("yyyyMMddHHmmss") : fileName;
            extension = string.IsNullOrEmpty(extension) ? "txt" : extension;
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + fileName + "." + extension);
            System.Web.HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            System.Web.HttpContext.Current.Response.BinaryWrite(HttpUtility.UrlDecodeToBytes(content));
            //ep.SaveAs(Response.OutputStream); 第二种方式  
            System.Web.HttpContext.Current.Response.Flush();
            System.Web.HttpContext.Current.Response.End();
        }

    }
}
