using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SaasFunctions
{
    public static class Webhook
    {
        [FunctionName("Webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            LogHeader(log);

            if (!RequestIsSecure(req, context))
            {
                log.LogInformation("Security checks did not pass!");
                return new StatusCodeResult(403);
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string logMessage = string.IsNullOrEmpty(data.ToString())
                ? "No POST body JSON was received."
                : data.ToString();

            LogFooter(log, logMessage);

            return new OkResult();
        }

        private static bool RequestIsSecure(HttpRequest req, ExecutionContext context)
        {
            // set up the configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            if (!QueryStringIsValid(req, context, config))
            {
                return false;
            }

            if (!ClaimsAreValid(req))
            {
                return false;
            }

            return true;

        }

        private static bool ClaimsAreValid(HttpRequest request)
        {
            string authHeader = request.Headers["Authorization"];

            if (authHeader == null) return false;

            var jwt = authHeader.Split(' ')[1];
            var payload = jwt.Split('.')[1];
            Console.WriteLine(payload);


            var bytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(bytes);

            Console.WriteLine(payloadJson);

            return true;
        }

        private static bool QueryStringIsValid(HttpRequest req, ExecutionContext context, IConfigurationRoot config)
        {
            // get the env:Variables
            var secretKey = config["QueryStringSecret:SecretKey"];
            var secretValue = config["QueryStringSecret:SecretValue"];

            // ensure the key/value exists
            if (req.Query[secretKey].Count == 0)
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

        private static void LogFooter(ILogger log, string logMessage)
        {
            log.LogInformation(logMessage);
            log.LogInformation("===================================");
        }

        private static void LogHeader(ILogger log)
        {
            log.LogInformation("===================================");
            log.LogInformation("SaaS WEBHOOK FUNCTION FIRING");
            log.LogInformation("-----------------------------------");
        }

    }
}
