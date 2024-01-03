using DurableFunctionPoC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Interfaces
{
    public interface IRunbookProcessor
    {
        Task<OutputResult<string>> Process(InputResult inputResult);
        string GetStatus();
    }
}
