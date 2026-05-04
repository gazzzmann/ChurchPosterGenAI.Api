using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChurchPosterGenAI.Api.Data;

public class PosterTemplateDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Title { get; set; }
    public required string Category { get; set; } // plain string, no enum needed
    public required string ImageUrl { get; set; }
}