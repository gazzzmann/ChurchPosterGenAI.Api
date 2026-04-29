namespace ChurchPosterGenAI.Api.DTOs
{
    public class GeneratePosterResponseDto
    {
        public int RequestId { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ImageUrl { get; set; } 
    }
}
