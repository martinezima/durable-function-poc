using System;
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
    public class HttpFunctions
    {
        [FunctionName(nameof(ProccessRunbookStarter))]
        public async Task<IActionResult> ProccessRunbookStarter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest req,
            [DurableClient] IDurableClient starter,
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

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, timeout);
        }


        [FunctionName(nameof(CheckDurableFuctionStatusbyInstanceId))]
        public static async Task<IActionResult> CheckDurableFuctionStatusbyInstanceId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CheckDurableFuctionStatusbyInstanceId")]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var durableFunctionClientRequest = JsonConvert.DeserializeObject<DurableFunctionClientRequest>(body);

            if (durableFunctionClientRequest == null)
            {
                return new BadRequestObjectResult("Need an Instance Id.");
            }

            var status = await client.GetStatusAsync(durableFunctionClientRequest.InstanceId, showHistoryOutput: true);

            return new OkObjectResult(status);
        }


        //[FunctionName(nameof(CancelRunbookForExternal))]
        //public static async Task<IActionResult> CancelRunbookForExternal(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CancelRunbook/{id}")]
        //    HttpRequest req,
        //    [DurableClient] IDurableOrchestrationClient client,
        //    [Table("MonitoringRunbooks", "MonitoringRunbook", "{id}", Connection = "AzureWebJobsStorage")] MonitoringRunbook monitoring,
        //    ILogger log)
        //{
        //    //if the approval code doesn't exist, framewor just return a 404 before we get here

        //    if (monitoring == null)
        //    {
        //        return new BadRequestObjectResult("Need an existing running Runbook.");
        //    }

        //    await client.TerminateAsync(monitoring.OrchestrationId, "Aborted");

        //    return new OkObjectResult("Event for cancellation have been requested.");
        //}


    }
}