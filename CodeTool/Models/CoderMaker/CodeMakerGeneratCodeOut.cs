using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodeTool.common;

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
    }
}