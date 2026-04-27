using System.Text.Json;

namespace ChurchPosterGenAI.Api.Services
{
    public class AIImageService : IAIImageService
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AIImageService(
            IHttpClientFactory factory,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = factory.CreateClient("OpenAI");
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<string> GenerateFromImageAsync(
     string imageUrl,
     string prompt)
        {
            var finalPrompt = $@"
Create a premium church flyer.

{prompt}

Requirements:
- modern church flyer
- premium typography
- white and blue theme
- clean layout
- spiritual atmosphere
- print ready
- high-end design
";

            var payload = new
            {
                model = "gpt-image-1",
                prompt = finalPrompt,
                size = "1536x1024"
            };

            var response = await _httpClient.PostAsJsonAsync(
                "images/generations",
                payload);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI Error: {body}");

            var json = JsonDocument.Parse(body);

            var base64 = json.RootElement
                .GetProperty("data")[0]
                .GetProperty("b64_json")
                .GetString();

            var bytes = Convert.FromBase64String(base64!);

            var fileName = $"{Guid.NewGuid():N}.png";

            var folder = Path.Combine(
                _env.WebRootPath,
                "images",
                "generated");

            Directory.CreateDirectory(folder);

            var fullPath = Path.Combine(folder, fileName);

            await File.WriteAllBytesAsync(fullPath, bytes);

            return BuildPublicUrl(fileName);
        }

        private string BuildPublicUrl(string fileName)
        {
            var request = _httpContextAccessor.HttpContext!.Request;

            return $"{request.Scheme}://{request.Host}/images/generated/{fileName}";
        }
    }
}

// ===============================
// AIImageService.cs
// Saves real PNG into wwwroot/generated
// Returns public URL
// ===============================