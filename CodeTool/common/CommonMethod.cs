using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using JasonLib;
using WebGrease.Css.Extensions;

namespace CodeTool.common
{
    public class CommonMethod
    {
        public static Dictionary<string, MethodInfo> GetAllPageMethod()
        {
            var parentType = Type.GetType(JlConfig.GetValue<string>("BaseController"));
            var controllers = Assembly.GetAssembly(parentType).GetTypes().Where(t => t.BaseType.Name == "BaseController").ToArray();

            var pageClass = new Dictionary<string, MethodInfo>();
            controllers.ForEach(c =>
            {
                c.GetMethods().Where(t =>
                    t.GetCustomAttributes(typeof(ViewPageAttribute), true).Length > 0).ForEach(m =>
                    {
                        pageClass.Add(GetMethodDescription(m).ToLower(), m);
                    });
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

        public static string GetMethodDescription(MethodInfo method)
        {
            var descriptionAttributes = method.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (descriptionAttributes.Length > 0)
            {
                return ((DescriptionAttribute)descriptionAttributes.First()).Description;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}