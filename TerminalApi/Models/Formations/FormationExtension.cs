namespace TerminalApi.Models
{
    public static class FormationExtension
    {
        public static Formation ToFormation(this FormationCreateDTO formationDTO, string userId)
        {
            return new Formation {
            Title = formationDTO.Title,
            StartAt = formationDTO.StartAt,
            EndAt = formationDTO.EndAt,
            Company= formationDTO.Company,
            City= formationDTO.City,
            Country= formationDTO.Country,
            UserId = userId
            };
        }
        public static FormationResponseDTO ToFormationDTO(this Formation formation)
        {
            return new FormationResponseDTO
            {
                Id = formation.Id,
                Title = formation.Title,
                StartAt = formation.StartAt,
                EndAt = formation.EndAt,
                Company = formation.Company,
                City= formation.City,
                Country= formation.Country,
                UserId = formation.UserId
            };
        }

        public static Formation ToFormation(this FormationUpdateDTO formationDTO, Formation formation)
        {
            
            formation.Title= formationDTO.Title;
            formation.StartAt= formationDTO.StartAt;
            formation.EndAt= formationDTO.EndAt;
            formation.City= formationDTO.City;
            formation.Country= formationDTO.Country;
            return formation;
        }
    }
}
