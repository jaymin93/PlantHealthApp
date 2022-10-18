using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;

namespace GetPlantHealthDetails
{
    public static class GetPlantHealth
    {
        internal static CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=planthealthapp;AccountKey=TtUK1vbYh3Z1vBycLsn+1UAeAGI0lBvPYwyQCE9Z68JdmT69byy7Qx8BSvqJDGPN/awTvrDf+6Zb+ASt9CV4mw==;EndpointSuffix=core.windows.net");
        static string tableName = "PlantHealthAppTable";


        internal static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        internal static CloudTable _linkTable = tableClient.GetTableReference(tableName);

        internal static PlantHealthDeatils deatails = new PlantHealthDeatils();

        [FunctionName("GetPlantHealth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get" , Route = null)] HttpRequest req,
            ILogger log)
        {
            

            string datetime = req.Query["datetime"];

           

            return new OkObjectResult(await GetPlantHealthDeatilsAsync());
        }

        public async static Task<List<PlantHealthDeatils>> GetPlantHealthDeatilsAsync()
        {
            try
            {
                List<PlantHealthDeatils> plantHealthDeatilsList = new List<PlantHealthDeatils>();
                TableQuery<PlantHealthDeatils> query;
                query = new TableQuery<PlantHealthDeatils>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{tableName}"));
                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<PlantHealthDeatils> resultSegment = await _linkTable.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    token = resultSegment.ContinuationToken;

                    foreach (var entity in resultSegment.Results)
                    {
                        PlantHealthDeatils details = new PlantHealthDeatils
                        {
                            longitude = entity.longitude,
                            ImageURL = entity.ImageURL,
                            latitude  = entity.latitude,
                            Pesticidesprayed =entity.Pesticidesprayed,
                            CapturedTime =entity.CapturedTime
                        };

                        plantHealthDeatilsList.Add(details);
                    }
                } while (token != null);


                return plantHealthDeatilsList;

            }
            catch (Exception exp)
            {
                Debug.Write(exp);
                return default;
            }
        }
    }
}
