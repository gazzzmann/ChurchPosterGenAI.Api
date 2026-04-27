namespace ChurchPosterGenAI.Api.DTOs;

public class GeneratedPosterResultDto
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}