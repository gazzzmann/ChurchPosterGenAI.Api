using System.Net.Http.Headers;
using System.Text.Json;

namespace ChurchPosterGenAI.Api.Services
{
    public class PosterLayout
    {
        public string BackgroundStyle { get; set; } = "";       // "dark overlay", "gradient", "photo"
        public string PrimaryColor { get; set; } = "";          // "#FF0000"
        public string SecondaryColor { get; set; } = "";        // "#FFFFFF"
        public string AccentColor { get; set; } = "";           // "#FFD700"
        public List<TextLayer> TextLayers { get; set; } = [];
        public LogoPlacement Logo { get; set; } = new();
        public InfoBar? BottomBar { get; set; }
    }

    public class TextLayer
    {
        public string Content { get; set; } = "";               // actual text
        public string GoogleFont { get; set; } = "";            // "Bebas Neue"
        public int FontSize { get; set; }                       // relative: 1-100
        public string Color { get; set; } = "";                 // "#FF0000"
        public string Weight { get; set; } = "";                // "Bold", "Regular"
        public string Alignment { get; set; } = "";             // "Center", "Left", "Right"
        public string VerticalPosition { get; set; } = "";      // "Top", "Middle", "Bottom"
        public float XPercent { get; set; }                     // 0.0 - 1.0 of image width
        public float YPercent { get; set; }                     // 0.0 - 1.0 of image height
        public string Role { get; set; } = "";                  // "Title", "Subtitle", "Scripture", "Detail"
    }

    public class LogoPlacement
    {
        public string Position { get; set; } = "";              // "TopLeft", "TopCenter", "TopRight"
        public float XPercent { get; set; }
        public float YPercent { get; set; }
    }

    public class InfoBar
    {
        public string BackgroundColor { get; set; } = "";       // "#CC0000"
        public string TextColor { get; set; } = "";             // "#FFFFFF"
        public string Position { get; set; } = "";              // "Bottom"
        public List<string> Fields { get; set; } = [];          // ["Date", "Venue", "Time"]
    }

    public class PosterLayoutAnalyzerService
    {
        private readonly IConfiguration _config;

        public PosterLayoutAnalyzerService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<PosterLayout> AnalyzeLayoutAsync(string imagePath)
        {
            DotNetEnv.Env.Load();
            string apiKey = _config["OpenRouter:ApiKey"] ?? throw new Exception("Key not found");

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string base64Image = Convert.ToBase64String(imageBytes);

            string prompt = @"You are a professional graphic design analyst. Analyze this church poster image and extract its complete layout specification.

Return ONLY a valid JSON object matching this exact structure — no markdown, no explanation:

{
  ""backgroundStyle"": ""<describe: solid color | gradient | photo with overlay | texture>"",
  ""primaryColor"": ""<dominant hex color>"",
  ""secondaryColor"": ""<secondary hex color>"",
  ""accentColor"": ""<accent hex color>"",
  ""textLayers"": [
    {
      ""content"": ""<exact text as seen>"",
      ""googleFont"": ""<closest matching Google Font name>"",
      ""fontSize"": <relative size 1-100>,
      ""color"": ""<hex color>"",
      ""weight"": ""<Bold|Regular|Light>"",
      ""alignment"": ""<Left|Center|Right>"",
      ""verticalPosition"": ""<Top|Middle|Bottom>"",
      ""xPercent"": <0.0-1.0>,
      ""yPercent"": <0.0-1.0>,
      ""role"": ""<Title|Subtitle|Scripture|Detail|Label>""
    }
  ],
  ""logo"": {
    ""position"": ""<TopLeft|TopCenter|TopRight>"",
    ""xPercent"": <0.0-1.0>,
    ""yPercent"": <0.0-1.0>
  },
  ""bottomBar"": {
    ""backgroundColor"": ""<hex>"",
    ""textColor"": ""<hex>"",
    ""position"": ""Bottom"",
    ""fields"": [""<field1>"", ""<field2>"", ""<field3>""]
  }
}

Rules:
- googleFont must be a real Google Fonts name (e.g. 'Bebas Neue', 'Montserrat', 'Playfair Display', 'Anton', 'Oswald', 'Lora')
- List ALL visible text layers, ordered top to bottom
- xPercent and yPercent are the top-left anchor of each text block as a fraction of image dimensions
- If no bottom bar exists, set bottomBar to null
- Return only the JSON object, nothing else";

            var requestBody = new
            {
                model = "openai/gpt-4o",
                max_tokens = 1200,
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
                throw new Exception($"Unexpected API response: {result}");

            var rawJson = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? throw new Exception("No content returned");

            // Strip markdown fences if GPT wraps it anyway
            rawJson = rawJson
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            return JsonSerializer.Deserialize<PosterLayout>(rawJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Failed to deserialize layout");
        }
    }
}