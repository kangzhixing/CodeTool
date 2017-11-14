using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using CodeTool.common;
using JasonLib.Web.Mvc;
using JasonLib.Web;
using System.Text;
using System.IO;
using JasonLib;
using JasonLib.Data;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace CodeTool.Controllers
{
    public class ToolController : BaseController
    {

        [ViewPage]
        [Description("MD5编码")]
        public ActionResult Md5()
        {
            return View();
        }

        [ViewPage]
        [Description("GUID字符串")]
        public ActionResult Guid()
        {
            return View();
        }

        [ViewPage]
        [Description("RGB颜色值")]
        public ActionResult Color()
        {
            return View();
        }

        [ViewPage]
        [Description("字符编码")]
        public ActionResult Encode()
        {
            return View();
        }

        public ActionResult EncodeString(string str, string encoding)
        {
            try
            {
                var result = HttpUtility.UrlEncode(str, Encoding.GetEncoding(encoding));

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

        public ActionResult DecodeString(string str, string encoding)
        {
            try
            {
                var result = HttpUtility.UrlDecode(str, Encoding.GetEncoding(encoding));

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

        [ViewPage]
        [Description("文字朗读")]
        public ActionResult Speech()
        {
            return View();
        }

        public ActionResult GetAudio(string tex, int per)
        {
            var speech = new Speech();

            var fileName = speech.Tts(HttpUtility.UrlDecode(tex), per);

            return new JlJsonResult()
            {
                Content = JlJson.ToJson(JlConfig.GetValue<string>("SaveFilePath") + fileName)
            };
        }

        [ViewPage]
        [Description("Excel导出")]
        public ActionResult Export()
        {
            return View();
        }

        public ActionResult GetDatas(string connection, string sql, string dbType)
        {
            sql = HttpUtility.UrlDecode(sql);
            if (sql.Contains("delete ") || sql.Contains("update ") || sql.Contains("drop "))
            {
                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(new { Message = "涉及敏感关键字" })
                };
            }
            var ds = new DataSet();
            JlDatabase.Fill(HttpUtility.UrlDecode(connection), HttpUtility.UrlDecode(sql), ds, null, (JlDatabaseType)Enum.Parse(typeof(JlDatabaseType), dbType));

            var result = new List<object>();

            var rowHeader = new List<object>();
            for (var i = 0; i < ds.Tables[0].Columns.Count; i++)
            {
                rowHeader.Add(ds.Tables[0].Columns[i].ToString());
            }
            result.Add(rowHeader);

            for (var j = 0; j < ds.Tables[0].Rows.Count && j < 100; j++)
            {
                var row = new List<object>();
                for (var k = 0; k < ds.Tables[0].Columns.Count; k++)
                {
                    switch (ds.Tables[0].Columns[k].DataType.Name.ToLower())
                    {
                        case "string":
                            row.Add(ds.Tables[0].Rows[j][k].ToString());
                            break;
                        case "bool":
                            row.Add(JlConvert.TryToBool(ds.Tables[0].Rows[j][k]));
                            break;
                        case "int32":
                        case "decimal":
                            row.Add(JlConvert.TryToDouble(ds.Tables[0].Rows[j][k]));
                            break;
                        default:
                            row.Add(ds.Tables[0].Rows[j][k].ToString());
                            break;
                    }
                }
                result.Add(row);
            }

            return new JlJsonResult()
            {
                Content = JlJson.ToJson(result)
            };
        }

        public void Export2File(string connection, string sql, string dbType, string excelType, string fileName)
        {
            var ds = new DataSet();
            JlDatabase.Fill(HttpUtility.UrlDecode(connection), HttpUtility.UrlDecode(sql), ds, null, (JlDatabaseType)Enum.Parse(typeof(JlDatabaseType), dbType));

            var wbType = JlWorkbookType.HSSF;
            if (excelType == "xlsx")
                wbType = JlWorkbookType.XSSF;
            else if (excelType == "xls" && ds.Tables[0].Rows.Count > 65535)
            {
                throw new Exception("xls格式限制单sheet数据不超过65535行");
            }
            var wb = JlExcel.ExportToWorkbook(ds, wbType);

            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + (string.IsNullOrEmpty(fileName) ? DateTime.Now.ToString("yyyyMMddHHmmss") : fileName) + "." + excelType);
            System.Web.HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            wb.Write(System.Web.HttpContext.Current.Response.OutputStream);
            //ep.SaveAs(Response.OutputStream); 第二种方式  
            System.Web.HttpContext.Current.Response.Flush();
            System.Web.HttpContext.Current.Response.End();

        }

    }
}
