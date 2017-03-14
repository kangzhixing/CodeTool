using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using WebGrease.Css.Extensions;

namespace CodeTool.common
{
    public class CommonMethod
    {
        public static List<MethodInfo> GetAllPageClass()
        {
            var parentType = Type.GetType("CodeTool.Controllers.BaseController");
            var controllers = Assembly.GetAssembly(parentType).GetTypes().Where(t => t.BaseType.Name == "BaseController").ToArray();

            var pageClass = new List<MethodInfo>();
            controllers.ForEach(c =>
            {
                pageClass.AddRange(c.GetMethods().Where(t =>
                   t.GetCustomAttributes(typeof(DescriptionAttribute), true).Length > 0
                ));
            });

            return pageClass;
        }

        public static string GetUrlByControllerMethod(MethodInfo method)
        {
            var parentType = method.DeclaringType;
            var result = new StringBuilder();
            var dir = parentType.FullName.Split('.').ToList();
            for (var i = dir.IndexOf("Controllers") + 1; i < dir.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(dir[i]))
                    result.Append("/" + dir[i]);
            }

            return result.ToString().Substring(0, result.Length - 10) + "/" + method.Name;
        }
    }
}