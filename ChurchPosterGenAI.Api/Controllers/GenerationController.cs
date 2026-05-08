using ChurchPosterGenAI.Api.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPosterGenAI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GenerationController : ControllerBase
{
    private readonly IGenerationService _generationService;

    public GenerationController(IGenerationService generationService)
    {
        _generationService = generationService;
    }

    /// <summary>
    /// Generate a new poster
    /// </summary>
    [HttpPost("/GenerateImage")]
    public async Task<ActionResult<GeneratePosterResponseDto>> Generate(
        [FromBody] GeneratePosterRequestDto dto)
    {   
        if (string.IsNullOrWhiteSpace(dto.UserId))
        {
            Random rand = new Random();
            int randomId = rand.Next();
            dto.UserId = randomId.ToString();
        }
        if (dto == null)
            return BadRequest("Request body is required");

        if (string.IsNullOrWhiteSpace(dto.Prompt))
            return BadRequest("Prompt is required");

        try
        {
            var result = await _generationService.GenerateAsync(dto);

            return Ok(result);
        }
        catch (Exception ex)
        {
            // You can replace this with proper logging later
            return StatusCode(500, new
            {
                message = "An error occurred while generating the poster",
                detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Get generation result by requestId
    /// </summary>
    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<GenerationResultDto>> GetResult(int requestId)
    {
        if (requestId <= 0)
            return BadRequest("Invalid requestId");

        var result = await _generationService.GetResultAsync(requestId);

        if (result == null)
            return NotFound($"No generation request found with ID {requestId}");

        return Ok(result);
    }
}
