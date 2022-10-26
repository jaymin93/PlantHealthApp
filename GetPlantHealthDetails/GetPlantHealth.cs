using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using Microsoft.Azure.KeyVault;

namespace GetPlantHealthDetails
{
    public class GetPlantHealth
    {
        private static CloudStorageAccount storageAccount = null;
        private static CloudTableClient tableClient = null;
        private static CloudTable table = null;
        private static KeyVaultClient client = null;
        private static Microsoft.Azure.KeyVault.Models.SecretBundle connectionstring = null;
        private static string tableName = "PlantHealthAppTable";
        private static string secretIdentifier = "https://planthealthappsecret.vault.azure.net/secrets/storageAccountConnectionString/92f4ed20ff4041ae8b05303f7baf79f7";

        [FunctionName("GetPlantHealth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            SetClientIDAndSecret();
            client ??= new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(TokenHelper.GetAccessTokenAsync));
            connectionstring ??= await client.GetSecretAsync(secretIdentifier);
            storageAccount ??= CloudStorageAccount.Parse(connectionstring.Value);
            tableClient ??= storageAccount.CreateCloudTableClient();
            table ??= tableClient.GetTableReference(tableName);

            string rowkey = req.Query["RowKey"];
            if (string.IsNullOrEmpty(rowkey))
            {
                return new OkObjectResult(await GetPlantHealthDeatilsAsync(log));
            }
            else
            {
                return new OkObjectResult(await UpdatePlantHealthDeatilsByRowkeyAsync(rowkey, log));
            }
        }

        private static async Task<List<PlantHealthDeatils>> GetPlantHealthDeatilsAsync(ILogger logger)
        {
            try
            {
                List<PlantHealthDeatils> plantHealthDeatilsList = new List<PlantHealthDeatils>();
                TableQuery<PlantHealthDeatils> query;
                query = new TableQuery<PlantHealthDeatils>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{tableName}"));
                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<PlantHealthDeatils> resultSegment = await table.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    foreach (var entity in resultSegment.Results.OrderBy(x => x.Pesticidesprayed))
                    {
                        PlantHealthDeatils details = new PlantHealthDeatils
                        {
                            longitude = entity.longitude,
                            ImageURL = entity.ImageURL,
                            latitude = entity.latitude,
                            Pesticidesprayed = entity.Pesticidesprayed,
                            CapturedTime = entity.CapturedTime,
                            RowKey = entity.RowKey,
                            ETag = entity.ETag,
                            PartitionKey = entity.PartitionKey
                        };
                        plantHealthDeatilsList.Add(details);
                    }
                } while (token != null);
                return plantHealthDeatilsList;
            }
            catch (Exception exp)
            {
                logger.LogError(exp, "Unable to GetPlantHealthDeatils");
                return default;
            }
        }

        private static async Task<HttpResponseMessage> UpdatePlantHealthDeatilsByRowkeyAsync(string rowkey, ILogger logger)
        {
            try
            {
                PlantHealthDeatils plantHealthDeatilsList = new PlantHealthDeatils();
                TableQuery<PlantHealthDeatils> query;
                query = new TableQuery<PlantHealthDeatils>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, $"{rowkey}"));
                TableContinuationToken token = null;

                TableQuerySegment<PlantHealthDeatils> resultSegment = await table.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);

                var plantdetail = resultSegment.Results.FirstOrDefault();
                plantdetail.Pesticidesprayed = true;
                var operation = TableOperation.Replace(plantdetail);
                await table.ExecuteAsync(operation);

                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            catch (Exception exp)
            {
                logger.LogError(exp, "Unable to Update PlantHealthDeatils");
                return default;
            }
        }

        private static string GetEnviromentValue(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        private static void SetClientIDAndSecret()
        {
            TokenHelper.clientID ??= GetEnviromentValue("clientID");
            TokenHelper.clientSecret ??= GetEnviromentValue("clientSecret");
        }
    }
}
