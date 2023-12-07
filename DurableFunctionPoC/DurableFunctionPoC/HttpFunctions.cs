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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var runbook = JsonConvert.DeserializeObject<RunbookRequest>(body);

            if (runbook == null)
            {
                return new BadRequestObjectResult("Please pass the runbook name at body.");
            }

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ProcessRunbookOrchestrator", null, runbook);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}