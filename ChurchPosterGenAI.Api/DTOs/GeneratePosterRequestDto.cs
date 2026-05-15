using System.ComponentModel.DataAnnotations;

namespace ChurchPosterGenAI.Api.DTOs;

public class GeneratePosterRequestDto
{
    public string? UserId { get; set; }

    public int? PosterTemplateId { get; set; }

    public string? ImageUrl { get; set; }

    [Required]
    public string Prompt { get; set; } = string.Empty;
}