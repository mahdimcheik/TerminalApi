using TerminalApi.Models;

namespace TerminalApi.Services
{
    public interface ICursusService
    {
        Task<(IEnumerable<CursusDto>, int)> GetAllAsync(QueryPagination queryPagination);
        Task<CursusDto> CreateAsync(CreateCursusDto dto);
        Task<CursusDto?> UpdateAsync(Guid id, UpdateCursusDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<Level>> GetLevelsAsync();
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
} 