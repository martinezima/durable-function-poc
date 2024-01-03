using DurableFunctionPoC.Interfaces;
using DurableFunctionPoC.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DurableFunctionPoC.Services
{
    public class SalesforceRunbookProcessor : ISalesforceRunbookProcessor
    {
        public readonly ILogger _log;
        public RunbookProcessorStatus Status { get; set; }
        public SalesforceRunbookProcessor(ILogger<SalesforceRunbookProcessor> log)
        {
            _log = log;
        }

        public async Task<OutputResult<string>> Process(InputResult inputResult)
        {
            List<RunbookMonitoring> monitorings = new();
            try
            {
                Status = RunbookProcessorStatus.Processing;
                _log.LogInformation($"Getting configuration to use for Salesforce system = {inputResult.SomeConfigHere}.");
                _log.LogInformation($@"Running Salesforce runbook process {inputResult.Runbook.JobId} = ""{inputResult.Runbook.JobName}"".");

                var message = $@"Starting to do it some job ""{inputResult.Runbook.DoSomeJob}"" for for Salesforce.";
                _log.LogInformation(message);
                monitorings.Add(new RunbookMonitoring { Event = "Process Started", Detail = message });
                // simulate doing the activity
                await Task.Delay(10000);
                await DoSomeProcessForSalesforce();
                Status = RunbookProcessorStatus.Processed;

                message = $@"The runbook Job ""{inputResult.Runbook.JobName}"" was completed and processed to Salesforce system.";
                monitorings.Add(new RunbookMonitoring { Event = "Process Completed", Detail = message });
                return new OutputResult<string>
                {
                    HasErrors = false,
                    Message = $@"The runbook Job ""{inputResult.Runbook.JobName}"" was completed and processed to Salesforce system.",
                    ProccesedIn = ExternalSystem.Salesforce,
                    Data = "Salesforce additional interesting data.",
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
                    ProccesedIn = ExternalSystem.Salesforce,
                    RunbookStatus = GetStatus(),
                    RunbookMonitoring = monitorings
                };
            }
        }

        public string GetStatus()
        {
            return Status.ToString();
        }

        public async Task DoSomeProcessForSalesforce()
        {
            //Doing some process for Concur
            await Task.Delay(5000);
        }
    }
}
