using System;
using System.ComponentModel;
using System.Data;
using System.Web.Mvc;
using JasonLib;
using JasonLib.Data;
using JasonLib.Web.Mvc;

namespace CodeTool.Controllers
{
    public class TestController : BaseController
    {
        //
        // GET: /Default/
        private string localConnectionString = "data source=.;database=CodeTool;uid=sa;pwd=admin;";

        //[ViewPage]
        //[Description("测试 - 订单状态查询")]
        //public ActionResult POS()
        //{
        //    return View();
        //}

        public ActionResult DBTest()
        {
            try
            {
                var sql = @"INSERT INTO Test VALUES('" + JlGuid.NewGuid() + "','" + DateTime.Now.ToUniversalTime() + @"',getdate())
                        select top 1 name from Test with(nolock) order by CreateTime desc";
                var dataTable = new DataTable();
                JlDatabase.Fill(localConnectionString, sql, dataTable);

                return new JlJsonResult()
                {
                    Content = dataTable.Rows[0][0].ToString()
                };
            }
            catch (Exception ex)
            {
                InsertErrorLog(ex, Request.Url.OriginalString);
                return new JlJsonResult()
                {
                    Content = ex.Message
                };
            }
        }

        public ActionResult lockdb()
        {
            try
            {
                var sql = "select * from Test where id like '%a%' or id like '%b%'";
                var dataTable = new DataTable();
                JlDatabase.Fill(localConnectionString, sql, dataTable);

                return new JlJsonResult()
                {
                    Content = dataTable.Rows[0][0].ToString()
                };
            }
            catch (Exception ex)
            {
                InsertErrorLog(ex, Request.Url.OriginalString);
                return new JlJsonResult()
                {
                    Content = ex.Message
                };
            }
        }

        public ActionResult nolockdb()
        {
            try
            {
                var sql = "select * from Test with(nolock) where id like '%a%' or id like '%b%'";
                var dataTable = new DataTable();
                JlDatabase.Fill(localConnectionString, sql, dataTable);

                return new JlJsonResult()
                {
                    Content = dataTable.Rows[0][0].ToString()
                };
            }
            catch (Exception ex)
            {
                InsertErrorLog(ex, Request.Url.OriginalString);
                return new JlJsonResult()
                {
                    Content = ex.Message
                };
            }
        }

        //public ActionResult SearchPayNo()
        //{
        //    var payno = Request["payno"];
        //    if (string.IsNullOrWhiteSpace(Request["payno"]))
        //    {
        //        return new JlJsonResult()
        //        {
        //            Content = JlJson.ToJson(new { Message = "订单号为空" })
        //        };
        //    }
        //    try
        //    {
        //        var tableName = string.Empty;
        //        switch (payno[0])
        //        {
        //            case '7':
        //                tableName = "EsfPay";
        //                break;
        //            case '9':
        //                tableName = "AuctionPay";
        //                break;
        //            default:
        //                {
        //                    return new JlJsonResult()
        //                    {
        //                        Content = JlJson.ToJson(new { Message = "订单号: " + payno + "有误" })
        //                    };
        //                }
        //        }
        //        var sql = @"select * from " + tableName + " where payno in ('" + payno + "')";

        //        var dataTable = new DataTable();
        //        JlDatabase.Fill(connectionString, sql, dataTable);

        //        var payStatus = dataTable.Rows[0]["PayStatus"];
        //        sql = @"select * from AcceptThird_Log where logtext like '%" + payno + "%'";
        //        dataTable = new DataTable();
        //        JlDatabase.Fill(connectionString, sql, dataTable);

        //        var logText = dataTable.Rows[0]["LogText"];
        //        return new JlJsonResult()
        //        {
        //            Content = JlJson.ToJson(new { PayStatus = payStatus, LogText = logText })
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JlJsonResult()
        //        {
        //            Content = JlJson.ToJson(new { Message = ex.Message })
        //        };
        //    }
        //}

        //[ViewPage]
        //[Description("测试 - 表单提交")]
        //public ActionResult FormSubmit()
        //{
        //    return View();
        //}

    }
}
