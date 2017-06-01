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

namespace CodeTool.Controllers
{
    public class ToolController : BaseController
    {
        //
        // GET: /Default/

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


    }
}
