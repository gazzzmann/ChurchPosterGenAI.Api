using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace ChurchPosterGenAI.Api.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "posters";

        public BlobStorageService(IConfiguration configuration)
        {
            // This grabs the connection string you put in your secrets.json / appsettings
            string connectionString = configuration["AzureBlob:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(IFormFile image, string category)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Format category for the virtual folder (e.g., "Sunday Service" -> "sunday-service")
            string folderName = category.ToLower().Replace(" ", "-");
            string uniqueBlobName = $"{folderName}/{Guid.NewGuid()}-{image.FileName}";

            var blobClient = containerClient.GetBlobClient(uniqueBlobName);

            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }
    }
}