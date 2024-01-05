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
                return new BadRequestObjectResult(new { Message = "Need an Instance Id." });
            }

            var status = await client.GetStatusAsync(durableFunctionClientRequest.InstanceId, showHistoryOutput: true);

            return new OkObjectResult(status);
        }


        [FunctionName(nameof(CancelRunbookForExternal))]
        public static async Task<IActionResult> CancelRunbookForExternal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CancelRunbook/")]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var durableFunctionClientRequest = JsonConvert.DeserializeObject<DurableFunctionClientRequest>(body);

            if (durableFunctionClientRequest == null)
            {
                return new BadRequestObjectResult(new { Message = "Need an Instance Id." });
            }

            await client.TerminateAsync(durableFunctionClientRequest.InstanceId, "Aborted");

            return new OkObjectResult(new { Message = "Event for cancellation have been requested." });
        }


    }
}