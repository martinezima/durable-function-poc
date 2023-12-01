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
        [FunctionName(nameof(FirstRunbookToProcess))]
        public static async Task<string> FirstRunbookToProcess([ActivityTrigger] string runbook, ILogger log)
        {
            log.LogInformation($"Running first runbook process {runbook}.");
            // simulate doing the activity
            await Task.Delay(5000);
            return $"The runbook was processed {runbook} to first external system.";
        }

        [FunctionName(nameof(SecondRunbookToProcess))]
        public static async Task<object> SecondRunbookToProcess([ActivityTrigger] string runbook, ILogger log)
        {
            try
            {
                log.LogInformation($"Running second runbook process {runbook}.");
                if (runbook.Contains("Bad"))
                {
                    throw new InvalidOperationException("Failed to running second runbook process.");
                }

                // simulate doing the activity
                await Task.Delay(5000);
                return new
                {
                    Success = true,
                    Output = $"The runbook was processed {runbook} to second external system."
                };
            }
            catch (Exception e)
            {
                return new
                {
                    Success = false,
                    Output = e.Message
                };
            }
        }

        [FunctionName(nameof(ThirdRunbookProcess))]
        public static async Task<string> ThirdRunbookProcess([ActivityTrigger] string runbook, ILogger log)
        {
            log.LogInformation($"Running third runbook process {runbook}.");
            if (runbook.Contains("Retry"))
            {
                throw new InvalidOperationException("Failed to running third runbook process.");
            }
            // simulate doing the activity
            await Task.Delay(5000);
            return $"The runbook was processed {runbook} to third external system.";
        }
    }
}