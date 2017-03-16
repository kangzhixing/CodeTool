using System;
using System.Collections.Generic;
using System.Linq;
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
