using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models; // Add this using directive at the top
using Microsoft.AspNetCore.Http;

namespace ChurchPosterGenAI.Api.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "posters";

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureBlob:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(IFormFile image, string category)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            string folderName = category.ToLower().Replace(" ", "-");
            string uniqueBlobName = $"{folderName}/{Guid.NewGuid()}-{image.FileName}";

            var blobClient = containerClient.GetBlobClient(uniqueBlobName);

            // Create the options object and map the ContentType from the incoming file
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = image.ContentType // This will be "image/jpeg", "image/png", etc.
                }
            };

            using (var stream = image.OpenReadStream())
            {
                // Pass the options into the upload method
                await blobClient.UploadAsync(stream, uploadOptions);
            }

            return blobClient.Uri.ToString();
        }
    }
}