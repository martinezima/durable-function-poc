using DurableFunctionPoC.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DurableFunctionPoC
{

    public static class ActivityFunctions
    {
        [FunctionName(nameof(IntactRunbookToProcess))]
        public static async Task<OutputResult<string>> IntactRunbookToProcess([ActivityTrigger] RunbookRequest runbook, ILogger log)
        {
            log.LogInformation($"Running Intact runbook process {runbook.JobId} = {runbook.JobName}.");
            log.LogInformation($"Starting to do it some job {runbook.DoSomeJob}. for Intact.");
            // simulate doing the activity
            await Task.Delay(5000);
            return new OutputResult<string>
            {
                HasErrors = false,
                Message = $"The runbook Job {runbook.JobName} was completed and processed to Intact system.",
                Data = "Intact additional interesting data."
            };
        }

        [FunctionName(nameof(SalesforceRunbookToProcess))]
        public static async Task<OutputResult<string>> SalesforceRunbookToProcess([ActivityTrigger] RunbookRequest runbook, ILogger log)
        {
            try
            {
                log.LogInformation($"Running Salesforce runbook process {runbook.JobId} = {runbook.JobName}.");
                if (runbook.DoSomeJob.Contains("Bad"))
                {
                    throw new InvalidOperationException($"Failed to running SalesForce process. Job: {runbook.JobName}");
                }

                log.LogInformation($"Starting to do it some job {runbook.DoSomeJob}. for Salesforce.");
                // simulate doing the activity
                await Task.Delay(5000);
                return new OutputResult<string>
                {
                    HasErrors = false,
                    Message = $"The runbook Job {runbook.JobName} was completed and processed to Salesforce system.",
                    Data = "Salesforce additional interesting data."
                };
            }
            catch (Exception e)
            {

                return new OutputResult<string>
                {
                    HasErrors = true,
                    Message = e.Message
                };
            }
        }

        [FunctionName(nameof(ConcurRunbookToProcess))]
        public static async Task<OutputResult<string>> ConcurRunbookToProcess([ActivityTrigger] RunbookRequest runbook, ILogger log)
        {
            log.LogInformation($"Running Concur runbook process {runbook.JobId} = {runbook.JobName}.");
            if (runbook.ItShouldRetry)
            {
                throw new InvalidOperationException($"Failed to running Concur process. Job: {runbook.JobName}");
            }
            // simulate doing the activity
            await Task.Delay(5000);
            return new OutputResult<string>
            {
                HasErrors = false,
                Message = $"The runbook Job {runbook.JobName} was completed and processed to Intact system.",
                Data = "Concur additional interesting data."
            };
        }
    }
}