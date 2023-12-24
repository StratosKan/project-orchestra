using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using project_orchestra.Models.Orchestration;

namespace project_orchestra.Triggers
{
    public class OrchestrationApi
    {

        private readonly ILogger<OrchestrationApi> _logger;

        public OrchestrationApi(ILogger<OrchestrationApi> log)
        {
            _logger = log;
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> StartInstance(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, [DurableClient] IDurableOrchestrationClient orchestrator) //authorization not implemented yet...
        {
            _logger.LogInformation("OrchestratorApi - StartInstance function processed a request.");

            try
            {
                LogAllHeaders(req, _logger);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();               

                AzureOrchestrationCommand command = JsonConvert.DeserializeObject<AzureOrchestrationCommand>(requestBody);

                if (string.IsNullOrEmpty(command.FunctionName))
                {
                    return new BadRequestResult();
                }

                string instanceId = await orchestrator.StartNewAsync(command.FunctionName, command);

                return orchestrator.CreateCheckStatusResponse(req, instanceId);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, $"API-ERROR MESSAGE: {ex.Message}");

                return new ObjectResult(new { error = "Internal Server Error" })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }            
        }

        #region Helpers
        private static void LogAllHeaders(HttpRequest request, ILogger log)
        {
            if (request.Headers != null && request.Headers.Count > 0)
            {
                log.LogInformation("Request Headers:");

                foreach (var header in request.Headers)
                {
                    log.LogInformation($"{header.Key}: {string.Join(",", header.Value)}");
                }
            }
            else
            {
                log.LogInformation("No headers present in the request.");
            }
        }

        #endregion
    }
}

