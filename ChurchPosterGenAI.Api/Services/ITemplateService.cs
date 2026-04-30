using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.DTOs;

namespace ChurchPosterGenAI.Api.Services
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateResponseDto>> GetAllAsync();
        Task<TemplateResponseDto?> GetByIdAsync(int id);
        Task<TemplateResponseDto> AddTemplateAsync(PosterTemplate template);
    }
}
