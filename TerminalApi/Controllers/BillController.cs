using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Payments;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BillController : ControllerBase
    {
        private readonly PdfService pdfService;
        private readonly ApiDefaultContext context;

        public BillController(PdfService pdfService, ApiDefaultContext context)
        {
            this.pdfService = pdfService;
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string orderId)
        {
            Console.WriteLine(orderId);
            if (orderId.Trim().IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO { Message = "Aucune commande correspondante", Status = 404, }
                );
            }

            var order = context
                .Orders.Where(x => x.Id == Guid.Parse(orderId))
                .Include(x => x.Booker)
                .Include(x => x.Bookings)
                .ThenInclude(x => x.Slot)
                .FirstOrDefault();
            if (order is null)
            {
                return BadRequest(
                    new ResponseDTO { Message = "Aucune commande correspondante", Status = 404, }
                );
            }
            var file = await pdfService.GeneratePdfAsync(order.ToOrderResponseForStudentDTO());
            return File(file, "application/pdf", "facture.pdf");
        }
        //[HttpGet("testing")]
        //public async Task<IActionResult> GetTest()
        //{
        //    //var file = await pdfService.GeneratePdfAsync("");
        //    //return File(file, "application/pdf", "facture.pdf");
        //}
    }
}
