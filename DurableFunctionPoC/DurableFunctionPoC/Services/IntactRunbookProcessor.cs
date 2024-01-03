using DurableFunctionPoC.Interfaces;
using DurableFunctionPoC.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Services
{
    public class IntactRunbookProcessor : IIntactRunbookProcessor
    {
        public readonly ILogger _log;
        public RunbookProcessorStatus Status { get; set; }
        public IntactRunbookProcessor(ILogger<IntactRunbookProcessor> log)
        {
            _log = log;
        }

        public async Task<OutputResult<string>> Process(InputResult inputResult)
        {
            List<RunbookMonitoring> monitorings = new();
            try
            {
                Status = RunbookProcessorStatus.Processing;
                _log.LogInformation($"Getting configuration to use for Intact system = {inputResult.SomeConfigHere}.");
                _log.LogInformation($@"Running Intact runbook process {inputResult.Runbook.JobId} = ""{inputResult.Runbook.JobName}"".");
                var message  = $"Starting to do it some job {inputResult.Runbook.DoSomeJob}. for Intact.";
                _log.LogInformation(message);
                monitorings.Add(new RunbookMonitoring { Event = "Process Started", Detail = message });
                // simulate doing the activity
                await Task.Delay(5000);
                await DoSomeProcessForIntact();
                Status = RunbookProcessorStatus.Processed;

                message = $@"The runbook Job ""{inputResult.Runbook.JobName}"" was completed and processed to Intact system.";
                monitorings.Add(new RunbookMonitoring { Event = "Process Completed", Detail = message });

                return new OutputResult<string>
                {
                    HasErrors = false,
                    Message = message,
                    ProccesedIn = ExternalSystem.Intact,
                    Data = "Intact additional interesting data.",
                    RunbookStatus = GetStatus(),
                    RunbookMonitoring = monitorings
                };
            }
            catch (Exception e)
            {
                Status = RunbookProcessorStatus.Failed;
                monitorings.Add(new RunbookMonitoring { Event = "Process Failed", Detail = e.Message });
                return new OutputResult<string>
                {
                    HasErrors = true,
                    Message = e.Message,
                    ProccesedIn = ExternalSystem.Intact,
                    RunbookStatus = GetStatus(),
                    RunbookMonitoring = monitorings
                };
            }
        }

        public async Task DoSomeProcessForIntact()
        {
            //Doing some process for Intact
            await Task.Delay(5000);
        }

        public string GetStatus()
        {
            return Status.ToString();
        }
    }
}
