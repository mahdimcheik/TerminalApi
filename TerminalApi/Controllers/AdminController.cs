using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TerminalApi.Models.User;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UsersService userService;

        public AdminController(UsersService userService)
        {
            this.userService = userService;
        }

        [HttpGet("all-students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await userService.GetAllStudentsDTO();
            return Ok(students);
        }
    }
}
