using ChurchPosterGenAI.Api.Enum;

namespace ChurchPosterGenAI.Api.Data;

public class MongoPosterTemplate
{
    public int Id { get; set; }// e.g. "Sunday Service Flyer"
    public PosterCategory Category { get; set; }  
    public required string ImageUrl { get; set; }
}
