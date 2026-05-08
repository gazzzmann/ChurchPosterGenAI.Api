using System.Net.Http.Headers;
using System.Text.Json;

namespace ChurchPosterGenAI.Api.Services
{
    public class ImageDescriptionService
    {
        private readonly IConfiguration _config;

        public ImageDescriptionService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> DescribeImageAsync(string imageUrl)
        {
            DotNetEnv.Env.Load();
            string apiKey = _config["OpenRouter:ApiKey"] ?? throw new Exception("OpenRouter API key not found");

            // Download image and convert to base64
            using var downloadClient = new HttpClient();
            var imageBytes = await downloadClient.GetByteArrayAsync(imageUrl);
            var base64Image = Convert.ToBase64String(imageBytes);

            string prompt = @"You are an expert image analyst assisting an AI image generation pipeline.
            Describe the image in exhaustive visual detail so that an AI model (like FLUX.1) can recreate it accurately from text alone.
            Ignore all text, words, logos, and typography in the image — do not describe or reference them in any way.

            Your description must cover:
            - Overall composition and layout (e.g. centered subject, split layout, full bleed background)
            - Color palette: dominant colors, gradients, overlays, and contrast levels
            - Background: solid color, texture, pattern, photo, bokeh, gradient — be specific
            - Foreground elements: people, objects, icons, illustrations — describe appearance, clothing, pose, expression
            - Decorative elements: borders, shapes, lines, glows, shadows, sparkles, overlays
            - Lighting and mood: bright, dark, warm, dramatic, soft, cinematic
            - Style: photorealistic, illustrated, flat design, 3D render, painterly, minimalist

            Return only the description. No commentary, no labels, no markdown — plain flowing prose only. Be extremely specific about every single visual detail.";

            var requestBody = new
            {
                model = "openai/gpt-4o",
                max_tokens = 900,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } },
                            new { type = "text", text = prompt }
                        }
                    }
                }
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenRouter API error {(int)response.StatusCode}: {result}");

            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                throw new Exception($"Unexpected API response structure: {result}");

            var text = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text?.Trim() ?? throw new Exception("No description returned from model");
        }
    }
}