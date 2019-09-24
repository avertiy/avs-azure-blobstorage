using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BlobStorageWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobStorageWebApp.Helpers
{
    public class AzureStorageClient
    {
        private readonly WebAppConfig _storageConfig;
        public AzureStorageClient(WebAppConfig config)
        {
            _storageConfig = config;
        }
        
        public async Task<bool> AttachDepositDocumentAsync(string userId, IFormFile file)
        {
            // Get the reference to the block blob from the container
            CloudBlobContainer container = await GetContainerAsync(_storageConfig.DepositDocsContainer);

            var fileName = $"{userId}/{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            try
            {
                using (var fileStream = file.OpenReadStream())
                {
                    // Upload the file
                    await blockBlob.UploadFromStreamAsync(fileStream);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<byte[]> DownloadDepositDocumentAsync(string userId, string blobName)
        {
            CloudBlobContainer container = await GetContainerAsync(_storageConfig.DepositDocsContainer);
            
            CloudBlockBlob blob = container.GetBlockBlobReference($"{userId}/{blobName}");
            
            using (MemoryStream memstream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memstream).ConfigureAwait(false);
                return memstream.ToArray();
            }
        }

        public async Task<List<string>> GetDepositDocumentUrls(string userId)
        {
            var urls = new List<string>();
            
            CloudBlobContainer container = await GetContainerAsync(_storageConfig.DepositDocsContainer);
            
            BlobContinuationToken token = null;

            //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
            //When the continuation token is null, the last page has been returned and execution can exit the loop.
            do
            {
                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                //or by calling a different overload.
                var resultSegment = await container.ListBlobsSegmentedAsync($"{userId}/", true, BlobListingDetails.All, 10, token, null, null);

                // Blob type could be CloudBlockBlob  [or CloudPageBlob or CloudBlobDirectory]
                foreach (IListBlobItem blob in resultSegment.Results)
                {
                    urls.Add(blob.StorageUri.PrimaryUri.ToString());
                }

                //Get the continuation token.
                token = resultSegment.ContinuationToken;
            }

            while (token != null);

            return await Task.FromResult(urls);
        }

        public async Task<List<string>> GetDepositDocumentBlobNames(string userId)
        {
            var list = new List<string>();

            CloudBlobContainer container = await GetContainerAsync(_storageConfig.DepositDocsContainer);

            BlobContinuationToken token = null;

            //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
            //When the continuation token is null, the last page has been returned and execution can exit the loop.
            do
            {
                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                //or by calling a different overload.
                var resultSegment = await container.ListBlobsSegmentedAsync($"{userId}/", true, BlobListingDetails.All, 10, token, null, null);

                // Blob type could be CloudBlockBlob  [or CloudPageBlob or CloudBlobDirectory]
                foreach (IListBlobItem blob in resultSegment.Results)
                {
                    if (!(blob is CloudBlockBlob bb))
                        throw new Exception($"CloudBlockBlob is expected [Container {_storageConfig.DepositDocsContainer}]");
                    list.Add(bb.Name.Replace($"{userId}/",""));
                }

                //Get the continuation token.
                token = resultSegment.ContinuationToken;
            }

            while (token != null);

            return await Task.FromResult(list);
        }


        private CloudBlobContainer GetContainer(string container)
        {
            CloudBlobClient blockBlob = CreateBlobClient();
            return blockBlob.GetContainerReference(container);
        }
        
        private CloudBlobClient CreateBlobClient()
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }

        private async Task<CloudBlobContainer> GetContainerAsync(string containerName)
        {
            // Create blob client
            CloudBlobClient blobClient = CreateBlobClient();
            // Get reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            await container.CreateIfNotExistsAsync();

            //set access permissions let's make it public as the demo app converts images to thumbnails
            // Read the existing permissions first so that we have all container permissions. 
            // This ensures that we do not inadvertently remove any shared access policies while setting the public access level.
            BlobContainerPermissions permissions = await container.GetPermissionsAsync();
            // Set the container's public access level.
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            await container.SetPermissionsAsync(permissions);

            return container;
        }

        private async Task DeleteContainers()
        {
            // Delete the blob and its snapshots.
            CloudBlobContainer container = GetContainer(_storageConfig.IdentificationDocsContainer);
            await container.DeleteIfExistsAsync();

            container = GetContainer(_storageConfig.DepositDocsContainer);
            await container.DeleteIfExistsAsync();
        }

    }
}



//public async Task<List<byte[]>> DownloadDocumentsAsync(string userId)
//{
//    var files = new List<byte[]>();

//    CloudBlobContainer container = await GetContainerAsync(_storageConfig.DepositDocsContainer);

//    BlobContinuationToken token = null;

//    //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
//    //When the continuation token is null, the last page has been returned and execution can exit the loop.
//    do
//    {
//        //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
//        //or by calling a different overload.
//        var resultSegment = await container.ListBlobsSegmentedAsync($"{userId}/", true, BlobListingDetails.All, 10, token, null, null);



//        //blockBlob.DownloadToStreamAsync()
//        // Blob type could be CloudBlockBlob  [or CloudPageBlob or CloudBlobDirectory]
//        foreach (IListBlobItem blob in resultSegment.Results)
//        {
//            var bb = blob as CloudBlockBlob;
//            if(bb == null)
//                throw new Exception($"CloudBlockBlob is expected [Container {_storageConfig.DepositDocsContainer}]");

//            MemoryStream memstream = new MemoryStream();
//            await bb.DownloadToStreamAsync(memstream).ConfigureAwait(false);
//            files.Add(memstream.ToArray());
//        }

//        //Get the continuation token.
//        token = resultSegment.ContinuationToken;
//    }

//    while (token != null);

//    return files;
//}