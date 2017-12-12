using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CodeTool.common
{
    /// <summary>
    /// 权限拦截
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (this.AuthorizeCore(filterContext) == false) //根据验证判断进行处理
            {
                filterContext.Result = new ContentResult { Content = "<script type = 'text/javascript'> alert('您没有该页面权限！');history.go(-1); </script>" };
                //filterContext.Result = new HttpUnauthorizedResult(); //直接URL输入的页面地址跳转到登陆页
            }
        }
        //权限判断业务逻辑
        protected virtual bool AuthorizeCore(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            var host = filterContext.HttpContext.Request.UserHostAddress;
            var path = filterContext.HttpContext.Request.Path.ToLower();
            //需要验证的页面链接
            var authorizePageList = new List<string>() { "/test/lakalarefund", "/test/lakalaconfirm" };
            if (authorizePageList.Contains(path))
            {
                var ipList = new List<string>() {
                    "127.0.0.1",
                    "10.2.131.161",
                    "10.2.131.165" };

                return ipList.Contains(host);
            }
            else
            {
                return true;
            }
        }
    }
}