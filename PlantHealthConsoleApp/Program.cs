using Azure.Storage.Blobs;
using Microsoft.Azure.KeyVault;
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
        static readonly string secretIdentifier = "https://planthealthappsecret.vault.azure.net/secrets/storageAccountConnectionString/92f4ed20ff4041ae8b05303f7baf79f7";
        static readonly string imageDirPath = "imageDir";

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
            FileSystemWatcher watcher = new()
            {
                Path = GetDirectoryForImageUpload()
            };
            watcher.Created += FileSystemWatcher_FileCreatedEvent;
            watcher.EnableRaisingEvents = true;
        }

        private static string GetDirectoryForImageUpload()
        {
            string path = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),imageDirPath) }";
            Console.WriteLine($"path is {path}");
            CreateDirectoryIfNotExist(path);
            return path;
        }

        private static void CreateDirectoryIfNotExist(string DirectoryPath)
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
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
                    var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(TokenHelper.GetAccessTokenAsync));
                    var secret = await client.GetSecretAsync(secretIdentifier);
                    connectionstring = secret.Value;
                    storageaccounturi = GetvaluesFromConfig("storageaccounturi");
                    containername = GetvaluesFromConfig("containername");
                    if (!string.IsNullOrEmpty(fileSystemEvent.Name))
                    {
                        await UploadFileToAzureStorageAsync(connectionstring, fileSystemEvent.Name, containername, fileStream);
                    }
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
