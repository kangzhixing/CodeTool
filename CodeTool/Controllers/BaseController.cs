using JasonLib;
using JasonLib.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeTool.Controllers
{
    public class BaseController : Controller
    {
        private string localConnectionString = "data source=.;database=CodeTool;uid=sa;pwd=admin;";

        public ActionResult RedirectToErrorPage()
        {
            return RedirectToAction("Error", "Main");
        }
    }
}
