using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.DTOs;
using ChurchPosterGenAI.Api.Enum;
using Microsoft.EntityFrameworkCore;

namespace ChurchPosterGenAI.Api.Services
{
    public class GenerationService : IGenerationService
    {
        private readonly ChurchPosterDbContext _context;
        private readonly IAIImageService _aiService;

        public GenerationService(
            ChurchPosterDbContext context,
            IAIImageService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<GeneratePosterResponseDto> GenerateAsync(
            GeneratePosterRequestDto dto)
        {
            var template = await GetPosterTemplateAsync(dto.PosterTemplateId);

            var structuredPrompt = BuildStructuredPrompt(template, dto.Prompt);

            var request = await CreateGenerationRequestAsync(dto, structuredPrompt);

            try
            {
                var imageUrl = await _aiService.GenerateFromImageAsync(
                    template.ImageUrl,
                    structuredPrompt);

                await SaveGeneratedPosterAsync(request, imageUrl);

                return new GeneratePosterResponseDto
                {
                    RequestId = request.Id,
                    Status = request.Status.ToString(),
                    ImageUrl = imageUrl
                };
            }
            catch
            {
                request.Status = GenerationStatus.Failed;
                await _context.SaveChangesAsync();
                throw;
            }
        }

        public async Task<GenerationResultDto?> GetResultAsync(int requestId)
        {
            var request = await _context.Requests
                .Include(x => x.Results)
                .FirstOrDefaultAsync(x => x.Id == requestId);

            if (request == null)
                return null;

            return new GenerationResultDto
            {
                RequestId = request.Id,
                Status = request.Status.ToString(),
                Results = request.Results
                    .Select(result => new GeneratedPosterResultDto
                    {
                        Id = result.Id,
                        ImageUrl = result.ImageUrl,
                        CreatedAt = result.CreatedAt
                    })
                    .ToList()
            };
        }

        private async Task<PosterTemplate> GetPosterTemplateAsync(int templateId)
        {
            var template = await _context.Set<PosterTemplate>()
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template != null)
                return template;

            return new PosterTemplate
            {
                Id = templateId,
                Title = "Sunday Service",
                Category = PosterCategory.Conference,
                ImageUrl = "https://example.com/default-church-template.jpg"
            };
        }

        private string BuildStructuredPrompt(
            PosterTemplate template,
            string userPrompt)
        {
            return $@"
            Modify this church flyer template:
            
            Title: {template.Title}
            Category: {template.Category}
            
            User Input:
            {userPrompt}
            
            Enhance with:
            - modern typography
            - vibrant colors
            - spiritual atmosphere
            - clean layout";
        }

        private async Task<GenerationRequest> CreateGenerationRequestAsync(
            GeneratePosterRequestDto dto,
            string structuredPrompt)
        {
            var request = new GenerationRequest
            {
                UserId = dto.UserId,
                PosterTemplateId = dto.PosterTemplateId,
                Prompt = structuredPrompt,
                Status = GenerationStatus.Processing,
                CreatedAt = DateTime.UtcNow
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return request;
        }

        private async Task SaveGeneratedPosterAsync(
            GenerationRequest request,
            string imageUrl)
        {
            var generatedPoster = new GeneratedPoster
            {
                GenerationRequestId = request.Id,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posters.Add(generatedPoster);

            request.Status = GenerationStatus.Completed;

            await _context.SaveChangesAsync();
        }
    }
}