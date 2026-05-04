using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

[ApiController]
[Route("api/[controller]")]
public class ImageGeneratorController : ControllerBase
{
    private readonly IConfiguration _config;
    public ImageGeneratorController(IConfiguration config) => _config = config;

    [HttpPost]
    public async Task<IActionResult> EditImage(IFormFile image, [FromForm] string prompt)
    {
        var hfToken = _config["HuggingFace:ApiToken"];
        if (string.IsNullOrEmpty(hfToken))
            return StatusCode(500, "HuggingFace:ApiToken is not configured.");

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", hfToken);

        // Step 1: Resize image before sending to stay under size limits
        byte[] imageBytes;
        using (var ms = new MemoryStream())
        {
            await image.CopyToAsync(ms);
            imageBytes = ms.ToArray();
        }

        // Step 2: HuggingFace instruct-pix2pix expects JSON with base64
        // BUT we need to compress/resize first to avoid 413
        // Use SixLabors.ImageSharp to resize
        using var inputStream = new MemoryStream(imageBytes);
        using var outputStream = new MemoryStream();
        
        using (var img = await Image.LoadAsync(inputStream))
        {
            // Resize to max 512x512 to keep payload small
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(512, 512),
                Mode = ResizeMode.Max
            }));
            await img.SaveAsJpegAsync(outputStream, 
                new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 75 });
        }

        var resizedBytes = outputStream.ToArray();
        var base64 = Convert.ToBase64String(resizedBytes);
        var dataUri = $"data:image/jpeg;base64,{base64}";

        // Step 3: Send to HuggingFace - returns image bytes directly, no polling!
        var payload = new { inputs = new { prompt = prompt, image = dataUri } };
        
        var requestContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(
            "https://api-inference.huggingface.co/models/timbrooks/instruct-pix2pix/predict",
            requestContent
        );

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, $"HuggingFace error: {err}");
        }

        // HuggingFace returns image bytes directly - convert to base64 for frontend
        var resultBytes = await response.Content.ReadAsByteArrayAsync();
        var resultBase64 = Convert.ToBase64String(resultBytes);
        var resultDataUri = $"data:image/jpeg;base64,{resultBase64}";

        return Ok(new { resultUrl = resultDataUri });
    }
}