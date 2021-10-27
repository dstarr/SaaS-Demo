using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace SaasFunctions
{
    public static class Webhook
    {
        /// <summary>
        /// This function is called by Azure to execute the function. 
        /// It is called by Azure when an event occurs on a subscription in the Azure marketplace.
        /// It needs to be configured in Partner Center to be called by Azure marketplace.
        /// </summary>
        /// <param name="req">HttpRequest</param>
        /// <param name="log">ILogger</param>
        /// <param name="context">ExecutionContext</param>
        /// <returns>Task<IActionResult></returns>
        [FunctionName("Webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            LogHeader(log);

            if (!RequestIsSecure(req, context, log))
            {
                log.LogInformation("Security checks did not pass!");
                return new StatusCodeResult(403);
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            LogAction(data, log);

            string logMessage = string.IsNullOrEmpty(data.ToString())
                ? "No POST body JSON was received."
                : data.ToString();

            LogFooter(log, logMessage);

            return new OkResult();
        }

        /// <summary>
        /// Calls functions that check various security aspects of this webhook
        /// </summary>
        /// <param name="req">HttpRequest</param>
        /// <param name="context">ExecutionCOntext</param>
        /// <param name="log">ILogger</param>
        /// <returns>bool indicating if the request is secure</returns>
        private static bool RequestIsSecure(HttpRequest req, ExecutionContext context, ILogger log)
        {
            // set up the configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            if (!QueryStringIsValid(req, context, config, log))
            {
                log.LogInformation("Querystring is invalid.");
                return false;
            }

            if (!ClaimsAreValid(req, config, log))
            {
                log.LogInformation("Claims are invalid.");
                return false;
            }

            return true;

        }

        /// <summary>
        /// Checks that JWT claims are valid
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <param name="config">IConfiguration</param>
        /// <param name="log">ILogger</param>
        /// <returns>A bool indicating if the Claims in the JWT are valid.</returns>
        private static bool ClaimsAreValid(HttpRequest request, IConfigurationRoot config, ILogger log)
        {
            string authHeader = request.Headers["Authorization"];

            var jwt = authHeader.Split(' ')[1];
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(jwt))
            {
                log.LogInformation("Can't read JWT");
                return false;
            }

            var jwtToken = handler.ReadToken(jwt) as JwtSecurityToken;

            var appId = jwtToken.Claims.FirstOrDefault(c => c.Type == "appid").Value;
            var tenantId = jwtToken.Claims.FirstOrDefault(c => c.Type == "tid").Value;
            var issuer = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss").Value;

            if (appId != config["Auth:ApplicationId"])
            {
                log.LogInformation("Application ID does not match.");
                return false;
            }

            if (tenantId != config["Auth:TenantId"])
            {
                log.LogInformation("Tenant ID does not match.");
                return false;
            }

            var validIssuer = $"https://sts.windows.net/{tenantId}/";
            if (validIssuer != issuer)
            {
                log.LogInformation("Issuer does not match.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks that the secret on the querystring is present and correct
        /// </summary>
        /// <param name="req">HttpRequest</param>
        /// <param name="context">ExecutionContext</param>
        /// <param name="config">IConfigurationRoot</param>
        /// <param name="log">ILogger</param>
        /// <returns>A bool indicating if the querystring is valid.</returns>
        private static bool QueryStringIsValid(HttpRequest req, ExecutionContext context, IConfigurationRoot config, ILogger log)
        {
            // get the env:Variables
            var secretKey = config["QueryStringSecret:SecretKey"];
            var secretValue = config["QueryStringSecret:SecretValue"];

            // ensure the key/value exists
            if (req.Query[secretKey].Count == 0)
            {
                log.LogInformation("No secret key on query string.");
                return false;
            }

            // look for a match on the SecretValue
            var token = req.Query[secretKey][0];
            if (secretValue != token)
            {
                log.LogInformation("Wrong secret key on query string.");
                return false;
            }

            return true;
        }

        private static void LogHeader(ILogger log)
        {
            log.LogInformation("===================================");
            log.LogInformation("SaaS WEBHOOK FUNCTION FIRING");
            log.LogInformation("-----------------------------------");
        }

        private static void LogFooter(ILogger log, string logMessage)
        {
            log.LogInformation(logMessage);
            log.LogInformation("===================================");
        }

        private static void LogAction(dynamic data, ILogger log)
        {
            var action = data.action;
            log.LogInformation($"ACTION: {action}");
            log.LogInformation("-----------------------------------");
        }


    }
}
