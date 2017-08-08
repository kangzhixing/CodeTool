using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeTool.Models.CoderMaker
{
    public class CodeMakerGetTablesIn
    {
        /// <summary>
        /// 数据库链接
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 表或视图名
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DbType { get; set; }
    }
}