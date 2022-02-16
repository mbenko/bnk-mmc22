using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace myfn
{
    public class LogTable{
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Text { get; set; }
        public string RemoteIpAddress { get; set; }
        public string SessionId { get; set; }

        public LogTable ()
        {
            PartitionKey = "HttpLoggit";
            RowKey = DateTime.UtcNow.ToString("yyyyMMddhhmmss");
        }
    }
    public static class HttpLoggit
    {
        [FunctionName("HttpLoggit")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Table("myTable")]ICollector<LogTable> myTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var rc = new LogTable {
                RemoteIpAddress = req.HttpContext.Connection.RemoteIpAddress.ToString(),
                SessionId = req.HttpContext.Connection.Id
            };            

            string meta = req.Query["meta"];
            string source = req.Query["source"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            data = meta ?? data?.name;
            source = source ?? data?.source;

            if (!string.IsNullOrEmpty(source))
                rc.PartitionKey = source;

            rc.Text = data;
            myTable.Add(rc);

            return new OkObjectResult($"Complete. {rc.PartitionKey}:{rc.RowKey} {rc.Text}");
        }
    }
}
