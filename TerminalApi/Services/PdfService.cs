using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using RazorLight;
using TerminalApi.Contexts;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;
using TerminalApi.Services.Templates;

namespace TerminalApi.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApiDefaultContext context;
        private readonly IRazorLightEngine _razorLightEngine;

        public PdfService(IWebHostEnvironment env, ApiDefaultContext context)
        {
            _env = env;
            this.context = context;

            // Initialize RazorLightEngine
            _razorLightEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(_env.WebRootPath, "TemplatesInvoice"))
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<byte[]> GeneratePdfAsync(OrderResponseForStudentDTO order)
        {
            //var res = Guid.TryParse(orderId, out Guid guidId);

            string templatePath = "Invoice.cshtml"; // Name of your template file


            string htmlContent = await _razorLightEngine.CompileRenderAsync(templatePath, order);

            using var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions { Headless = true }
            );
            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent);
            var file = await page.PdfDataAsync( new PdfOptions
            {
                Format = PaperFormat.A4,
                DisplayHeaderFooter = true,
                FooterTemplate = @"<div style='width:100%;text-align:center;font-size:10px;padding:5px;'>
                Page <span class='pageNumber'></span> / <span class='totalPages'></span>
            </div>",
                MarginOptions = new PuppeteerSharp.Media.MarginOptions { Top = "40px", Bottom = "60px" }
            });
            return file;
        }
    }
}
