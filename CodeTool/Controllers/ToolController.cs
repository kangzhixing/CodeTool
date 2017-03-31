using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using CodeTool.common;
using JasonLib;
using WebGrease.Css.Extensions;
using CodeTool.Models.Index;
using JasonLib.Web;
using JasonLib.Web.Mvc;
using SoufunLab.Framework.Web.Mvc;

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


    }
}
