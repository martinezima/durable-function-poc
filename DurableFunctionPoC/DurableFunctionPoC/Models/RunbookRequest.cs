using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public  class RunbookRequest
    {
        public string DoSomeJob { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public bool ItShouldRetry { get; set; }
        public int TimesToRetry { get; set; }
    }
}
