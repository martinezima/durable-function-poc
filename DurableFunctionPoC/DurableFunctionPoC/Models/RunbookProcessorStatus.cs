﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Models
{
    public enum RunbookProcessorStatus
    {
        NoStarted, Started, Processing, Processed, Failed
    }
}
