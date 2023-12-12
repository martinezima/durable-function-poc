using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public  class ApprovalInfo
    {
        public string RunbookId { get; set; }
        public string OrchestrationId { get; set; }
    }
}
