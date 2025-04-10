using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BillController : ControllerBase
    {
        private readonly PdfService pdfService;

        public BillController(PdfService pdfService)
        {
            this.pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string orderId)
        {
            Console.WriteLine(orderId);
            await pdfService.GeneratePdfAsync(orderId);
            return Ok();
        }
        [HttpGet("testing")]
        public async Task<IActionResult> GetTest()
        {
            var file = await pdfService.GeneratePdfAsync("");
            return File(file, "application/pdf", "facture.pdf");
        }
    }
}
