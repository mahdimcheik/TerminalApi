using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TerminalApi.Contexts;
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

        public async Task<List<UserResponseDTO>> GetAllStudentsDTO()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            return students.Select(x => x.ToUserResponseDTO()).ToList();
        }
    }
}
