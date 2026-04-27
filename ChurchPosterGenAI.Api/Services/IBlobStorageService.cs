using Microsoft.AspNetCore.Http;

namespace ChurchPosterGenAI.Api.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile image, string category);
    }
}