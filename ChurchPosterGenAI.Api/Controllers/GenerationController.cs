using ChurchPosterGenAI.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPosterGenAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenerationController : ControllerBase
{
    private readonly IGenerationService _generationService;

    public GenerationController(IGenerationService generationService)
    {
        _generationService = generationService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GeneratePosterResponseDto>> Generate(
        [FromBody] GeneratePosterRequestDto dto)
    {
        var result = await _generationService.GenerateAsync(dto);
        return Ok(result);
    }

    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<GenerationResultDto>> GetResult(int requestId)
    {
        var result = await _generationService.GetResultAsync(requestId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}