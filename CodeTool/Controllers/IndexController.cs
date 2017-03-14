using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using CodeTool.common;
using JasonLib;
using WebGrease.Css.Extensions;
using CodeTool.Models.Index;
using SoufunLab.Framework.Web.Mvc;

namespace CodeTool.Controllers
{
    public class IndexController : BaseController
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GoPageByName()
        {
            var inModel = new IndexGoPageByName();
            UpdateModel(inModel);

            try
            {
                var methods = CommonMethod.GetAllPageClass();
                foreach (var method in methods.Where(m => inModel.PageName.ToLower() == m.Name.ToLower()))
                    //foreach (var method in methods.Where(m => JlString.GetSimilarityPrecent(inModel.PageName.ToLower(), m.Name.ToLower()) > 0.5))
                {
                    Response.Redirect(CommonMethod.GetUrlByControllerMethod(method));
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }


    }
}
