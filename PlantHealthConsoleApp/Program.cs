using Azure.Storage.Blobs;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace PlantHealthConsoleApp
{
    public class Program
    {
        private static IConfiguration? config;
        private static string? connectionstring;
        private static string? storageaccounturi;
        private static string? containername;
        private static string secretIdentifier = string.Empty;
        private static string imageDirPath = string.Empty;
        private static SecretBundle? secret;
        private static KeyVaultClient? client;

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
            imageDirPath = GetvaluesFromConfig("imageDirPath");
            FileSystemWatcher watcher = new()
            {
                Path = GetDirectoryForImageUpload()
            };
            watcher.Created += FileSystemWatcher_FileCreatedEvent;
            watcher.EnableRaisingEvents = true;
        }

        private static string GetDirectoryForImageUpload()
        {
            string path = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), imageDirPath)}";
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

        private static void SetClientIDAndSecret()
        {
            TokenHelper.clientID ??= GetvaluesFromConfig("clientID");
            TokenHelper.clientSecret ??= GetvaluesFromConfig("clientSecret");
        }
        private async static void FileSystemWatcher_FileCreatedEvent(object sender, FileSystemEventArgs fileSystemEvent)
        {
            using (FileStream fileStream = new(fileSystemEvent.FullPath, FileMode.Open))
            {
                try
                {
                    storageaccounturi = GetvaluesFromConfig("storageaccounturi");
                    containername = GetvaluesFromConfig("containername");
                    secretIdentifier = GetvaluesFromConfig("secretIdentifier");
                    SetClientIDAndSecret();
                    client ??= new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(TokenHelper.GetAccessTokenAsync));
                    secret ??= await client.GetSecretAsync(secretIdentifier);
                    connectionstring ??= secret.Value;
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
