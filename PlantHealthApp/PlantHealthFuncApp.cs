using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlantHealthApp;
using RestSharp;

namespace PlantHealth.Function
{
    public class PlantHealthFuncApp
    {
        static string predictionUrl = "https://planthealthcustomvision-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/e8690773-d03f-439f-9cd0-4bf193cd8687/classify/iterations/Iteration1/url";
        static string storageAccountUri = "https://planthealthapp.blob.core.windows.net/";
        static string containerName = "planthealthcontainer/";
        static string tableName = "PlantHealthAppTable";
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=planthealthapp;AccountKey=TtUK1vbYh3Z1vBycLsn+1UAeAGI0lBvPYwyQCE9Z68JdmT69byy7Qx8BSvqJDGPN/awTvrDf+6Zb+ASt9CV4mw==;EndpointSuffix=core.windows.net");

        static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        static CloudTable table = tableClient.GetTableReference(tableName);

        [FunctionName("PlantHealthFuncApp")]
        public async static Task Run([BlobTrigger("planthealthcontainer/{name}", Connection = "planthealthapp_STORAGE")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            string imageUrl = $"{storageAccountUri}{containerName}{name}";
            PlantHealthCustomVisionModel response = await GetProbabilityValuesFromCustomVisionRestApiAsync(imageUrl);
            if (ReportAffectedPlant(response))
            {
                await AddAffectedPlantDeatilsToAzureTableAsync(GetLongitudeLatitudeDeatailsFromFileName(name)[0], GetLongitudeLatitudeDeatailsFromFileName(name)[1], imageUrl, log);
            }
        }

        public static bool ReportAffectedPlant(PlantHealthCustomVisionModel plantHealthCustomVisionModel)
        {
            double affected = Convert.ToDouble(plantHealthCustomVisionModel.predictions.ElementAt(0).probability);
            double healthy = Convert.ToDouble(plantHealthCustomVisionModel.predictions.ElementAt(1).probability);
            if (affected > healthy)
            {
                return true;
            }
            return false;
        }

        public static string[] GetLongitudeLatitudeDeatailsFromFileName(string fileName)
        {
            return fileName.Replace(".jpg", "").Replace(".png", "").Replace(".bmp", "").Replace(".jpeg", "").Split("---");
        }

        public static async Task<bool> AddAffectedPlantDeatilsToAzureTableAsync(string longitude, string latitude, string imageurl, ILogger log)
        {
            try
            {
                PlantHealthDeatils details = new PlantHealthDeatils($"{tableName}", $"{tableName}{DateTime.Now:dd-MM-yyyy-HH-mm-ss}")
                {
                    ImageURL = imageurl,
                    longitude = longitude,
                    latitude = latitude,
                    CapturedTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE),
                    TodayDate = DateTime.Today.Date
                };

                TableOperation tableoperations = TableOperation.Insert(details);
                TableResult operationresult = await table.ExecuteAsync(tableoperations);
                return true;
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return false;
            }
        }

        private static async Task<PlantHealthCustomVisionModel> GetProbabilityValuesFromCustomVisionRestApiAsync(string fileurl)
        {
            using (var client = new RestClient(predictionUrl))
            {
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddHeader("Prediction-Key", "5e59072f082048dbb880ce37fdf9a782");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $@"{{""Url"": ""{fileurl}""}}", ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<PlantHealthCustomVisionModel>(response.Content.ToString());
            }
        }
    }
}
