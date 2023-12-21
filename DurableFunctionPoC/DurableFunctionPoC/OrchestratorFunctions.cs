using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using DurableFunctionPoC.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionPoC
{
    public static class OrchestratorFunctions
    {
        [FunctionName(nameof(ProcessRunbookOrchestrator))]
        public static async Task<object> ProcessRunbookOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            //Technique to only write them if we're not replaying
            //at this point in the orchestractor function.
            log = context.CreateReplaySafeLogger(log);

            var runbook = context.GetInput<RunbookRequest>();
            //BTW there  is also a CallSubOrchestratorAsyncWithRetries
            // We can use if we want to retries with backoff
            //await context.CallActivityAsync("MonitoringDf", new RunbookStep
            //{
            //    OrchestrationId = context.InstanceId,
            //    Message = "About to run Fan in / Fan out."
            //});
            var runbooksResults = await context.
                CallSubOrchestratorAsync<OutputResult<string>[]>(nameof(ExcecuteRunbookOrchestrator), runbook);


            var intactRunbookProcessResult = runbooksResults.Where(x => x.ProccesedIn == ExternalSystem.Intact).SingleOrDefault();
            var salesforceRunbookProcessResult = runbooksResults.Where(x => x.ProccesedIn == ExternalSystem.Salesforce).SingleOrDefault();
            var concurRunbookProcessResult = runbooksResults.Where(x => x.ProccesedIn == ExternalSystem.Concur).SingleOrDefault();

            //await context.CallActivityAsync("MonitoringDf", new RunbookStep
            //{
            //    OrchestrationId = context.InstanceId, 
            //    Message = "Fan in / Fan out completed."
            //});

            //await context.CallActivityAsync("SendApprovalToContinue", new ApprovalInfo()
            //{
            //    OrchestrationId = context.InstanceId,
            //    RunbookId = runbook.JobId
            //});

            //var approvalResult = await context.WaitForExternalEvent<ApprovalOutput>("ApprovalResult");

            ////This peace of code would be another great use case for Subsorchestration.
            //if (approvalResult.IsApproved)
            //{
            //    approvalResult.SomeMoreRelevantInfo = runbook.JobName;
            //    await context.CallActivityAsync("SendRunbookResultsProcess", approvalResult);
            //}
            //else
            //{
            //    await context.CallActivityAsync("AbortAndCleanUpProcess", approvalResult);
            //    await context.WaitForExternalEvent<string>("JustCancelTheWorkflow");
            //}

            //await context.CallActivityAsync("MonitoringDf", new RunbookStep
            //{
            //    OrchestrationId = context.InstanceId,
            //    Message = $"Runbook for Job \"{runbook.JobName}\" process is completed, Approval Status; {approvalResult?.ApprovalStatus ?? "Unknown"}."
            //});

            return new
            {
                Intact = intactRunbookProcessResult,
                Salesforce = salesforceRunbookProcessResult,
                Concur = concurRunbookProcessResult,
                //ApprovalStatus = approvalResult?.ApprovalStatus ?? "Unknown"
            };

        }
        
        [FunctionName(nameof(ExcecuteRunbookOrchestrator))]
        public static async Task<OutputResult<string>[]> ExcecuteRunbookOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var runbook = context.GetInput<RunbookRequest>();

            var runbookToProcessTasks = new List<Task<OutputResult<string>>>();
            var configsForRunbooks = await context.CallActivityAsync<Dictionary<ExternalSystem, string>>("GetConfigValues", null);

            runbookToProcessTasks.Add(context.CallActivityAsync<OutputResult<string>>("IntactRunbookToProcess",
                new InputResult
                {
                    Runbook = runbook,
                    SomeConfigHere = configsForRunbooks.GetValueOrDefault(ExternalSystem.Intact)
                }));

            runbookToProcessTasks.Add(context.CallActivityAsync<OutputResult<string>>("SalesforceRunbookToProcess",
                new InputResult
                {
                    Runbook = runbook,
                    SomeConfigHere = configsForRunbooks.GetValueOrDefault(ExternalSystem.Salesforce)
                }));

            runbookToProcessTasks.Add(context.CallActivityAsync<OutputResult<string>>("ConcurRunbookToProcess",
                new InputResult
                {
                    Runbook = runbook,
                    SomeConfigHere = configsForRunbooks.GetValueOrDefault(ExternalSystem.Concur)
                }));

            //await context.CallActivityAsync("MonitoringDf", new RunbookStep 
            //{
            //    OrchestrationId = context.InstanceId,
            //    Message = "About to run Fan in / Fan out, called by SubOrchestrator."
            //});

            var runbooksResults = await Task.WhenAll(runbookToProcessTasks);



            //await context.CallActivityAsync("MonitoringDf", new RunbookStep
            //{
            //    OrchestrationId = context.InstanceId,
            //    Message = "About to run Fan in / Fan out completed, called by SubOrchestrator."
            //});

            return runbooksResults;

        }


    }
}