using ChurchPosterGenAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPosterGenAI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _templateService.GetAllAsync();
        return Ok(result); // Returns a list of TemplateResponseDto
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _templateService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Template not found" });

        return Ok(result); // Returns a single TemplateResponseDto
    }
}