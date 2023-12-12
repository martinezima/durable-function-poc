using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public  class InputResult
    {
        public RunbookRequest Runbook { get; set; }
        public string SomeConfigHere { get; set; }
        public string OrchestrationId { get; set; }
        public string Message { get; set; }

    }
}
