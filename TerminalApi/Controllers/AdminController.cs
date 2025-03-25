using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TerminalApi.Models;
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

        [HttpPost("all-students")]
        public async Task<IActionResult> GetAllStudents([FromBody] QueryPagination query)
        {
            var students = await userService.GetAllStudentsDTO( query);
            return Ok(students);
        }
    }
}
