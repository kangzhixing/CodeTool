using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeTool.Models.CoderMaker
{
    public class CodeMakerGeneratCodeIn
    {
        /// <summary>
        /// 数据库链接
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 表或视图名
        /// </summary>
        /// 
        public string Table { get; set; }

        /// <summary>
        /// 包
        /// </summary>
        public string Package { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
    }
}