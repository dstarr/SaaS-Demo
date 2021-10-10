using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SaaSFunctions.Functions
{

    public class Webhook
    {
        [Function("Webhook")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SaaSWebhook");

            logger.LogInformation("===================================");
            logger.LogInformation("WEBHOOK FUNCTION FIRING");
            logger.LogInformation("-----------------------------------");
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(requestBody));
            //logger.LogInformation((string)data.ToString());
            
            logger.LogInformation(requestBody);
            logger.LogInformation("===================================");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}