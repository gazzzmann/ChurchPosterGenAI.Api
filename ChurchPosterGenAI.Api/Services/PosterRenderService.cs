using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace ChurchPosterGenAI.Api.Services
{
    public class PosterRenderService
    {
        private readonly GoogleFontService _fontService;

        public PosterRenderService(GoogleFontService fontService)
        {
            _fontService = fontService;
        }

        public async Task<string> RenderAsync(
            string backgroundImagePath,
            PosterLayout layout,
            PosterContent newContent,   // the new event details to inject
            string? logoPath = null)
        {
            using var image = await Image.LoadAsync<Rgba32>(backgroundImagePath);
            int W = image.Width, H = image.Height;

            var fontCollection = new FontCollection();

            // Load all unique fonts the layout needs
            var uniqueFonts = layout.TextLayers
                .Select(t => (t.GoogleFont, t.Weight))
                .Distinct();

            var fontFamilies = new Dictionary<string, FontFamily>();
            foreach (var (fontName, weight) in uniqueFonts)
            {
                var key = $"{fontName}-{weight}";
                if (!fontFamilies.ContainsKey(key))
                    fontFamilies[key] = await _fontService.GetFontAsync(fontCollection, fontName, weight);
            }

            // Dark overlay for readability
            image.Mutate(ctx => ctx.Fill(
                new SixLabors.ImageSharp.Drawing.Processing.DrawingOptions(),
                SixLabors.ImageSharp.Drawing.Processing.Brushes.Solid(Color.FromRgba(0, 0, 0, 100)),
                new RectangleF(0, 0, W, H)
            ));

            // Render each text layer, swapping in new content where roles match
            foreach (var layer in layout.TextLayers)
            {
                var text = ResolveContent(layer.Role, layer.Content, newContent);
                var key = $"{layer.GoogleFont}-{layer.Weight}";
                var family = fontFamilies[key];

                var fontSize = (layer.FontSize / 100f) * (H * 0.15f); // scale relative to image height
                var font = family.CreateFont(fontSize, MapStyle(layer.Weight));
                var color = Color.ParseHex(layer.Color.TrimStart('#'));

                var x = layer.XPercent * W;
                var y = layer.YPercent * H;

                var textOptions = new RichTextOptions(font)
                {
                    Origin = new System.Numerics.Vector2(x, y),
                    HorizontalAlignment = layer.Alignment switch
                    {
                        "Center" => HorizontalAlignment.Center,
                        "Right" => HorizontalAlignment.Right,
                        _ => HorizontalAlignment.Left
                    },
                    WrappingLength = W * 0.8f
                };

                image.Mutate(ctx => ctx.DrawText(textOptions, text, color));
            }

            // Render bottom bar if present
            if (layout.BottomBar != null)
            {
                var bar = layout.BottomBar;
                var barHeight = H * 0.12f;
                var barY = H - barHeight;
                var barColor = Color.ParseHex(bar.BackgroundColor.TrimStart('#'));

                image.Mutate(ctx => ctx.Fill(
                    new DrawingOptions(),
                    Brushes.Solid(barColor),
                    new RectangleF(0, barY, W, barHeight)
                ));

                // Draw date | venue | time fields
                var barFont = fontCollection.Families.First().CreateFont(24, FontStyle.Regular);
                var fields = new[] { newContent.Date, newContent.Venue, newContent.Time };
                var fieldCount = fields.Length;
                for (int i = 0; i < fieldCount; i++)
                {
                    var fx = (W / fieldCount) * i + (W / fieldCount / 2f);
                    var opts = new RichTextOptions(barFont)
                    {
                        Origin = new System.Numerics.Vector2(fx, barY + barHeight / 2f),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    image.Mutate(ctx => ctx.DrawText(opts, fields[i] ?? "", Color.ParseHex(bar.TextColor.TrimStart('#'))));
                }
            }

            // Overlay logo if provided
            if (logoPath != null && File.Exists(logoPath))
            {
                using var logo = await Image.LoadAsync<Rgba32>(logoPath);
                logo.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(W / 5, 0),
                    Mode = ResizeMode.Max
                }));

                var lx = (int)(layout.Logo.XPercent * W);
                var ly = (int)(layout.Logo.YPercent * H);
                image.Mutate(ctx => ctx.DrawImage(logo, new Point(lx, ly), 1f));
            }

            var outputPath = Path.Combine("TempGenerated", $"{Guid.NewGuid():N}.png");
            await image.SaveAsPngAsync(outputPath);
            return outputPath;
        }

        private static string ResolveContent(string role, string originalContent, PosterContent newContent)
        {
            return role switch
            {
                "Title" => newContent.Title ?? originalContent,
                "Subtitle" => newContent.Subtitle ?? originalContent,
                "Scripture" => newContent.Scripture ?? originalContent,
                "Detail" or "Label" => originalContent, // keep as-is
                _ => originalContent
            };
        }

        private static FontStyle MapStyle(string weight) => weight switch
        {
            "Bold" => FontStyle.Bold,
            "Light" => FontStyle.Regular,
            "Black" => FontStyle.Bold,
            _ => FontStyle.Regular
        };
    }

    public class PosterContent
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Scripture { get; set; }
        public string? Date { get; set; }
        public string? Venue { get; set; }
        public string? Time { get; set; }
    }
}