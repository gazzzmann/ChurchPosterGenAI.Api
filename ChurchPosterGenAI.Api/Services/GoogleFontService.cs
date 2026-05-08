using SixLabors;
using SixLabors.Fonts;

namespace ChurchPosterGenAI.Api.Services
{
    public class GoogleFontService
    {
        private readonly HttpClient _httpClient;
        private readonly string _fontCacheDir;
        private readonly string _googleFontsApiKey;

        public GoogleFontService(IHttpClientFactory factory, IConfiguration config)
        {
            _httpClient = factory.CreateClient();
            _fontCacheDir = Path.Combine(Directory.GetCurrentDirectory(), "FontCache");
            _googleFontsApiKey = config["GoogleFonts:ApiKey"] ?? "";
            Directory.CreateDirectory(_fontCacheDir);
        }

        public async Task<FontFamily> GetFontAsync(FontCollection collection, string fontName, string weight = "Bold")
        {
            // Check cache first
            var safeName = fontName.Replace(" ", "_");
            var cachedPath = Path.Combine(_fontCacheDir, $"{safeName}-{weight}.ttf");

            if (File.Exists(cachedPath))
                return collection.Add(cachedPath);

            // Fetch download URL from Google Fonts API
            var apiUrl = $"https://fonts.googleapis.com/css2?family={Uri.EscapeDataString(fontName)}:wght@{MapWeight(weight)}&display=swap";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var css = await _httpClient.GetStringAsync(apiUrl);

            // Extract .ttf/.woff2 URL from CSS response
            var urlMatch = System.Text.RegularExpressions.Regex.Match(css, @"src:\s*url\(([^)]+\.(?:ttf|woff2))\)");
            if (!urlMatch.Success)
                throw new Exception($"Could not find font file URL for '{fontName}' in Google Fonts CSS");

            var fontUrl = urlMatch.Groups[1].Value;
            var fontBytes = await _httpClient.GetByteArrayAsync(fontUrl);
            await File.WriteAllBytesAsync(cachedPath, fontBytes);

            return collection.Add(cachedPath);
        }

        private static string MapWeight(string weight) => weight switch
        {
            "Bold" => "700",
            "Light" => "300",
            "Black" => "900",
            _ => "400"
        };
    }
}