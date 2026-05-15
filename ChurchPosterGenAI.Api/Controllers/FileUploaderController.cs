using ChurchPosterGenAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChurchPosterGenAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileUploaderController : ControllerBase
{
    private readonly FileUploaderService _fileUploaderService;

    public FileUploaderController(FileUploaderService fileUploaderService)
    {
        _fileUploaderService = fileUploaderService;
    }

    [HttpPost("upload-all")]
    public async Task<IActionResult> UploadAllAsync()
    {
        await _fileUploaderService.AutomaticFileUploaderAsync();

        return Ok(new
        {
            message = "All pending files were uploaded successfully."
        });
    }
}