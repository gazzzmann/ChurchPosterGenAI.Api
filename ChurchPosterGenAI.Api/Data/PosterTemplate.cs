using ChurchPosterGenAI.Api.Enum;

namespace ChurchPosterGenAI.Api.Data;

public class PosterTemplate
{
    public int Id { get; set; }
    public string Title { get; set; } // e.g. "Sunday Service Flyer"
    public PosterCategory Category { get; set; }  
    public string ImageUrl { get; set; }
}
