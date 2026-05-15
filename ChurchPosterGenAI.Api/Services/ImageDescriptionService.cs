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
            string apiKey = _config["OpenRouter:ApiKey"] ?? throw new Exception("OpenRouter API key not found");

            // Download image and convert to base64
            using var downloadClient = new HttpClient();
            var imageBytes = await downloadClient.GetByteArrayAsync(imageUrl);
            var base64Image = Convert.ToBase64String(imageBytes);

            string prompt = @"You are an expert image analyst assisting a two-stage AI image generation pipeline.
            Your output will be fed directly into a text-to-image diffusion model (FLUX.1) as its sole prompt.
            Describe every single visual and textual element in the image with exhaustive precision so the diffusion model can recreate it pixel-accurately.

            STRUCTURE YOUR DESCRIPTION AS FLOWING PROSE covering these aspects in order:

            1. OVERALL LAYOUT & COMPOSITION
            - Aspect ratio, orientation (portrait/landscape/square)
            - How the space is divided (top/middle/bottom zones, left/right split, layering order)
            - Visual hierarchy: what dominates, what is secondary

            2. BACKGROUND
            - Base color(s), gradients, textures, patterns
            - Any photographic background elements, their position, blur level
            - Vignettes, overlays, color washes

            3. PHOTOGRAPHIC / ILLUSTRATED ELEMENTS
            - Every object, person, or illustration: exact position (top-center, lower-left, etc.)
            - Physical description: shape, material, color, finish (matte/glossy/metallic)
            - Lighting on each object: direction, intensity, shadows, reflections, glow
            - Overlapping relationships between elements

            4. DECORATIVE & GRAPHIC ELEMENTS
            - Shapes, lines, borders, ribbons, fabric, smoke, particles, sparkles
            - Their color, opacity, position, size relative to canvas
            - Any 3D vs flat appearance

            5. ALL TEXT CONTENT — EXACT AND COMPLETE
            - Every single word and phrase, reproduced exactly as written
            - For each text block: exact wording, position on canvas (e.g. top-center, bottom-left strip),
                font style (serif/sans-serif/display/script), weight (thin/regular/bold/black),
                approximate size relative to canvas (massive/large/medium/small/tiny),
                color, any outline/shadow/gradient/texture applied to the letters,
                letter spacing (tight/normal/wide/extremely wide), capitalization style

            6. COLOR PALETTE
            - List the 4–6 dominant hex-approximate colors and where each appears

            7. LIGHTING & MOOD
            - Overall lighting style: dramatic, soft, cinematic, flat, etc.
            - Light source direction and color temperature
            - Mood and atmosphere conveyed

            8. STYLE CLASSIFICATION
            - Photorealistic, 3D render, illustrated, flat design, mixed-media, etc.
            - Any specific design era or aesthetic (modern, vintage, luxury, etc.)

            Return only the description as plain flowing prose. No bullet points, no headers, no markdown, no commentary — just one continuous richly detailed paragraph or series of paragraphs that reads like a complete visual brief.";

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