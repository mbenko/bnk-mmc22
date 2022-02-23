using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;

namespace fnApp
{
    public class LogTable : TableEntity
    {
        public object Data { get; set; }
    }
    public static class MyStuff
    {
        public static int count = 0;

        [FunctionName("GetMyStuff")]
        public static async Task<IEnumerable<LogTable>> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Table("myTable")] CloudTable myTable,
            ILogger log)
        {
            var tsInvoke = DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            var tsLastDay = DateTime.UtcNow.AddDays(-1).ToString("yyyyMMddhhmmss");
            log.LogInformation($"[{++count}] : MyStuff|GET : Start ({tsInvoke})");

            string owner = req.Query["owner"];

            string responseMessage = string.IsNullOrEmpty(owner)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {owner}. This HTTP triggered function executed successfully.";

            TableQuery<LogTable> rangeQuery = new TableQuery<LogTable>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                        owner),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan,
                        tsLastDay)));

            var rc = new List<LogTable>();
            // Execute the query and loop through the results
            foreach (LogTable entity in
                await myTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                rc.Add(entity);
            }

            log.LogInformation($"[{count}] : MyStuff|GET : Complete");

            return rc.AsEnumerable();
            // return new OkObjectResult(rc);
        }
        [FunctionName("PostMyStuff")]
        public static async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table("myTable")] ICollector<LogTable> myTable,
            ILogger log)
        {
            var tsInvoke = DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            log.LogInformation($"[{++count}] : MyStuff|GET : Start ({tsInvoke})");

            var loggit = new LogTable
            {
                PartitionKey = "default",
                RowKey = tsInvoke
            };

            string owner = req.Query["owner"];
            loggit.PartitionKey = owner;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            loggit.Data = requestBody;
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            myTable.Add(loggit);

            string responseMessage = string.IsNullOrEmpty(owner)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {owner}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
