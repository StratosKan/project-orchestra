using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_orchestra.Models.Orchestration
{
    public class AzureOrchestrationCommand
    {
        public string FunctionName {  get; set; }
        public string FunctionVersion { get; set; } = string.Empty;
        public dynamic Payload { get; set; }
    }
}
