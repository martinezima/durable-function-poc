using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DurableFunctionPoC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DurableFunctionPoC
{
    public static class HttpFunctions
    {
        [FunctionName(nameof(ProccessRunbookStarter))]
        public static async Task<IActionResult> ProccessRunbookStarter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var runbook = JsonConvert.DeserializeObject<RunbookRequest>(body);

            if (runbook == null)
            {
                return new BadRequestObjectResult("Please pass the RunbookRequest at body.");
            }

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ProcessRunbookOrchestrator", null, runbook);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        [FunctionName(nameof(SubmitApprovalForExternal))]
        public static async Task<IActionResult> SubmitApprovalForExternal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SubmitRunbookApproval/{id}")]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            [Table("RunbookApprovals", "RunbookApproval", "{id}", Connection ="AzureWebJobsStorage")] RunbookApproval approval,
            ILogger log)
        {
            //if the approval code doesn't exist, framewor just return a 404 before we get here
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var approvalOutput = JsonConvert.DeserializeObject<ApprovalOutput>(body);

            if (approvalOutput == null)
            {
                return new BadRequestObjectResult("Need an approval outout result.");
            }

            await client.RaiseEventAsync(approval.OrchestrationId, "ApprovalResult", approvalOutput);

            return new OkObjectResult("Event for approval have been requested.");
        }


        [FunctionName(nameof(CancelRunbookForExternal))]
        public static async Task<IActionResult> CancelRunbookForExternal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CancelRunbook/{id}")]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            [Table("MonitoringRunbooks", "MonitoringRunbook", "{id}", Connection = "AzureWebJobsStorage")] MonitoringRunbook monitoring,
            ILogger log)
        {
            //if the approval code doesn't exist, framewor just return a 404 before we get here

            if (monitoring == null)
            {
                return new BadRequestObjectResult("Need an existing running Runbook.");
            }

            await client.TerminateAsync(monitoring.OrchestrationId, "Aborted");

            return new OkObjectResult("Event for cancellation have been requested.");
        }


    }
}