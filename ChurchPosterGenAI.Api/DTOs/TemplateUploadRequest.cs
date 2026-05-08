using ChurchPosterGenAI.Api.Enum;
using Microsoft.AspNetCore.Http;

namespace ChurchPosterGenAI.Api.DTOs
{
    public class TemplateUploadRequest
    {
        public required string Title { get; set; }
        public PosterCategory Category { get; set; }
        public required IFormFile Image { get; set; }
    }
}