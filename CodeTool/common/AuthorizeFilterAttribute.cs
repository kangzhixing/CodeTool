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

            if (!AdminDic.Contains(host) && AuthorizeDic.ContainsKey(path))
            {
                return AuthorizeDic[path].Contains(host);
            }
            else
            {
                return true;
            }
        }

        public static Dictionary<string, List<string>> AuthorizeDic = new Dictionary<string, List<string>> {
            {
                "/test/lakalarefund", new List<string>(){
                    "10.2.131.155", //韩倩
                    "10.2.131.165" //林雪晴
                }
            },
            { "/test/lakalaconfirm", new List<string>(){
                    "10.2.131.155",
                    "10.2.131.165"
                }
            },
            { "/test/bizstatement", new List<string>(){
                    "10.2.131.155"
                }
            }
        };

        public static List<string> AdminDic = new List<string> {
                    "127.0.0.1",
                    "10.2.131.161",
        };
    }
}