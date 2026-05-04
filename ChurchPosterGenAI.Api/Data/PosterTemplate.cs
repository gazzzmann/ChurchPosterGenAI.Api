using ChurchPosterGenAI.Api.Enum;

namespace ChurchPosterGenAI.Api.Data;

public class PosterTemplate
{
    public int Id { get; set; }
    public required string Title { get; set; } // e.g. "Sunday Service Flyer"
    public PosterCategory Category { get; set; }  
    public required string ImageUrl { get; set; }
}
