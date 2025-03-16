using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class UsersService
    {
        private readonly ApiDefaultContext context;

        public UsersService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public async Task< UserApp?> GetTeacherUser()
        {
            return await context.Users.FirstOrDefaultAsync(x => x.Id  == EnvironmentVariables.TEACHER_ID);
        }
    }
}
