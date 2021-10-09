using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SaaSWebhook
{
    public static class WebhookFunction
    {
        [FunctionName("Webhook")]
        public static async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
                ILogger log)
        {
            log.LogInformation("========================================");
            log.LogInformation("PROCESSING REQUEST");
            log.LogInformation("========================================");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            string responseMessage = string.IsNullOrEmpty(data.ToString())
                ? "This HTTP triggered function executed successfully, but recieved no POST body."
                : data.ToString();

            log.LogInformation(responseMessage);

            return new OkObjectResult(responseMessage);
        }
    }
}
