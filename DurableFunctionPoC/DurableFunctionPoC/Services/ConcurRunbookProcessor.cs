using DurableFunctionPoC.Interfaces;
using DurableFunctionPoC.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Services
{
    public class ConcurRunbookProcessor : IConcurRunbookProcessor
    {
        public readonly ILogger _log;
        public RunbookProcessorStatus Status { get; set; }
        public ConcurRunbookProcessor(ILogger<ConcurRunbookProcessor> log)
        {
            _log = log;
            Status = RunbookProcessorStatus.NoStarted;
        }

        public async Task<OutputResult<string>> Process(InputResult inputResult)
        {
            List<RunbookMonitoring> monitorings = new();
            try
            {
                Status = RunbookProcessorStatus.Processing;
                _log.LogInformation($"Getting configuration to use for Concur system = {inputResult.SomeConfigHere}.");
                _log.LogInformation($@"Running Concur runbook process {inputResult.Runbook.JobId} = ""{inputResult.Runbook.JobName}"".");
                // simulate doing the activity
                var message  = $@"Starting to do it some job ""{inputResult.Runbook.DoSomeJob}"" for Concur.";
                _log.LogInformation(message);
                monitorings.Add(new RunbookMonitoring { Event = "Process Started", Detail = message });
                await Task.Delay(5000);

                if (inputResult.Runbook.ThrowException)
                {
                    throw new InvalidOperationException($@"Failed to running Concur process. Job: ""{inputResult.Runbook.JobName}"".");
                }

                await DoSomeProcessForConcur();
                Status = RunbookProcessorStatus.Processed;

                message = $@"The runbook Job ""{inputResult.Runbook.JobName}"" was completed and processed to Concur system.";
                monitorings.Add(new RunbookMonitoring { Event = "Process Completed", Detail = message });

                return new OutputResult<string>
                {
                    HasErrors = false,
                    Message = $@"The runbook Job ""{inputResult.Runbook.JobName}"" was completed and processed to Intact system.",
                    ProccesedIn = ExternalSystem.Concur,
                    Data = "Concur additional interesting data.",
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
                    ProccesedIn = ExternalSystem.Concur,
                    RunbookStatus = GetStatus(),
                    RunbookMonitoring = monitorings
                };
            }
        }

        public string GetStatus()
        {
            return Status.ToString();
        }

        public async Task DoSomeProcessForConcur()
        {
            //Doing some process for Concur
            await Task.Delay(5000);
        }
    }
}
