using ChurchPosterGenAI.Api.DTOs;

public interface IGenerationService
{
    Task<GeneratePosterResponseDto> GenerateAsync(GeneratePosterRequestDto dto);
    Task<GenerationResultDto?> GetResultAsync(int requestId);
}