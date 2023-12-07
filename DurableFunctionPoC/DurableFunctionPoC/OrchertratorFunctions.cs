using System;
using System.Threading.Tasks;
using DurableFunctionPoC.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionPoC
{
    public static class OrchertratorFunctions
    {
        [FunctionName(nameof(ProcessRunbookOrchestrator))]
        public static async Task<object> ProcessRunbookOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            //Technique to only write them if we're not replaying
            //at this point in the orchestractor function.
            log = context.CreateReplaySafeLogger(log);
            
            var runbook = context.GetInput<RunbookRequest>();

            log.LogInformation("about to call first activity");
            var intactRunbookProcessResult = await context.CallActivityAsync<OutputResult<string>>("IntactRunbookToProcess", runbook);
            
            log.LogInformation("about to call second activity");
            var salesforceRunbookProcessResult = await context.CallActivityAsync<OutputResult<string>>("SalesforceRunbookToProcess", runbook);

            log.LogInformation("about to call third activity");
            var concurRunbookProcessResult = await context.CallActivityWithRetryAsync<OutputResult<string>>("ConcurRunbookToProcess",
                new RetryOptions(TimeSpan.FromSeconds(5), runbook.TimesToRetry)
                {
                    Handle = ex =>
                    {
                        return ex.InnerException is InvalidOperationException;
                    }
                },runbook);

            return new
            {
                Intact = intactRunbookProcessResult,
                Salesforce = salesforceRunbookProcessResult,
                Concur = concurRunbookProcessResult
            };

        }
    }
}