
namespace ChurchPosterGenAI.Api.Services;

public interface IAIImageService
{
    Task<string> GenerateFromImageAsync(string imageUrl, string prompt);
}