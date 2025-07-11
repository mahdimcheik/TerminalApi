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
                CategoryId = cursus.CategoryId,
                CreatedAt = cursus.CreatedAt,
                UpdatedAt = cursus.UpdatedAt,
                Category = cursus.Category.ToCategoryDTO(),
                Level = cursus.Level.ToLevelDTO()

            };
        }

        public static void UpdateFromDto(this Cursus cursus, UpdateCursusDto dto)
        {
            cursus.Id = dto.Id;
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

        public static LevelDTO ToLevelDTO(this Level level)
        {
            return new LevelDTO
            {
                Id = level.Id,
                Name = level.Name,
                Color = level.Color,
                Icon = level.Icon
            };
        }

        public static CategoryDTO ToCategoryDTO(this Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Color = category.Color,
                Icon = category.Icon
            };
        }
    }
} 