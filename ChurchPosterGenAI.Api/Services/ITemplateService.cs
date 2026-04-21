using ChurchPosterGenAI.Api.Data;
using ChurchPosterGenAI.Api.DTO_s;

namespace ChurchPosterGenAI.Api.Services
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateResponseDto>> GetAllAsync();
        Task<TemplateResponseDto?> GetByIdAsync(int id);
    }
}
