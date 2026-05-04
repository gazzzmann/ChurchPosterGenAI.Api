namespace ChurchPosterGenAI.Api.Services;

using ChurchPosterGenAI.Api.Data;
using MongoDB.Driver;

public class MongoService
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<PosterTemplateDocument> _collection;

    public MongoService(IConfiguration configuration)
    {
        string connectionString = configuration["Mongo:ConnectionString"] ?? throw new Exception("Connection string not found");
        _mongoClient = new MongoClient(connectionString);

        var database = _mongoClient.GetDatabase("ChurchPoster");
        _collection = database.GetCollection<PosterTemplateDocument>("PosterTemplates");
    }

    public async Task<bool> CheckFiles(string blobPath)
    {
        var filter = Builders<PosterTemplateDocument>.Filter.Eq(p => p.ImageUrl, blobPath);
        return await _collection.Find(filter).AnyAsync();
    }

    public async Task SaveFileAsync(string filePath, string blobUrl, string category)
    {
        var poster = new PosterTemplateDocument
        {
            Title = Path.GetFileNameWithoutExtension(filePath),
            ImageUrl = blobUrl,
            Category = category,
        };

        await _collection.InsertOneAsync(poster);
    }

    public async Task<List<PosterTemplateDocument>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<List<PosterTemplateDocument>> GetByCategoryAsync(string category)
    {
        var filter = Builders<PosterTemplateDocument>.Filter.Eq(p => p.Category, category);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<PosterTemplateDocument?> GetByIdAsync(string id)
    {
        var filter = Builders<PosterTemplateDocument>.Filter.Eq(p => p.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<PosterTemplateDocument>.Filter.Eq(p => p.Id, id);
        await _collection.DeleteOneAsync(filter);
    }
}