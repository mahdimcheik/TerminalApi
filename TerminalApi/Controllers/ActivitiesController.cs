using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;

namespace TerminalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly ApiDefaultContext context;

        public ActivitiesController(ApiDefaultContext context)
        {
            this.context = context;
        }

        [HttpGet("teacher")]
        public async Task<ActionResult<ResponseDTO<ActivitiesTeacher>>> GetTeacherTraffic()
        {
         
            var firstDayOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);

            var bookinsOftheWeek = await context.Bookings
                .Include(b => b.Slot)
                .Include(b => b.Booker)
                .Where(b => b.Slot.StartAt >= firstDayOfWeek && b.Slot.EndAt < firstDayOfWeek.AddDays(7))
                .OrderBy(b => b.Slot.StartAt)
                .Select(b => b.ToBookingResponseDTO())
                .ToListAsync();

            var OrdersOfTheWeek = await context.Orders
                .Where(o => o.UpdatedAt >= firstDayOfWeek && o.PaymentIntent != null)
                .OrderBy(o => o.UpdatedAt)
                .Select(o => o.ToOrderResponseForTeacherDTO())
                .ToListAsync();

            var newStudents = await context.Users
                .Where(u => u.CreatedAt >= firstDayOfWeek)
                .Select(u => u.ToUserResponseDTO(null))
                .ToListAsync();

            var trafficData = new ActivitiesTeacher
            {
                NewStudents = newStudents,
                BookingsOftheWeek = bookinsOftheWeek,
                OrdersOfTheWeek = OrdersOfTheWeek
            };

            return Ok(new ResponseDTO<ActivitiesTeacher>
            {
                Message = "Activités envoyées qvec succés",
                Data = trafficData,
                Status = 200
            });

        }


        [HttpGet("student/{id}")]
        public async Task<ActionResult<ResponseDTO<ActivitiesStudent>>> GetStudentTraffic([FromRoute] string id)
        {

            var firstDayOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);

            var bookinsOftheWeek = await context.Bookings
                .Include(b => b.Slot)
                .Include(b => b.Booker)
                .Where(b => b.Slot.StartAt >= firstDayOfWeek && b.Slot.EndAt < firstDayOfWeek.AddDays(7) && b.BookedById == id)
                .OrderBy(b => b.Slot.StartAt)
                .Select(b => b.ToBookingResponseDTO())
                .ToListAsync();

            var OrdersHistory = await context.Orders
                .Where(o => o.UpdatedAt >= firstDayOfWeek && o.PaymentIntent != null && o.BookerId == id)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(o => o.ToOrderResponseForTeacherDTO())
                .ToListAsync();

            var trafficData = new ActivitiesStudent
            {
                BookingsOftheWeek = bookinsOftheWeek,
                OrdersHistory = OrdersHistory
            };

            return Ok(new ResponseDTO<ActivitiesStudent>
            {
                Message = "Activités envoyées qvec succés",
                Data = trafficData,
                Status = 200
            });

        }
    }
}
