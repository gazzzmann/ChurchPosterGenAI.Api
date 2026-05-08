namespace ChurchPosterGenAI.Api.DTOs;

public class GeneratePosterRequestModel
{
    public IFormFile TemplateImage { get; set; } = null!;
    public IFormFile? Logo { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Scripture { get; set; }
    public string? Date { get; set; }
    public string? Venue { get; set; }
    public string? Time { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}