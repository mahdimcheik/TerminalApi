using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;

namespace TerminalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly ApiDefaultContext context;

        public ActivitiesController(ApiDefaultContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Activities>> > GetTeacherTraffic()
        {
         
            var firstDayOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);

            var bookinsOftheWeek = await context.Bookings
                .Include(b => b.Slot)
                .Include(b => b.Booker)
                .Where(b => b.Slot.StartAt >= firstDayOfWeek && b.Slot.EndAt < firstDayOfWeek.AddDays(7))
                .Select(b => b.ToBookingResponseDTO())
                .ToListAsync();

            var OrdersOfTheWeek = await context.Orders
                .Where(o => o.UpdatedAt >= firstDayOfWeek && o.PaymentIntent != null)
                .Select(o => o.ToOrderResponseForTeacherDTO())
                .ToListAsync();

            var newStudents = await context.Users
                .Where(u => u.CreatedAt >= firstDayOfWeek)
                .Select(u => u.ToUserResponseDTO(null))
                .ToListAsync();

            var trafficData = new Activities
            {
                NewStudents = newStudents,
                BookingsOftheWeek = bookinsOftheWeek,
                OrdersOfTheWeek = OrdersOfTheWeek
            };

            return Ok(new ResponseDTO<Activities>
            {
                Message = "Activités envoyées qvec succés",
                Data = trafficData,
                Status = 200
            });

        }
    }
}
