using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.DTOs;
using ChurchPosterGenAI.Api.Enum;
using Microsoft.EntityFrameworkCore;

namespace ChurchPosterGenAI.Api.Services;

public class GenerationService : IGenerationService
{
    private readonly ChurchPosterDbContext _context;
    private readonly IAIImageService _aiImageService;

    public GenerationService(
        ChurchPosterDbContext context,
        IAIImageService aiImageService)
    {
        _context = context;
        _aiImageService = aiImageService;
    }

    public async Task<GeneratePosterResponseDto> GenerateAsync(
        GeneratePosterRequestDto dto)
    {
        // Generate a UserId if one was not provided
        dto.UserId ??= Guid.NewGuid().ToString("N");

        // Determine which template image to use:
        // - If ImageUrl is provided directly, use it.
        // - Otherwise, load the template from SQL Server using PosterTemplateId.
        var templateImageUrl = await ResolveTemplateImageUrlAsync(dto);

        // Save the generation request
        var request = new GenerationRequest
        {
            UserId = dto.UserId,
            PosterTemplateId = dto.PosterTemplateId ?? 1,
            Prompt = dto.Prompt,
            Status = GenerationStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        _context.Requests.Add(request);
        await _context.SaveChangesAsync();

        try
        {
            // Core AI workflow:
            // 1. Describe the template image.
            // 2. Combine description with the user's prompt.
            // 3. Generate a new poster.
            var generatedImageUrl = await _aiImageService.GenerateFromImageAsync(
                templateImageUrl,
                dto.Prompt);

            // Save generated poster record
            var generatedPoster = new GeneratedPoster
            {
                GenerationRequestId = request.Id,
                ImageUrl = generatedImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posters.Add(generatedPoster);

            request.Status = GenerationStatus.Completed;

            await _context.SaveChangesAsync();

            return new GeneratePosterResponseDto
            {
                RequestId = request.Id,
                Status = request.Status.ToString(),
                ImageUrl = generatedImageUrl
            };
        }
        catch
        {
            request.Status = GenerationStatus.Failed;
            await _context.SaveChangesAsync();
            throw;
        }
    }

    public async Task<GenerationResultDto?> GetResultAsync(int requestId)
    {
        var request = await _context.Requests
            .Include(r => r.Results)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return null;

        return new GenerationResultDto
        {
            RequestId = request.Id,
            Status = request.Status.ToString(),
            Results = request.Results
                .Select(result => new GeneratedPosterResultDto
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    CreatedAt = result.CreatedAt
                })
                .ToList()
        };
    }

    private async Task<string> ResolveTemplateImageUrlAsync(
        GeneratePosterRequestDto dto)
    {
        // If a direct image URL is provided, use it
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            return dto.ImageUrl;

        // Otherwise, a template ID must be supplied
        if (!dto.PosterTemplateId.HasValue)
            throw new ArgumentException(
                "Either PosterTemplateId or ImageUrl must be provided.");

        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == dto.PosterTemplateId.Value);

        if (template == null)
            throw new KeyNotFoundException("Template not found.");

        return template.ImageUrl;
    }
}