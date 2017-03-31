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
    public class MainController : BaseController
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult SearchByName()
        {
            var inModel = new IndexSearchByNameIn();
            UpdateModel(inModel);
            var keyword = inModel.PageName.ToLower();

            var cache = JlHttpCache.Current;
            var pages = cache.Get<Dictionary<string, MethodInfo>>("pages");

            if (pages == null)
            {
                pages = CommonMethod.GetAllPageMethod();
                cache.Set("pages", pages);
            }

            if (keyword == "kzx")
            {
                return new JlJsonResult()
                {
                    Content = JlJson.ToJson("/Main/SearchPagesByKeyword?isall=1")
                };
            }
            if (pages.Count(p => p.Key.Contains(keyword)) == 1)
            {
                return new JlJsonResult()
                {
                    Content = JlJson.ToJson(CommonMethod.GetUrlByControllerMethod(pages.Single(p => p.Key.Contains(keyword)).Value))
                };
            }
            else if (pages.Any(m => m.Key.Contains(keyword)))
            {
                return new JlJsonResult()
                {
                    Content = JlJson.ToJson("/Main/SearchPagesByKeyword?keyword=" + HttpUtility.HtmlEncode(keyword))
                };
            }

            return new JlJsonResult()
            {
                Content = JlJson.ToJson(new
                {
                    Message = "empty"
                })
            };

        }

        public ActionResult SearchPagesByKeyword(string keyword, int isall = 0)
        {
            var outModel = new IndexSearchPagesOut();

            try
            {
                var cache = JlHttpCache.Current;
                var allPages = cache.Get<Dictionary<string, MethodInfo>>("pages");

                var result = allPages.Where(m => isall == 1 || m.Key.Contains(keyword)).ToDictionary(m => m.Key, m => m.Value);

                if (result.Count > 0)
                {
                    outModel.MethodList = result.Values.ToList();
                }

            }
            catch (Exception ex)
            {
                return RedirectToErrorPage();
            }
            return View("/Views/Main/SearchPage.cshtml", outModel);
        }


    }
}
