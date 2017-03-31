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

        public bool InsertErrorLog(string message, string stackTrace, string details = null)
        {
            var sql = @"INSERT INTO [ErrorLog]( [Id],[Message],[StackTrace],[Details],[CreateTime]
                    ) VALUES (@Id,@Message,@StackTrace,@Details,GETDATE())";


            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() { ParameterName = "@Message", Value = message });
            parameters.Add(new SqlParameter() { ParameterName = "@StackTrace", Value = stackTrace });
            parameters.Add(new SqlParameter() { ParameterName = "@Details", Value = details });

            int i = JlDatabase.ExecuteNonQuery(localConnectionString, sql, parameters.ToArray());
            return i > 0;

        }

        public bool InsertErrorLog(Exception ex, string details = null)
        {
            return InsertErrorLog(ex.Message, ex.StackTrace, details);
        }
    }
}
