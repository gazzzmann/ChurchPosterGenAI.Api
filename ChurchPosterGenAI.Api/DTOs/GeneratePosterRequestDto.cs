using System.ComponentModel.DataAnnotations;

namespace ChurchPosterGenAI.Api.DTOs
{
    public class GeneratePosterRequestDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int PosterTemplateId { get; set; }

        [Required]
        public string Prompt { get; set; } = string.Empty;

    }
}
