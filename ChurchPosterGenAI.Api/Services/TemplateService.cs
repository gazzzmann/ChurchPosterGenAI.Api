using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.DTO_s;
using Microsoft.EntityFrameworkCore;

namespace ChurchPosterGenAI.Api.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ChurchPosterDbContext _context;

        public TemplateService(ChurchPosterDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TemplateResponseDto>> GetAllAsync()
        {
            return await _context.Templates
             .Select(t => new TemplateResponseDto
             {
                 Id = t.Id,
                 Title = t.Title,
                 CategoryName = t.Category.ToString(), // Converts enum to string
                 ImageUrl = t.ImageUrl
             })
             .ToListAsync();
        }

        public async Task<TemplateResponseDto?> GetByIdAsync(int id)
        {
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == id);
            if (template == null) return null;

            return new TemplateResponseDto
            {
                Id = template.Id,
                Title = template.Title,
                CategoryName = template.Category.ToString(),
                ImageUrl = template.ImageUrl
            };
        }
    }
}
