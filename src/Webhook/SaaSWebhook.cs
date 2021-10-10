using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Webhook
{
    public static class SaaSWebhook
    {
        [FunctionName("Webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("===================================");
            log.LogInformation("WEBHOOK FUNCTION FIRING");
            log.LogInformation("===================================");
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string responseMessage = string.IsNullOrEmpty(data.ToString())
                ? "This HTTP triggered function executed successfully but no JSON POST body was sent."
                : data.ToString();

            log.LogInformation(responseMessage);

            return new OkObjectResult(responseMessage);
        }
    }
}
