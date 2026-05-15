using ChurchPosterGenAI.Api.Controllers;
using ChurchPosterGenAI.Api.Helpers;

namespace ChurchPosterGenAI.Api.Services;

public class FileUploaderService
{
    private readonly MongoService _mongoService;
    private readonly PosterClassifierService _classifierService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly string _directory;

    public FileUploaderService(
        IConfiguration configuration,
        MongoService mongoService,
        PosterClassifierService classifierService,
        IBlobStorageService blobStorageService)
    {
        _mongoService = mongoService;
        _classifierService = classifierService;
        _blobStorageService = blobStorageService;

        _directory = configuration["Folder:Directory"]
            ?? throw new InvalidOperationException(
                "Folder:Directory is not configured.");
    }

    /// <summary>
    /// Scans the configured folder and uploads every file
    /// that has not already been imported.
    /// </summary>
    public async Task AutomaticFileUploaderAsync()
    {
        var files = Directory.EnumerateFiles(_directory);

        foreach (var filePath in files)
        {
            await ProcessFileAsync(filePath);
        }
    }

    /// <summary>
    /// Processes a single file:
    /// 1. Checks if it already exists in MongoDB.
    /// 2. Classifies the poster category using AI.
    /// 3. Uploads the image to Azure Blob Storage.
    /// 4. Saves template metadata to MongoDB.
    /// </summary>
    private async Task ProcessFileAsync(string filePath)
    {
        var alreadyExists = await _mongoService.CheckFiles(filePath);

        if (alreadyExists)
            return;

        var category = await _classifierService.PosterClassifier(filePath);

        using var formFile = new PhysicalFormFile(filePath);

        var blobUrl = await _blobStorageService.UploadImageAsync(
            formFile,
            category);

        await _mongoService.SaveFileAsync(
            filePath,
            blobUrl,
            category);
    }
}