using ChurchPosterGenAI.Api.DTOs;
using ChurchPosterGenAI.Api.Services;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class ImageGeneratorController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IAIImageService _imageService;
    private readonly IHttpClientFactory _httpClientFactory;
    public ImageGeneratorController(IConfiguration config,IAIImageService imageService,IHttpClientFactory httpClientFactory) { 
        _config = config;
        _imageService= imageService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> EditImage([FromForm] string prompt,[FromForm] string imageUrl)
    {
        
        // return Ok(result);
        if (string.IsNullOrWhiteSpace(imageUrl))
        return BadRequest("imageUrl is required.");
    
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            return BadRequest("imageUrl must be a valid absolute URI (e.g., https://...)");
        var descriptionService = new ImageDescriptionService(_config);
        var imageDescription = await descriptionService.DescribeImageAsync(imageUrl);
        Console.Write(imageDescription);
        var enrichedPrompt = $"{imageDescription}. {prompt}";

    // STEP 3: Generate with FLUX
        var hfToken = _config["HuggingFace:Token"];
        if (string.IsNullOrEmpty(hfToken))
            return StatusCode(500, "HuggingFace:Token is not configured.");

        var result = await _imageService.GenerateFromImageAsync(
            imageUrl: imageUrl,
            prompt: prompt
        );
        
        return Ok(result);
    }
    [HttpPost("generate-poster")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> GeneratePoster(
        GeneratePosterRequestModel generatePosterRequestModel
        )
    {
        // Save uploads temporarily
        var tempPath = Path.Combine("TempGenerated", $"{Guid.NewGuid():N}.png");
        await using (var fs = System.IO.File.Create(tempPath))
            await generatePosterRequestModel.TemplateImage.CopyToAsync(fs);

        string? logoPath = null;
        if (generatePosterRequestModel.Logo != null)
        {
            logoPath = Path.Combine("TempGenerated", $"logo_{Guid.NewGuid():N}.png");
            await using var fs = System.IO.File.Create(logoPath);
            await generatePosterRequestModel.Logo.CopyToAsync(fs);
        }

        // 1. Analyze layout
        var analyzer = new PosterLayoutAnalyzerService(_config);
        var layout = await analyzer.AnalyzeLayoutAsync(tempPath);

        // 2. Generate background with FLUX (no text)
        var fluxPrompt = $"Church poster background only, no text, no words. {layout.BackgroundStyle} style. Colors: {layout.PrimaryColor}, {layout.SecondaryColor}. Dramatic lighting, high quality.";
        var backgroundPath = await _imageService.GenerateFromImageAsync(prompt:fluxPrompt,imageUrl:generatePosterRequestModel.ImageUrl);

        // 3. Render final poster
        var renderer = new PosterRenderService(new GoogleFontService(_httpClientFactory, _config));
        var content = new PosterContent { Title = generatePosterRequestModel.Title, Subtitle = generatePosterRequestModel.Subtitle, Scripture = generatePosterRequestModel.Scripture, Date = generatePosterRequestModel.Date, Venue = generatePosterRequestModel.Venue, Time = generatePosterRequestModel.Time };
        var finalPath = await renderer.RenderAsync(backgroundPath, layout, content, logoPath);

        return PhysicalFile(Path.GetFullPath(finalPath), "image/png", "poster.png");
    }
}