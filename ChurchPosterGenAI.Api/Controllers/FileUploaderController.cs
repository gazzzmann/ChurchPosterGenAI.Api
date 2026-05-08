using ChurchPosterGenAI.Api.Controllers;
using ChurchPosterGenAI.Api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPosterGenAI.Api.Services;

[Route("api/[controller]")]
[ApiController]
public class FileUploaderController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly MongoService _mongoService;
    private readonly PosterClassifierService _classifierService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly string _directory;

    public FileUploaderController(
        IConfiguration config,
        MongoService mongoService,
        PosterClassifierService classifierService,
        IBlobStorageService blobStorageService)
    {
        _config = config;
        _mongoService = mongoService;
        _classifierService = classifierService;
        _blobStorageService = blobStorageService;
        _directory = config["Folder:Directory"] ?? throw new Exception("Directory not found");
    }

    // uploads all new files in the directory that haven't been processed yet
    [HttpPost("/FileUploader")]
    public async Task<IActionResult> AutomaticFileUploaderAsync()
    {
        var files = Directory.EnumerateFiles(_directory);

        foreach (var filePath in files)
        {
            await ProcessFileAsync(filePath);
        }
        return Ok("Task Completed");
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
    private async Task UploadSingleFileAsync(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        await ProcessFileAsync(filePath);
    }

    // returns all files in the directory
    private IEnumerable<string> GetAllFiles()
    {
        return Directory.EnumerateFiles(_directory);
    }

    // returns only files that haven't been uploaded yet
    private async Task<IEnumerable<string>> GetPendingFilesAsync()
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