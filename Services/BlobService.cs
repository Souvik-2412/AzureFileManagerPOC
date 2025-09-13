using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFileManager.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobService(string connectionString, string containerName)
        {
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            // Generate a unique blob name
            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blobClient = _blobContainerClient.GetBlobClient(uniqueName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            try
            {
                await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders: blobHttpHeaders);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                // Log or handle error as needed
                throw new ApplicationException("File upload failed.", ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        public async Task DeleteFileAsync(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}