using DurableFunctionPoC.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DurableFunctionPoC
{

    public static class ActivityFunctions
    {

        [FunctionName(nameof(IntactRunbookToProcess))]
        public static async Task<OutputResult<string>> IntactRunbookToProcess([ActivityTrigger]        
        InputResult inputResult, ILogger log)
        {
            log.LogInformation($"Getting configuration to use for Intact system = {inputResult.SomeConfigHere}.");
            log.LogInformation($"Running Intact runbook process {inputResult.Runbook.JobId} = \"{inputResult.Runbook.JobName}\".");
            log.LogInformation($"Starting to do it some job {inputResult.Runbook.DoSomeJob}. for Intact.");
            // simulate doing the activity
            await Task.Delay(5000);
            return new OutputResult<string>
            {
                HasErrors = false,
                Message = $"The runbook Job \"{inputResult.Runbook.JobName}\" was completed and processed to Intact system.",
                ProccesedIn = ExternalSystem.Intact,
                Data = "Intact additional interesting data."
            };
        }

        [FunctionName(nameof(SalesforceRunbookToProcess))]
        public static async Task<OutputResult<string>> SalesforceRunbookToProcess([ActivityTrigger] InputResult inputResult, ILogger log)
        {
            try
            {
                log.LogInformation($"Getting configuration to use for Salesforce system = {inputResult.SomeConfigHere}.");
                log.LogInformation($"Running Salesforce runbook process {inputResult.Runbook.JobId} = \"{inputResult.Runbook.JobName}\".");
                if (inputResult.Runbook.DoSomeJob.Contains("Bad"))
                {
                    throw new InvalidOperationException($"Failed to running SalesForce process. Job: \"{inputResult.Runbook.JobName}\"");
                }

                log.LogInformation($"Starting to do it some job {inputResult.Runbook.DoSomeJob}. for Salesforce.");
                // simulate doing the activity
                await Task.Delay(10000);
                return new OutputResult<string>
                {
                    HasErrors = false,
                    Message = $"The runbook Job \"{inputResult.Runbook.JobName}\" was completed and processed to Salesforce system.",
                    ProccesedIn = ExternalSystem.Salesforce,
                    Data = "Salesforce additional interesting data."
                };
            }
            catch (Exception e)
            {

                return new OutputResult<string>
                {
                    HasErrors = true,
                    Message = e.Message,
                    ProccesedIn = ExternalSystem.Salesforce,
                };
            }
        }

        [FunctionName(nameof(ConcurRunbookToProcess))]
        public static async Task<OutputResult<string>> ConcurRunbookToProcess([ActivityTrigger] InputResult inputResult, ILogger log)
        {
            log.LogInformation($"Getting configuration to use for Concur system = {inputResult.SomeConfigHere}.");
            log.LogInformation($"Running Concur runbook process {inputResult.Runbook.JobId} = \"{inputResult.Runbook.JobName}\".");
            if (inputResult.Runbook.ItShouldRetry)
            {
                throw new InvalidOperationException($"Failed to running Concur process. Job: \"{inputResult.Runbook.JobName}\"");
            }
            // simulate doing the activity
            log.LogInformation($"Starting to do it some job {inputResult.Runbook.DoSomeJob}. for Concur.");
            await Task.Delay(5000);
            return new OutputResult<string>
            {
                HasErrors = false,
                Message = $"The runbook Job \"{inputResult.Runbook.JobName}\" was completed and processed to Intact system.",
                ProccesedIn = ExternalSystem.Concur,
                Data = "Concur additional interesting data."
            };
        }

        [FunctionName(nameof(GetConfigValues))]
        public static Dictionary<ExternalSystem, string> GetConfigValues([ActivityTrigger] object input)
        {
            return Environment.GetEnvironmentVariable("ConfigForExternalSystems")
                .Split(';')
                .Select(x =>
                {
                    var keyPair = x.Split('=');
                    var val = (ExternalSystem)int.Parse((keyPair[0]));
                    return new
                    {
                        Key = val,
                        Value = keyPair[1]
                    };
                }).ToDictionary(x => x.Key, x => x.Value);
        }

        #region For monitoring

        [FunctionName(nameof(MonitoringDf))]
        public static void MonitoringDf([ActivityTrigger] RunbookStep runbookStep,
        [Table("MonitoringRunbooks", "AzureWebJobsStorage")] out MonitoringRunbook monitoring, ILogger log)
        {
            var monitoringId = Guid.NewGuid().ToString("N");
            monitoring = new MonitoringRunbook
            {
                PartitionKey = "MonitoringRunbook",
                RowKey = monitoringId,
                OrchestrationId = runbookStep.OrchestrationId,
                Activity = runbookStep.Message
            };

        }
        #endregion


        #region For External Events activities.

        [FunctionName(nameof(SendApprovalToContinue))]
        public static void SendApprovalToContinue([ActivityTrigger] ApprovalInfo approvalInfo,
        [Table("RunbookApprovals", "AzureWebJobsStorage")] out RunbookApproval runbookApproval, ILogger log)
        {
            log.LogInformation($"Requesting approval for \"{approvalInfo.RunbookId}\".");
            var runbookApprovalId = Guid.NewGuid().ToString("N");
            runbookApproval = new RunbookApproval
            {
                PartitionKey = "RunbookApproval",
                RowKey = runbookApprovalId,
                OrchestrationId = approvalInfo.OrchestrationId,
                RunbookId = approvalInfo.RunbookId
            };

            // can sending an email using SendGridMessage,
            // sending notification using SignalRService ?
            // so on
        }

        [FunctionName(nameof(SendRunbookResultsProcess))]
        public static async Task SendRunbookResultsProcess([ActivityTrigger] ApprovalOutput approvalOutput, ILogger log)
        {
            log.LogInformation($"Completing {approvalOutput.SomeMoreRelevantInfo}.");
            // simulate completing runbook process
            await Task.Delay(1000);
        }

        [FunctionName(nameof(AbortAndCleanUpProcess))]
        public static async Task AbortAndCleanUpProcess([ActivityTrigger] RunbookRequest runbook, ILogger log)
        {
            log.LogInformation($"Abort and Cleanup process for {runbook.JobId} \"{runbook.JobName}\".");
            // simulate aborting and cleanup runbook process
            await Task.Delay(1000);
        }


        #endregion
    }
}