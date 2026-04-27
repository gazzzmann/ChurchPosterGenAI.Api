using ChurchPosterGenAI.Api.Services;
using ChurchPosterGenAI.Api.DTO_s;
using ChurchPosterGenAI.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPosterGenAI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IBlobStorageService _blobStorageService; // Add Blob Service

    public TemplateController(
        ITemplateService templateService,
        IBlobStorageService blobStorageService) // Inject it here
    {
        _templateService = templateService;
        _blobStorageService = blobStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _templateService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _templateService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Template not found" });

        return Ok(result);
    }

    // Add your new Upload endpoint
    [HttpPost("upload")]
    public async Task<IActionResult> UploadTemplate([FromForm] TemplateUploadRequest request)
    {
        if (request.Image == null || request.Image.Length == 0)
        {
            return BadRequest("No image file was provided.");
        }

        // 1. Upload image to Azure and get the URL
        string categoryString = request.Category.ToString();
        string azureImageUrl = await _blobStorageService.UploadImageAsync(request.Image, categoryString);

        // 2. Prepare the entity
        var newTemplate = new PosterTemplate
        {
            Title = request.Title,
            Category = request.Category,
            ImageUrl = azureImageUrl // Store the Azure URL
        };

        // 3. Save via your Service layer
        var createdTemplate = await _templateService.AddTemplateAsync(newTemplate);

        // 4. Return the new DTO
        return CreatedAtAction(nameof(GetById), new { id = createdTemplate.Id }, createdTemplate);
    }
}