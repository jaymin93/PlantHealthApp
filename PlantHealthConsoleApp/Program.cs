using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlantHealthConsoleApp
{
    public class Program
    {
        static IConfiguration? config;

        static string? connectionstring;

        static string? storageaccounturi;

        static string? containername;
        public async static Task Main(string[] args)
        {
            HostBuilder builder = new HostBuilder();

            config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", true, true)
             .Build();
            CheckForNewFileAdditionToDirectory();

            await builder.RunConsoleAsync();
        }

        private static void CheckForNewFileAdditionToDirectory()
        {
            FileSystemWatcher watcher = new();
            watcher.Path = GetvaluesFromConfig("imageDir");
            watcher.Created += FileSystemWatcher_FileCreatedEvent;
            watcher.EnableRaisingEvents = true;
        }

        private static string GetvaluesFromConfig(string configName)
        {
            if (!string.IsNullOrEmpty(configName) && config is not null)
            {
                return config[configName];
            }
            return string.Empty;
        }

        private async static void FileSystemWatcher_FileCreatedEvent(object sender, FileSystemEventArgs fileSystemEvent)
        {
            using (FileStream fileStream = new(fileSystemEvent.FullPath, FileMode.Open))
            {
                try
                {
                    connectionstring = GetvaluesFromConfig("connectionstring");
                    storageaccounturi = GetvaluesFromConfig("storageaccounturi");
                    containername = GetvaluesFromConfig("containername");

                    await UploadFileToAzureStorageAsync( connectionstring,fileSystemEvent?.Name,containername, fileStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static async Task<bool> UploadFileToAzureStorageAsync(string connectionString, string fileName, string containerName, Stream fileStream)
        {
            BlobClient blobClient = new BlobClient(connectionString, containerName, fileName);
            await blobClient.UploadAsync(fileStream);
            Console.WriteLine($"file {fileName} uploaded successfully");
            return await Task.FromResult(true);
        }
    }
}