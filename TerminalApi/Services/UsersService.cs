using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class UsersService
    {
        private readonly ApiDefaultContext context;
        private readonly UserManager<UserApp> _userManager;

        public UsersService(ApiDefaultContext context, UserManager<UserApp> userManager)
        {
            this.context = context;
            _userManager = userManager;
        }

        public async Task< UserApp?> GetTeacherUser()
        {
            return await context.Users.FirstOrDefaultAsync(x => x.Id  == EnvironmentVariables.TEACHER_ID);
        }

        public async Task<ResponseDTO> GetAllStudentsDTO(QueryPagination query)
        {
            var querySql = context.Users.Where(x => x.EmailConfirmed && x.Id != HardCode.TeacherId);

            var count = await querySql.CountAsync();

            if(query is null)
            {
                return new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Count = count,
                    Data = querySql.Skip( 0).Take( 10).ToList()
                };
            }

            querySql = querySql.Skip(query?.Start ?? 0).Take(query?.PerPage ?? 10);

            var result = await  querySql.ToListAsync();
            return new ResponseDTO
            {
                Message = "Demande acceptée",
                Count = count,
                Data = result
            };
        }
    }
}
