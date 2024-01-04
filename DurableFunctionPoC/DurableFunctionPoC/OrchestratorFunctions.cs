using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DurableFunctionPoC.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctionPoC
{
    public class OrchestratorFunctions
    {

        public static RunbookProcessorStatus Status = RunbookProcessorStatus.NoStarted;
        private List<RunbookMonitoring> _orchestratorMonitorDetails = new ();
        private readonly ILogger<OrchestratorFunctions> _log;

        public OrchestratorFunctions(ILogger<OrchestratorFunctions> log)
        {
            _log = log;
        }

        [FunctionName(nameof(ProcessRunbookOrchestrator))]
        public async Task<object> ProcessRunbookOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            Status = RunbookProcessorStatus.Processing;
            //Technique to only write them if we're not replaying
            //at this point in the orchestractor function.
            log = context.CreateReplaySafeLogger(log);

            var runbook = context.GetInput<RunbookRequest>();
            var pollingInterval = GetPollingInterval();
            var expireTime = GetExpiryTime(context);

            var runbooksResults = await context.
                CallSubOrchestratorAsync<OutputResult<string>[]>(nameof(ExcecuteRunbookOrchestrator), runbook);

            var intactRunbookProcessResult = runbooksResults.Where(x => x.ProccesedIn == ExternalSystem.Intact).SingleOrDefault();
            var salesforceRunbookProcessResult = runbooksResults.Where(x => x.ProccesedIn == ExternalSystem.Salesforce).SingleOrDefault();
            var concurRunbookProcessResult = runbooksResults.Where(x => x.ProccesedIn == ExternalSystem.Concur).SingleOrDefault();

            SetCustomStatus(context, intactRunbookProcessResult, salesforceRunbookProcessResult, concurRunbookProcessResult);
            await context.CallActivityAsync<bool>("AddingDelay",null);

            await context.CallActivityAsync("RunSomeProcess",
                new InputResult
                {
                    Runbook = runbook,
                    SomeConfigHere = "SomeConfigHere"
                });


            var isProcessWithinTime = context.CurrentUtcDateTime < expireTime;
            while (isProcessWithinTime)
            {
                var runbookStatus = await context.CallActivityAsync<RunbookProcessorStatus>("GetStatus", context.InstanceId);
                Status = runbookStatus;
                if (runbookStatus == RunbookProcessorStatus.Processed)
                {
                    break;
                }

                SetCustomStatus(context, intactRunbookProcessResult, salesforceRunbookProcessResult,
                    concurRunbookProcessResult, Status.ToString());

                var nextCheck = context.CurrentUtcDateTime.AddSeconds(pollingInterval);
                await context.CreateTimer(nextCheck, CancellationToken.None);

                if (!isProcessWithinTime)
                {
                    _log.LogWarning($"Time out.");
                }

            }

            SetCustomStatus(context, intactRunbookProcessResult, salesforceRunbookProcessResult,
                concurRunbookProcessResult, Status.ToString());

            return new
            {
                SomeProcess = new
                {
                    HasErrors = false,
                    Message = "SomeProcess runbook completed",
                    ProccesedIn = "SomeProcess runbook.",
                    Data = "SomeProcess some data.",
                    RunbookStatus = Status.ToString()
                },
                Intact = GetResult(intactRunbookProcessResult),
                Salesforce = GetResult(salesforceRunbookProcessResult),
                Concur = GetResult(concurRunbookProcessResult)
            };

        }
        
        [FunctionName(nameof(ExcecuteRunbookOrchestrator))]
        public async Task<OutputResult<string>[]> ExcecuteRunbookOrchestrator(
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

            var runbooksResults = await Task.WhenAll(runbookToProcessTasks);

            return runbooksResults;

        }

        private DateTime GetExpiryTime(IDurableOrchestrationContext context)
        {
            return context.CurrentUtcDateTime.AddSeconds(90);
        }

        private int GetPollingInterval()
        {
            return 10;
        }

        private void SetCustomStatus([OrchestrationTrigger] IDurableOrchestrationContext context, OutputResult<string> intactResult,
            OutputResult<string> salesforceResult, OutputResult<string> concurResult, string status = null)
        {
            List<object> result = new();

            if (!string.IsNullOrEmpty(status))
            {
                result.Add(new
                {
                    Activity = $@"Runbook Processor at: ""SomeProcessor"".",
                    Status = status,
                    Details = ""
                });
            }

            result.Add(new
            {
                Activity = $@"Runbook Processor at: ""{intactResult.ProccesedIn.ToString()}"".",
                Status = intactResult.RunbookStatus,
                Details = intactResult.RunbookMonitoring
            });
            result.Add(new
            {
                Activity = $@"Runbook Processor at: ""{salesforceResult.ProccesedIn.ToString()}"".",
                Status = salesforceResult.RunbookStatus,
                Details = salesforceResult.RunbookMonitoring
            });
            result.Add(new
            {
                Activity = $@"Runbook Processor at: ""{concurResult.ProccesedIn.ToString()}"".",
                Status = concurResult.RunbookStatus,
                Details = concurResult.RunbookMonitoring
            });

            context.SetCustomStatus(result);
        }

        private void SetCustomStatus([OrchestrationTrigger] IDurableOrchestrationContext context, string status)
        {
            context.SetCustomStatus(new
            {
                Activity = "Some Processor.",
                Status = status
            });
        }

        private object GetResult(OutputResult<string> outputResult)
        {
            return new
            {
                HasErrors = outputResult.HasErrors,
                Message = outputResult.Message,
                ProccesedIn = outputResult.ProccesedIn.ToString(),
                Data = outputResult.Data,
                RunbookStatus = outputResult.RunbookStatus
            };
        }
    }
}