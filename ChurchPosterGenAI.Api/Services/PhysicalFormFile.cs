using Microsoft.AspNetCore.Http;

namespace ChurchPosterGenAI.Api.Helpers;

public class PhysicalFormFile : IFormFile,IDisposable
{
    private readonly FileInfo _fileInfo;
    private readonly Stream _stream;

    public PhysicalFormFile(string filePath)
    {
        _fileInfo = new FileInfo(filePath);
        _stream = File.OpenRead(filePath);

        FileName = _fileInfo.Name;
        ContentType = ResolveContentType(_fileInfo.Extension);
        Name = _fileInfo.Name;
        Length = _fileInfo.Length;
        Headers = new HeaderDictionary();
        ContentDisposition = $"form-data; name=\"file\"; filename=\"{FileName}\"";
    }

    public string ContentType { get; }
    public string ContentDisposition { get; }
    public IHeaderDictionary Headers { get; }
    public long Length { get; }
    public string Name { get; }
    public string FileName { get; }

    public Stream OpenReadStream() => _stream;

    public void CopyTo(Stream target) => _stream.CopyTo(target);

    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        => await _stream.CopyToAsync(target, cancellationToken);

    private static string ResolveContentType(string extension) => extension.ToLower() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png"            => "image/png",
        ".gif"            => "image/gif",
        ".webp"           => "image/webp",
        ".pdf"            => "application/pdf",
        _                 => "application/octet-stream"
    };
    public void Dispose()
    {
        _stream.Dispose();
    }
}