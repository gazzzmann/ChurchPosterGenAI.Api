using System.Text;
using System.Text.Json;
namespace ChurchPosterGenAI.Api.Services
{
    public class AIImageService : IAIImageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AIImageService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }
        // ...


        public async Task<string> GenerateFromImageAsync(string imageUrl, string prompt)
        {
            // STEP 1: Get a detailed description of the uploaded image
            var descriptionService = new ImageDescriptionService(_config);
            var imageDescription = await descriptionService.DescribeImageAsync(imageUrl);

            // STEP 2: Build an enriched prompt that merges description + user instruction
            var enrichedPrompt = BuildEnrichedPrompt(imageDescription, prompt);

            // STEP 3: Send to FLUX as text-to-image
            var payload = new { inputs = enrichedPrompt };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var hfClient = _httpClientFactory.CreateClient("HuggingFace");
            const string modelId = "black-forest-labs/FLUX.1-schnell";
            var endpoint = $"https://router.huggingface.co/hf-inference/models/{modelId}";

            hfClient.DefaultRequestHeaders.Add("X-Wait-For-Model", "true");

            var response = await hfClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"HuggingFace Error ({(int)response.StatusCode}): {error}");
            }

            var generatedBytes = await response.Content.ReadAsByteArrayAsync();

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "TempGenerated");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}.png";
            var fullPath = Path.Combine(folder, fileName);

            await File.WriteAllBytesAsync(fullPath, generatedBytes);
            return fullPath;
        }

        private string BuildEnrichedPrompt(string imageDescription, string userInstruction)
        {
            return $"""
                You must generate an image based on the following visual description, but apply the user's modification to it.

                ORIGINAL IMAGE DESCRIPTION:
                {imageDescription}

                USER MODIFICATION REQUEST:
                {userInstruction}

                Apply the modification faithfully while keeping everything else in the original description exactly the same.
                High quality, photorealistic, detailed.
                """;
        }
            }
        }