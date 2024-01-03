using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public  class OutputResult<T>
    {

        public bool HasErrors { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public ExternalSystem ProccesedIn{ get; set; }
        public string RunbookStatus { get; set; }
        public List<RunbookMonitoring> RunbookMonitoring { get; set; }
    }
}
