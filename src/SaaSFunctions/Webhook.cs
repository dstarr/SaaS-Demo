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
        private static HttpRequest _request = null;
        private static ILogger _logger = null;
        private static ExecutionContext _executionContext = null;

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

            _request = req;
            _logger = log;
            _executionContext = context;

            LogHeader();

            if (!RequestIsSecure())
            {
                _logger.LogInformation("Security checks did not pass!");
                return new StatusCodeResult(403);
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            LogAction(data);

            string logMessage = string.IsNullOrEmpty(data.ToString())
                ? "No POST body JSON was received."
                : data.ToString();

            LogFooter(logMessage);

            return new OkResult();
        }

        /// <summary>
        /// Calls functions that check various security aspects of this webhook
        /// </summary>
        /// <returns>bool indicating if the request is secure</returns>
        private static bool RequestIsSecure()
        {
            // set up the configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(_executionContext.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            if (!QueryStringIsValid(config))
            {
                _logger.LogInformation("Querystring is invalid.");
                return false;
            }

            if (!ClaimsAreValid(config))
            {
                _logger.LogInformation("Claims are invalid.");
                return false;
            }

            return true;

        }

        /// <summary>
        /// Checks that JWT claims are valid
        /// </summary>
        /// <param name="config">IConfigurationRoot</param>
        /// <returns>A bool indicating if the Claims in the JWT are valid.</returns>
        private static bool ClaimsAreValid(IConfigurationRoot config)
        {
            string authHeader = _request.Headers["Authorization"];

            var jwt = authHeader.Split(' ')[1];
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(jwt))
            {
                _logger.LogInformation("Can't read JWT");
                return false;
            }

            var jwtToken = handler.ReadToken(jwt) as JwtSecurityToken;

            var appId = jwtToken.Claims.FirstOrDefault(c => c.Type == "appid").Value;
            var tenantId = jwtToken.Claims.FirstOrDefault(c => c.Type == "tid").Value;
            var issuer = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss").Value;

            if (appId != config["Auth:ApplicationId"])
            {
                _logger.LogInformation("Application ID does not match.");
                return false;
            }

            if (tenantId != config["Auth:TenantId"])
            {
                _logger.LogInformation("Tenant ID does not match.");
                return false;
            }

            var validIssuer = $"https://sts.windows.net/{tenantId}/";
            if (validIssuer != issuer)
            {
                _logger.LogInformation("Issuer does not match.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks that the secret on the querystring is present and correct
        /// </summary>
        /// <param name="config">IConfigurationRoot</param>
        /// <returns>A bool indicating if the querystring is valid.</returns>
        private static bool QueryStringIsValid(IConfigurationRoot config)
        {
            // get the env:Variables
            var secretKey = config["QueryStringSecret:SecretKey"];
            var secretValue = config["QueryStringSecret:SecretValue"];

            // ensure the key/value exists
            if (_request.Query[secretKey].Count == 0)
            {
                _logger.LogInformation("No secret key on query string.");
                return false;
            }

            // look for a match on the SecretValue
            var token = _request.Query[secretKey][0];
            if (secretValue != token)
            {
                _logger.LogInformation("Wrong secret key on query string.");
                return false;
            }

            return true;
        }

        private static void LogHeader()
        {
            _logger.LogInformation("===================================");
            _logger.LogInformation("SaaS WEBHOOK FUNCTION FIRING");
            _logger.LogInformation("-----------------------------------");
        }

        private static void LogFooter(string logMessage)
        {
            _logger.LogInformation(logMessage);
            _logger.LogInformation("===================================");
        }

        private static void LogAction(dynamic data)
        {
            var action = data.action;
            _logger.LogInformation($"ACTION: {action}");
            _logger.LogInformation("-----------------------------------");
        }


    }
}
