using System.Net;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api
{
    public class GetResumeCounter
    {
        private readonly ILogger<GetResumeCounter> _logger;

        public GetResumeCounter(ILogger<GetResumeCounter> logger){
            _logger = logger;
        }
        private async Task UpdateCosmosDbAsync(Counter counter){
            try{
                var connectionString = Environment.GetEnvironmentVariable("AzureResumeConnectionString");
                var cosmosClient = new CosmosClient(connectionString);
                var container = cosmosClient.GetContainer("AzureResume", "Counter");
                await container.ReplaceItemAsync(counter, counter.Id, new PartitionKey("1"));
            }
            catch (CosmosException ex){
                _logger.LogError($"Error updating counter in Cosmos DB: {ex.Message}");
            }
        }

        [Function("GetResumeCounter")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req,
            [CosmosDBInput(databaseName: "AzureResume", containerName: "Counter", Connection = "AzureResumeConnectionString", Id = "1", PartitionKey = "1")] Counter counter){
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            counter.Count += 1;
            await UpdateCosmosDbAsync(counter);
            var jsonToReturn = JsonConvert.SerializeObject(counter);
            var response = req.CreateResponse();
            response.StatusCode = HttpStatusCode.OK;
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(jsonToReturn, Encoding.UTF8);

            return response;
        }
    }
}
