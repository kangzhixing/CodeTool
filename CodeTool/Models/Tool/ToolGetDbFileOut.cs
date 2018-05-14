using CodeTool.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeTool.Models.Tool
{
    public class ToolGetDbFileOut
    {
        public string databaseName { get; set; }

        public List<JlFieldDescription> fieldDescriptions;

        public ToolGetDbFileOut(string databaseName, List<JlFieldDescription> fieldDescriptions) {
            this.databaseName = databaseName;
            this.fieldDescriptions = fieldDescriptions;
        }

    }
}