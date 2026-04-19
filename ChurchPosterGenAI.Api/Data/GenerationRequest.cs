using ChurchPosterGenAI.Api.Enum;

namespace ChurchPosterGenAI.Api.Data;

public class GenerationRequest
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int PosterTemplateId { get; set; }
    public PosterTemplate Template { get; set; }
    public string Prompt { get; set; }
    public GenerationStatus Status { get; set; } = GenerationStatus.Processing;
    public DateTime CreatedAt { get; set; }
    public ICollection<GeneratedPoster> Results { get; set; } = new List<GeneratedPoster>();
}