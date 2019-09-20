using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
//using ServiceBus.Adapter.Extensions;
//using ServiceBus.Adapter.Interfaces;
//using ServiceBus.Adapter.Validators;
//using ServiceBus.Shared;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ServiceBus.Adapter
{
    public class UploadImportedData
    {
        private readonly ILogger<UploadImportedData> _logger;
        //private readonly IArticleService _articleService;
        private readonly string fileName = "data.json";
        private readonly string _blobContainerName = "testcontainer";
        private readonly string _blobDestinationContainerName = "destinationcontainer";


        //public UploadImportedData(ILogger<UploadImportedData> logger, IArticleService articleService)
        //{
        //    _logger = logger;
        //    _articleService = articleService;
        //}
        public UploadImportedData(ILogger<UploadImportedData> logger)
        {
            _logger = logger;

        }
        [FunctionName("UploadImpotedData")]
        public void Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                //PerformTasks().GetAwaiter().GetResult();
                //GetFullBlobsAsync().GetAwaiter();
                var eventCount = GetFullBlobsAsync().Result;

            }
            catch (Exception ex)
            {
            }
        }


        public async Task<int> GetFullBlobsAsync()
        {
            var cloudBlobDic = new List<Microsoft.Azure.Storage.Blob.CloudBlobDirectory>();
            string storageConnection = GetEnvironmentVariable("AzureWebJobsStorage");
            var count = 0;
            if (CloudStorageAccount.TryParse(storageConnection, out CloudStorageAccount cloudStorageAccount))
            {
                Microsoft.Azure.Storage.Blob.BlobContinuationToken dirToken = null;

                var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference(_blobContainerName);
                var blobList = await cloudBlobContainer.ListBlobsSegmentedAsync(string.Empty, false, Microsoft.Azure.Storage.Blob.BlobListingDetails.None, int.MaxValue, null, null, null);


                //var dirResult = await cloudBlobContainer.ListBlobsSegmentedAsync(dirToken);
                //dirToken = dirResult.ContinuationToken;



                foreach (var dirItem in blobList.Results)
                {
                    if (dirItem is Microsoft.Azure.Storage.Blob.CloudBlobDirectory)
                    {
                        var dir = dirItem as Microsoft.Azure.Storage.Blob.CloudBlobDirectory;
                        Microsoft.Azure.Storage.Blob.BlobContinuationToken blobToken = null;
                        var blobResult = await dir.ListBlobsSegmentedAsync(blobToken);
                        foreach (var blobItem in blobResult.Results)
                        {
                            if (blobItem is Microsoft.Azure.Storage.Blob.CloudBlockBlob)
                            {
                                var blob = blobItem as Microsoft.Azure.Storage.Blob.CloudBlockBlob;
                                var filename = blob.Name;
                                string[] multiArray = blob.Name.Split(new Char[] { '/' });
                                var content = await blob.DownloadTextAsync();
                                count++;
                                using (var writer = new StreamWriter(string.Format("{0:D5}.xml", count)))
                                {
                                    // await writer.WriteAsync(content);
                                    await UploadFile(content, multiArray[1], multiArray[0]);
                                }
                                Console.WriteLine(count);
                                //if (count >= maxEvents)
                                //    break;
                            }
                        }
                    }
                }
            }
            // return (List<Microsoft.Azure.Storage.Blob.CloudBlobDirectory>)blobList.Results;
            return count;
        }
        //[FunctionName("UploadImpotedData")]
        //public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req)
        //{
        //    // log.LogInformation("C# HTTP trigger function processed a request.");
        //    try
        //    {
        //        var httpClient = new HttpClient();
        //        var getTheImportedData = await httpClient.GetStringAsync("https://webapi20190903093807.azurewebsites.net/api/demo");

        //        //  var getTheImportedData = await _articleService.GetImportedData();
        //        string storageConnection = GetEnvironmentVariable("AzureWebJobsStorage");
        //        if (!string.IsNullOrWhiteSpace(getTheImportedData))
        //        {
        //            var stream = ConvertToStream(getTheImportedData);
        //            if (stream.Length != 0)
        //            {
        //                //if (CloudStorageAccount.TryParse(storageConnection, out CloudStorageAccount cloudStorageAccount))
        //                //{
        //                //    var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        //                //    var cloudBlobContainer = cloudBlobClient.GetContainerReference(_blobContainerName);
        //                //    var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
        //                //    await cloudBlockBlob.UploadFromStreamAsync(stream);
        //                //    // _logger.LogInformation(SuccessMessages.ImportedFileHasBeenUploaded);
        //                //    _logger.LogInformation("SuccessMessages");
        //                //    req.CreateResponse(HttpStatusCode.OK, "Success");
        //                //}
        //                //else
        //                //{
        //                //    _logger.LogError("ErrorMessages");
        //                //    req.CreateErrorResponse(HttpStatusCode.BadRequest, "Error");
        //                //    //_logger.LogError(ErrorMessages.NoStorageConnectionStringAvailable);
        //                //}

        //                CloudStorageAccount storageacc = CloudStorageAccount.Parse(storageConnection);

        //                //Create Reference to Azure Blob
        //                CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();

        //                //The next 2 lines create if not exists a container named "democontainer"
        //                CloudBlobContainer container = blobClient.GetContainerReference("democontainer");
        //                // "democontainer" + DateTime.Now + ".csv"
        //                await container.CreateIfNotExistsAsync();

        //                //The next 7 lines upload the file test.txt with the name DemoBlob on the container "democontainer"
        //                CloudBlockBlob blockBlob = container.GetBlockBlobReference("DemoBlob");
        //                //blockBlob.Properties.ContentType = ".csv";

        //                await blockBlob.UploadFromStreamAsync(stream);
        //                _logger.LogInformation("SuccessMessages");
        //                req.CreateResponse(HttpStatusCode.OK, "Success");

        //            }
        //            else
        //            {

        //                _logger.LogError("ErrorMessages");
        //                req.CreateErrorResponse(HttpStatusCode.BadRequest, "Error");

        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        req.CreateErrorResponse(HttpStatusCode.BadRequest, "Error !");
        //    }
        //    return null;

        //}
        private async Task<bool> PerformTasks()
        {
            try
            {
                var httpClient = new HttpClient();
                var getTheImportedData = await httpClient.GetStringAsync("https://webapi20190903093807.azurewebsites.net/api/demo");

                //  var getTheImportedData = await _articleService.GetImportedData();
                string storageConnection = GetEnvironmentVariable("AzureWebJobsStorage");
                if (!string.IsNullOrWhiteSpace(getTheImportedData))
                {
                    var stream = ConvertToStream(getTheImportedData);
                    if (stream.Length != 0)
                    {
                        if (CloudStorageAccount.TryParse(storageConnection, out CloudStorageAccount cloudStorageAccount))
                        {

                            // CloudStorageAccount storageacc = CloudStorageAccount.Parse(storageConnection);

                            // //Create Reference to Azure Blob
                            // CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();

                            // //The next 2 lines create if not exists a container named "democontainer"
                            // CloudBlobContainer container = blobClient.GetContainerReference("democontainer");
                            // // "democontainer" + DateTime.Now + ".csv"
                            // await container.CreateIfNotExistsAsync();

                            // //The next 7 lines upload the file test.txt with the name DemoBlob on the container "democontainer"
                            // CloudBlockBlob blockBlob = container.GetBlockBlobReference("democontainer_" + DateTime.Now);
                            //// blockBlob.Properties.ContentType = "application/json";

                            // await blockBlob.UploadFromStreamAsync(stream);



                            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                            var cloudBlobContainer = cloudBlobClient.GetContainerReference(_blobContainerName);
                            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                            await cloudBlockBlob.UploadFromStreamAsync(stream);
                            _logger.LogInformation("SuccessMessages");

                        }
                        else
                        {
                            _logger.LogError("ErrorMessages");
                            //_logger.LogError(ErrorMessages.NoStorageConnectionStringAvailable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return false;
        }

        private async Task<bool> UploadFile(string getTheImportedData, string destinationfileName, string destinationDirectoryName)
        {
            try
            {


                string storageConnection = GetEnvironmentVariable("AzureWebJobsStorageDestination");
                if (!string.IsNullOrWhiteSpace(getTheImportedData))
                {
                    var stream = ConvertToStream(getTheImportedData);
                    if (stream.Length != 0)
                    {
                        if (CloudStorageAccount.TryParse(storageConnection, out CloudStorageAccount cloudStorageAccount))
                        {




                            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                            var cloudBlobContainer = cloudBlobClient.GetContainerReference(_blobDestinationContainerName);
                            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(destinationfileName);


                            var destinationdirectory = cloudBlobContainer.GetDirectoryReference(destinationDirectoryName);


                            var blobDestinationList = await cloudBlobContainer.ListBlobsSegmentedAsync(string.Empty, false, Microsoft.Azure.Storage.Blob.BlobListingDetails.None, int.MaxValue, null, null, null);


                            //    bool directoryExists = destinationdirectory.GetDirectoryReference(destinationDirectoryName).ListBlobs()
                            //bool directoryExists = blobDirectory.ListBlobs().Count() > 0



                            await cloudBlockBlob.UploadFromStreamAsync(stream);
                            _logger.LogInformation("SuccessMessages");

                        }
                        else
                        {
                            _logger.LogError("ErrorMessages");
                            //_logger.LogError(ErrorMessages.NoStorageConnectionStringAvailable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return false;
        }

        private static string GetEnvironmentVariable(string name)
        {
            return
             System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
        public static Stream ConvertToStream(string value) => new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));

    }
}