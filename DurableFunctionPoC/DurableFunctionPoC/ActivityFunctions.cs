using DurableFunctionPoC.Interfaces;
using DurableFunctionPoC.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DurableFunctionPoC
{

    public class ActivityFunctions
    {
        private readonly ILogger _log;
        private readonly IConcurRunbookProcessor _concurRunbookProcessor;
        private readonly IIntactRunbookProcessor _intactRunbookProcessor;
        private readonly ISalesforceRunbookProcessor _salesforceRunbookProcessor;
        private readonly ISomeRunbookProcessor _someRunbookProcessor;
        private List<RunbookMonitoring> _monitoring  = new();

        public ActivityFunctions(ILogger<ActivityFunctions> log, 
            IConcurRunbookProcessor concurRunbookProcessor,
            IIntactRunbookProcessor intactRunbookProcessor,
            ISalesforceRunbookProcessor salesforceRunbookProcessor,
            ISomeRunbookProcessor someRunbookProcessor)
        {
            _log = log;
            _concurRunbookProcessor = concurRunbookProcessor;
            _intactRunbookProcessor = intactRunbookProcessor;
            _salesforceRunbookProcessor = salesforceRunbookProcessor;
            _someRunbookProcessor = someRunbookProcessor;
        }

        [FunctionName(nameof(IntactRunbookToProcess))]
        public async Task<OutputResult<string>> IntactRunbookToProcess([ActivityTrigger]        
        InputResult inputResult)
        {
            return await _intactRunbookProcessor.Process(inputResult);
        }

        [FunctionName(nameof(SalesforceRunbookToProcess))]
        public async Task<OutputResult<string>> SalesforceRunbookToProcess([ActivityTrigger] InputResult inputResult)
        {
            return await _salesforceRunbookProcessor.Process(inputResult);
        }

        [FunctionName(nameof(ConcurRunbookToProcess))]
        public async Task<OutputResult<string>> ConcurRunbookToProcess([ActivityTrigger] InputResult inputResult)
        {
            return await _concurRunbookProcessor.Process(inputResult);
        }

        [FunctionName(nameof(RunSomeProcess))]
        public void RunSomeProcess([ActivityTrigger] InputResult inputResult)
        {
            _someRunbookProcessor.Process(inputResult);
        }

        [FunctionName(nameof(GetConfigValues))]
        public Dictionary<ExternalSystem, string> GetConfigValues([ActivityTrigger] object input)
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

        [FunctionName(nameof(GetStatus))]
        public RunbookProcessorStatus GetStatus([ActivityTrigger] string orchestrationId)
        {
            var currentStatus = _someRunbookProcessor.GetStatus();
            _log.LogInformation($@"This is the status for Orchestration = {orchestrationId}: {currentStatus}");
            return currentStatus;
        }

        #endregion


        //#region For External Events activities.

        //[FunctionName(nameof(SendApprovalToContinue))]
        //public static void SendApprovalToContinue([ActivityTrigger] ApprovalInfo approvalInfo,
        //[Table("RunbookApprovals", "AzureWebJobsStorage")] out RunbookApproval runbookApproval, ILogger log)
        //{
        //    log.LogInformation($"Requesting approval for \"{approvalInfo.RunbookId}\".");
        //    var runbookApprovalId = Guid.NewGuid().ToString("N");
        //    runbookApproval = new RunbookApproval
        //    {
        //        PartitionKey = "RunbookApproval",
        //        RowKey = runbookApprovalId,
        //        OrchestrationId = approvalInfo.OrchestrationId,
        //        RunbookId = approvalInfo.RunbookId
        //    };

        //    // can sending an email using SendGridMessage,
        //    // sending notification using SignalRService ?
        //    // so on
        //}

        //[FunctionName(nameof(SendRunbookResultsProcess))]
        //public static async Task SendRunbookResultsProcess([ActivityTrigger] ApprovalOutput approvalOutput, ILogger log)
        //{
        //    log.LogInformation($"Completing {approvalOutput.SomeMoreRelevantInfo}.");
        //    // simulate completing runbook process
        //    await Task.Delay(1000);
        //}

        //[FunctionName(nameof(AbortAndCleanUpProcess))]
        //public static async Task AbortAndCleanUpProcess([ActivityTrigger] RunbookRequest runbook, ILogger log)
        //{
        //    log.LogInformation($"Abort and Cleanup process for {runbook.JobId} \"{runbook.JobName}\".");
        //    // simulate aborting and cleanup runbook process
        //    await Task.Delay(1000);
        //}


        //#endregion
    }
}