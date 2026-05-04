using ChurchPosterGenAI.Api.Controllers;
using ChurchPosterGenAI.Api.Helpers;

namespace ChurchPosterGenAI.Api.Services;

public class FileUploaderService
{
    private readonly IConfiguration _config;
    private readonly MongoService _mongoService;
    private readonly PosterClassifierService _classifierService;
    private readonly BlobStorageService _blobStorageService;
    private readonly string _directory;

    public FileUploaderService(
        IConfiguration config,
        MongoService mongoService,
        PosterClassifierService classifierService,
        BlobStorageService blobStorageService)
    {
        _config = config;
        _mongoService = mongoService;
        _classifierService = classifierService;
        _blobStorageService = blobStorageService;
        _directory = config["Folder:Directory"] ?? throw new Exception("Directory not found");
    }

    // uploads all new files in the directory that haven't been processed yet
    public async Task AutomaticFileUploaderAsync()
    {
        var files = Directory.EnumerateFiles(_directory);

        foreach (var filePath in files)
        {
            await ProcessFileAsync(filePath);
        }
    }

    // processes a single file — classify, upload to blob, save to mongo
    private async Task ProcessFileAsync(string filePath)
    {
        bool alreadyExists = await _mongoService.CheckFiles(filePath);
        if (alreadyExists) return;

        string category = await _classifierService.PosterClassifier(filePath);

        using var formFile = new PhysicalFormFile(filePath);
        string blobUrl = await _blobStorageService.UploadImageAsync(formFile, category);

        await _mongoService.SaveFileAsync(filePath, blobUrl, category);
    }

    // uploads a single file manually by path
    public async Task UploadSingleFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        await ProcessFileAsync(filePath);
    }

    // returns all files in the directory
    public IEnumerable<string> GetAllFiles()
    {
        return Directory.EnumerateFiles(_directory);
    }

    // returns only files that haven't been uploaded yet
    public async Task<IEnumerable<string>> GetPendingFilesAsync()
    {
        var pending = new List<string>();

        foreach (var filePath in Directory.EnumerateFiles(_directory))
        {
            bool exists = await _mongoService.CheckFiles(filePath);
            if (!exists) pending.Add(filePath);
        }

        return pending;
    }
}