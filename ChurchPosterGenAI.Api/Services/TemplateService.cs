using ChurchPosterGenAI.Api.Data;
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
        public async Task<IEnumerable<PosterTemplate>> GetAllAsync()
        {
            return await _context.Templates.ToListAsync();
        }

        public async Task<PosterTemplate?> GetByIdAsync(int id)
        {
            return _context.Templates.FirstOrDefault(t => t.Id == id);
        }
    }
}
