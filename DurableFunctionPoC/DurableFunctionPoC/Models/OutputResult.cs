using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public  class OutputResult<T>
    {
        /// <summary>
        /// Status of current operation
        /// </summary>
        public bool HasErrors { get; set; }
        /// <summary>
        /// In case of error, Error Message will be on this property
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Data Result from succesfull operation
        /// </summary>
        public T Data { get; set; }
    }
}
