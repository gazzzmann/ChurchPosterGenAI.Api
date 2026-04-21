using ChurchPosterGenAI.Api.Data;

namespace ChurchPosterGenAI.Api.Services
{
    public interface ITemplateService
    {
        Task<IEnumerable<PosterTemplate>> GetAllAsync();
        Task<PosterTemplate?> GetByIdAsync(int id);
    }
}
