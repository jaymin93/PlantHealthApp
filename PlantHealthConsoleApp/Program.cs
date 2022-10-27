using Azure.Storage.Blobs;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using WinSCP;

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
        private static System.Timers.Timer? timer;
        private static string imageProcessPath = string.Empty;

        public async static Task Main(string[] args)
        {
            HostBuilder builder = new HostBuilder();

            config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", true, true)
             .Build();
            CheckForNewFileAdditionToDirectory();
            InitTimer();

            await builder.RunConsoleAsync();
        }
        private static void InitTimer()
        {
            timer ??= new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 60000;
            timer.Elapsed += Tmr_Elapsed;
        }

        private static void Tmr_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            GetFilesFromDronFTPServer(GetvaluesFromConfig("droneFtpUrl"), GetvaluesFromConfig("ftpUsername"), GetvaluesFromConfig("ftpPassword"), Convert.ToInt32(GetvaluesFromConfig("ftpport")));
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
            imageProcessPath = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), imageDirPath)}";
            Console.WriteLine($"path is {imageProcessPath}");
            CreateDirectoryIfNotExist(imageProcessPath);
            return imageProcessPath;
        }

        private static void CreateDirectoryIfNotExist(string DirectoryPath)
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
        }

        private static string GetvaluesFromConfig(string key)
        {
            if (!string.IsNullOrEmpty(key) && config is not null)
            {
                return config[key];
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

        private static void GetFilesFromDronFTPServer(string droneFtpUrl, string ftpUsername, string ftpPassword, int ftpport)
        {
            try
            {
                imageProcessPath ??= GetDirectoryForImageUpload();
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = droneFtpUrl,
                    UserName = ftpUsername,
                    Password = ftpPassword,
                    PortNumber = ftpport
                };
                using (Session session = new Session())
                {
                    string droneCapturedImagePath = "/home/prt85463/images";
                    session.Open(sessionOptions);
                    session.GetFiles(droneCapturedImagePath, imageProcessPath).Check();
                    session.RemoveFiles(droneCapturedImagePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
