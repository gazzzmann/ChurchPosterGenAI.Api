using ChurchPosterGenAI.Api.Enum;
using Microsoft.AspNetCore.Http;

namespace ChurchPosterGenAI.Api.DTOs
{
    public class TemplateUploadRequest
    {
        public string Title { get; set; }
        public PosterCategory Category { get; set; }
        public IFormFile Image { get; set; }
    }
}