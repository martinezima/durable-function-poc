using Azure.Storage.Blobs.Models;
using DurableFunctionPoC.Interfaces;
using DurableFunctionPoC.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Services
{
    public class SomeRunbookProcessor: ISomeRunbookProcessor
    {
        //This is just for demo , you will need 
        private static readonly List<RunbookProcessorStatus> State = new ();
        public SomeRunbookProcessor()
        {
        }

        public RunbookProcessorStatus Status { get; set; }
        public void Process(InputResult inputResult)
        {
            State.Add(RunbookProcessorStatus.NoStarted);
        }

        public RunbookProcessorStatus GetStatus()
        {
            // This is for demo purpose implementation.
            // FOR PRODUCTION, USE DURABLE ENTITY TO STORE THE STATE.
            static RunbookProcessorStatus checkStatus(RunbookProcessorStatus oldvalue)
            {
                switch (oldvalue)
                {
                    case RunbookProcessorStatus.NoStarted:
                        return RunbookProcessorStatus.Processing;
                    case RunbookProcessorStatus.Processing:
                        return RunbookProcessorStatus.Processed;
                    default:
                        return RunbookProcessorStatus.Failed;
                }
            }
            State.Add(checkStatus(State.Last()));

            return State.Last();
        }
    }
}
