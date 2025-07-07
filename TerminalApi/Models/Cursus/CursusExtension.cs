namespace TerminalApi.Models
{
    public static class CursusExtension
    {
        public static Cursus ToCursus(this CreateCursusDto dto)
        {
            return new Cursus
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                LevelId = dto.LevelId,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public static CursusDto ToCursusDto(this Cursus cursus)
        {
            return new CursusDto
            {
                Id = cursus.Id,
                Name = cursus.Name,
                Description = cursus.Description,
                LevelId = cursus.LevelId,
                Level = cursus.Level,
                CategoryId = cursus.CategoryId,
                Category = cursus.Category,
                CreatedAt = cursus.CreatedAt,
                UpdatedAt = cursus.UpdatedAt
            };
        }

        public static void UpdateFromDto(this Cursus cursus, UpdateCursusDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Name))
                cursus.Name = dto.Name;
            
            if (!string.IsNullOrEmpty(dto.Description))
                cursus.Description = dto.Description;
            
            if (dto.LevelId.HasValue)
                cursus.LevelId = dto.LevelId.Value;
            
            if (dto.CategoryId.HasValue)
                cursus.CategoryId = dto.CategoryId.Value;
            
            cursus.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
} 