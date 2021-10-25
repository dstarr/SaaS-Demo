using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebhookFunc
{
    public static class Webhook
{
        [FunctionName("Webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("===================================");
            log.LogInformation("SaaS WEBHOOK FUNCTION FIRING");
            log.LogInformation("-----------------------------------");

            var passed = CheckSecretString(req, context);
            if (!passed)
            {
                log.LogInformation("Query String check did not pass!");
                return new StatusCodeResult(403);
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string logMessage = string.IsNullOrEmpty(data.ToString())
                ? "No POST body JSON was received."
                : data.ToString();

            log.LogInformation(logMessage);
            log.LogInformation("===================================");

            return new OkResult();
        }

        private static bool CheckSecretString(HttpRequest req, ExecutionContext context)
        {
            // set up the configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // get the env:Variables
            var secretKey = config["QuerySecret:SecretKey"];
            var secretValue = config["QuerySecret:SecretValue"];

            // ensure the key/value exists
            if (req.Query[secretKey].Count != 1)
            {
                return false;
            }
            
            // look for a match on the SecretValue
            var token = req.Query[secretKey][0];
            if (secretValue != token)
            {
                return false;
            }

            return true;
        }
}
}
