namespace ChurchPosterGenAI.Api.Services
{
    public class AIImageService : IAIImageService
    {
        private readonly HttpClient _httpClient;

        public AIImageService(
            IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("HuggingFace");
        }

        public async Task<string> GenerateFromImageAsync(
            string imageUrl,
            string prompt)
        {
            // STEP 1: Download reference image from Azure Blob URL
            var imageBytes =
                await _httpClient.GetByteArrayAsync(imageUrl);

            var base64Image =
                Convert.ToBase64String(imageBytes);

            // STEP 2: Build payload for Hugging Face
            var payload = new
            {
                inputs = prompt,
                parameters = new
                {
                    image = base64Image,
                    strength = 0.55,
                    guidance_scale = 8.5
                }
            };

            // STEP 3: Generate image
            var response =
                await _httpClient.PostAsJsonAsync(
                    "stabilityai/stable-diffusion-xl-base-1.0",
                    payload);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"HuggingFace Error: {error}");
            }

            var generatedBytes =
                await response.Content.ReadAsByteArrayAsync();

            // STEP 4: Save locally for testing
            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "TempGenerated");

            Directory.CreateDirectory(folder);

            var fileName =
                $"{Guid.NewGuid():N}.png";

            var fullPath = Path.Combine(
                folder,
                fileName);

            await File.WriteAllBytesAsync(
                fullPath,
                generatedBytes);

            // STEP 5: Return local file path
            return fullPath;
        }
    }
}