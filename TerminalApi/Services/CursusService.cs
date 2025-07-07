using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;

namespace TerminalApi.Services
{
    public class CursusService : ICursusService
    {
        private readonly ApiDefaultContext _context;

        public CursusService(ApiDefaultContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CursusDto>> GetAllAsync()
        {
            var query = _context.Cursus
                .Include(c => c.Level)
                .Include(c => c.Category)
                .AsQueryable();
           
            var cursus = await query.ToListAsync();
            return cursus.Select(c => c.ToCursusDto());
        }



        public async Task<CursusDto> CreateAsync(CreateCursusDto dto)
        {
            // Validate that Level and Category exist
            var levelExists = await _context.Levels.AnyAsync(l => l.Id == dto.LevelId);
            if (!levelExists)
            {
                throw new ArgumentException("Level not found", nameof(dto.LevelId));
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                throw new ArgumentException("Category not found", nameof(dto.CategoryId));
            }

            var cursus = dto.ToCursus();
            _context.Cursus.Add(cursus);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(cursus)
                .Reference(c => c.Level)
                .LoadAsync();
            await _context.Entry(cursus)
                .Reference(c => c.Category)
                .LoadAsync();

            return cursus.ToCursusDto();
        }

        public async Task<CursusDto?> UpdateAsync(Guid id, UpdateCursusDto dto)
        {
            var cursus = await _context.Cursus
                .Include(c => c.Level)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cursus == null)
                return null;

            // Validate Level exists if provided
            if (dto.LevelId.HasValue)
            {
                var levelExists = await _context.Levels.AnyAsync(l => l.Id == dto.LevelId.Value);
                if (!levelExists)
                {
                    throw new ArgumentException("Level not found", nameof(dto.LevelId));
                }
            }

            // Validate Category exists if provided
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                if (!categoryExists)
                {
                    throw new ArgumentException("Category not found", nameof(dto.CategoryId));
                }
            }

            cursus.UpdateFromDto(dto);
            await _context.SaveChangesAsync();

            // Reload navigation properties if they were changed
            if (dto.LevelId.HasValue || dto.CategoryId.HasValue)
            {
                await _context.Entry(cursus)
                    .Reference(c => c.Level)
                    .LoadAsync();
                await _context.Entry(cursus)
                    .Reference(c => c.Category)
                    .LoadAsync();
            }

            return cursus.ToCursusDto();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var cursus = await _context.Cursus.FindAsync(id);
            if (cursus == null)
                return false;

            _context.Cursus.Remove(cursus);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Cursus.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Level>> GetLevelsAsync()
        {
            return await _context.Levels.ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
    }
} 