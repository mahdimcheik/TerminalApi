using Microsoft.AspNetCore.Mvc.Razor;
using PuppeteerSharp;
using RazorLight;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;
using TerminalApi.Services.Templates;

namespace TerminalApi.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IRazorLightEngine _razorLightEngine;

        public PdfService(IWebHostEnvironment env)
        {
            _env = env;

            // Initialize RazorLightEngine
            _razorLightEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(_env.WebRootPath, "TemplatesInvoice"))
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task GeneratePdfAsync(Order order)
        {
            // Render the HTML content using RazorLight
            string templatePath = "Invoice.cshtml"; // Name of your template file
            var model = new InvoiceViewModel();
            string htmlContent = await _razorLightEngine.CompileRenderAsync(templatePath, model);

            // Generate PDF using PuppeteerSharp
            // await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            using var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions { Headless = true }
            );
            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent);
            await page.PdfAsync("customContent.pdf");
        }
    }
}
