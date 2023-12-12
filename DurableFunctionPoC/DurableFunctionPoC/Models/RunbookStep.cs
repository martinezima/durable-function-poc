using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public  class RunbookStep
    {
        public string OrchestrationId { get; set; }
        public string Message { get; set; }

    }
}
