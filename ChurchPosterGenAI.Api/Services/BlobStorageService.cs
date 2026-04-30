using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ChurchPosterGenAI.Api.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "posters";

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureBlob:ConnectionString"];

            _blobServiceClient =
                new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(
            IFormFile image,
            string category)
        {
            using var stream = image.OpenReadStream();

            return await UploadStreamAsync(
                stream,
                image.FileName,
                category,
                image.ContentType);
        }

        public async Task<string> UploadBytesAsync(
            byte[] bytes,
            string fileName,
            string category,
            string contentType)
        {
            using var stream =
                new MemoryStream(bytes);

            return await UploadStreamAsync(
                stream,
                fileName,
                category,
                contentType);
        }

        private async Task<string> UploadStreamAsync(
            Stream stream,
            string fileName,
            string category,
            string contentType)
        {
            var container =
                _blobServiceClient
                .GetBlobContainerClient(_containerName);

            await container.CreateIfNotExistsAsync();

            var folder =
                category.ToLower().Replace(" ", "-");

            var blobName =
                $"{folder}/{Guid.NewGuid()}-{fileName}";

            var blobClient =
                container.GetBlobClient(blobName);

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders =
                        new BlobHttpHeaders
                        {
                            ContentType = contentType
                        }
                });

            return blobClient.Uri.ToString();
        }
    }
}