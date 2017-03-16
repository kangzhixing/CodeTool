using System;
using System.ComponentModel;
using System.Data;
using System.Web.Mvc;
using CodeTool.common;
using SoufunLab.Framework.Data;
using SoufunLab.Framework.Web;
using SoufunLab.Framework.Web.Mvc;

namespace CodeTool.Controllers
{
    public class TestController : BaseController
    {
        //
        // GET: /Default/
        private string connectionString = "data source=124.251.46.179;database=Pay_test;uid=Pay_test_admin;pwd=E792dF8e;";

        [ViewPage]
        [Description("测试 - 订单状态查询")]
        public ActionResult POS()
        {
            return View();
        }

        public ActionResult SearchPayNo()
        {
            var payno = Request["payno"];
            if (string.IsNullOrWhiteSpace(Request["payno"]))
            {
                return new SlJsonResult()
                {
                    Content = SlJson.ToJson(new { Message = "订单号为空" })
                };
            }
            try
            {
                var tableName = string.Empty;
                switch (payno[0])
                {
                    case '7':
                        tableName = "EsfPay";
                        break;
                    case '9':
                        tableName = "AuctionPay";
                        break;
                    default:
                        {
                            return new SlJsonResult()
                            {
                                Content = SlJson.ToJson(new { Message = "订单号: " + payno + "有误" })
                            };
                        }
                }
                var sql = @"select * from " + tableName + " where payno in ('" + payno + "')";

                var dataTable = new DataTable();
                SlDatabase.Fill(connectionString, sql, dataTable);

                var payStatus = dataTable.Rows[0]["PayStatus"];
                sql = @"select * from AcceptThird_Log where logtext like '%" + payno + "%'";
                dataTable = new DataTable();
                SlDatabase.Fill(connectionString, sql, dataTable);

                var logText = dataTable.Rows[0]["LogText"];
                return new SlJsonResult()
                {
                    Content = SlJson.ToJson(new { PayStatus = payStatus, LogText = logText })
                };
            }
            catch (Exception ex)
            {
                return new SlJsonResult()
                {
                    Content = SlJson.ToJson(new { Message = ex.Message })
                };
            }
        }

        [ViewPage]
        [Description("测试 - 表单提交")]
        public ActionResult FormSubmit()
        {
            return View();
        }

    }
}
