﻿using DurableFunctionPoC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Interfaces
{
    public interface ISomeRunbookProcessor
    {
        void Process(InputResult inputResult);
        RunbookProcessorStatus GetStatus();
    }
}