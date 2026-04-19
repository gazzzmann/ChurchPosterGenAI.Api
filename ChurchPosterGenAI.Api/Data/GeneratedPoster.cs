namespace ChurchPosterGenAI.Api.Data;

public class GeneratedPoster
{
    public int Id { get; set; }
    public int GenerationRequestId { get; set; }
    public GenerationRequest? Request { get; set; }
    public string ImageUrl { get; set; } 
    public DateTime CreatedAt { get; set; }
}