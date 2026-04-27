namespace ChurchPosterGenAI.Api.DTOs;

public class GenerationResultDto
{
    public int RequestId { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<GeneratedPosterResultDto> Results { get; set; } = new();
}
