using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public  class ApprovalOutput
    {
        public bool IsApproved { get; set; }
        public string SomeMoreRelevantInfo { get; set; }
        public string ApprovalStatus { get; set; }

    }
}
