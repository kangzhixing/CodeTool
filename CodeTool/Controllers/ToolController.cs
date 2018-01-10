using CodeTool.common;
using JasonLib;
using JasonLib.Data;
using JasonLib.Web;
using JasonLib.Web.Mvc;
using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Mvc;

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

        [ViewPage]
        [Description("匿名投票")]
        public ActionResult Vote()
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
        [Description("数据库查询")]
        public ActionResult Export()
        {
            return View();
        }

        public ActionResult GetDatas(string connection, string sql, string dbType)
        {
            sql = HttpUtility.UrlDecode(sql);
            connection = HttpUtility.UrlDecode(connection);
            if (sql.Contains("delete ") || sql.Contains("update ") || sql.Contains("drop "))
            {
                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(new { Message = "涉及敏感关键字" })
                };
            }
            var ds = new DataSet();
            try
            {
                JlDatabase.Fill(connection, sql, ds, null, (JlDatabaseType)Enum.Parse(typeof(JlDatabaseType), dbType));
            }
            catch (Exception ex)
            {
                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(new { Message = ex.Message })
                };
            }
            var result = CommonMethod.ConvertDataTableToList(ds.Tables[0], true);

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
