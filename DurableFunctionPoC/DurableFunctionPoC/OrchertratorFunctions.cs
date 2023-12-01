using System;
using System.Threading.Tasks;
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
            
            var runbook = context.GetInput<string>();

            log.LogInformation("about to call first activity");
            var firstRunbookProcessResult = await context.CallActivityAsync<string>("FirstRunbookToProcess", runbook);
            
            log.LogInformation("about to call second activity");
            var secondRunbookProcessResult = await context.CallActivityAsync<object>("SecondRunbookToProcess", runbook);

            log.LogInformation("about to call third activity");
            var thirdRunbookProcessResult = await context.CallActivityWithRetryAsync<string>("ThirdRunbookProcess",
                new RetryOptions(TimeSpan.FromSeconds(5), 5)
                {
                    Handle = ex =>
                    {
                        return ex.InnerException is InvalidOperationException;
                    }
                },runbook);

            return new
            {
                FirstRunbookProcessResult = firstRunbookProcessResult,
                SecondRunbookProcessResult = secondRunbookProcessResult,
                ThirdRunbookProcessResult = thirdRunbookProcessResult
            };

        }
    }
}