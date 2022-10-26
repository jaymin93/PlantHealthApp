using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using PlantHealthApp;
using RestSharp;

namespace PlantHealth.Function
{
    public class PlantHealthFuncApp
    {
        private static string predictionUrl;
        private static string storageAccountUri;
        private static string containerName;
        private static string tableName;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static string secretIdentifier;
        private static string secretIdentifierForPrediction;
        private static CloudStorageAccount storageAccount = null;
        private static CloudTableClient tableClient = null;
        private static CloudTable table = null;
        private static string predictionEndpointsecret = string.Empty;
        private static KeyVaultClient client = null;
        private static Microsoft.Azure.KeyVault.Models.SecretBundle connectionstring = null;
        private static Microsoft.Azure.KeyVault.Models.SecretBundle prediction = null;

        [FunctionName("PlantHealthFuncApp")]
        public static async Task Run([BlobTrigger("planthealthcontainer/{name}", Connection = "planthealthapp_STORAGE")] Stream myBlob, string name, ILogger log)
        {
            SetClientIDAndSecret();
            client ??= new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(TokenHelper.GetAccessTokenAsync));
            connectionstring ??= await client.GetSecretAsync(secretIdentifier);
            prediction ??= await client.GetSecretAsync(secretIdentifierForPrediction);
            predictionEndpointsecret = prediction.Value;
            storageAccount ??= CloudStorageAccount.Parse(connectionstring.Value);
            tableClient ??= storageAccount.CreateCloudTableClient();
            table ??= tableClient.GetTableReference(tableName);
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
                request.AddHeader("Prediction-Key", predictionEndpointsecret);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $@"{{""Url"": ""{fileurl}""}}", ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<PlantHealthCustomVisionModel>(response.Content.ToString());
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
            predictionUrl ??= GetEnviromentValue("predictionUrl");
            storageAccountUri ??= GetEnviromentValue("storageAccountUri");
            containerName ??= GetEnviromentValue("containerName");
            tableName ??= GetEnviromentValue("tableName");
            secretIdentifier ??= GetEnviromentValue("secretIdentifier");
            secretIdentifierForPrediction ??= GetEnviromentValue("secretIdentifierForPrediction");
        }
    }
}

