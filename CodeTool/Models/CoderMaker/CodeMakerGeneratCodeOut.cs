using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodeTool.common;
using JasonLib.Data;

namespace CodeTool.Models.CoderMaker
{
    public class CodeMakerGeneratCodeOut
    {

        /// <summary>
        /// 参数组
        /// </summary>
        public CodeMakerGeneratCodeIn CodeMakerGeneratCodeIn { get; set; }

        /// <summary>
        /// 字段列表
        /// </summary>
        public List<JlFieldDescription> FieldDescriptions { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public JlDatabaseType databaseType { get; set; }
    }
}