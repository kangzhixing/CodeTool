using JasonLib;
using JasonLib.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace CodeTool.Controllers
{
    public class BaseController : Controller
    {

        public ActionResult RedirectToErrorPage()
        {
            return RedirectToAction("Error", "Main");
        }
    }
}
