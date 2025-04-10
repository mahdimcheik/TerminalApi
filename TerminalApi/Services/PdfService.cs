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

        public async Task<byte[]> GeneratePdfAsync(string orderId)
        {
            var res = Guid.TryParse(orderId, out Guid guidId);
           
            //var orderDetails = await context.Orders
            //    .Where(x => x.Id == guid)
            //    .Select(x => new OrderDetailsDto
            //    {
            //        BookerImgUrl = x.Booker.ImgUrl ?? "",
            //        Bookings = x.Bookings.Select(b => new BookingDetailsDto
            //        {
            //            BookingCreatedAt = b.CreatedAt,
            //            SlotStartAt = b.Slot.StartAt,
            //            SlotEndAt = b.Slot.EndAt
            //        }).ToList()
            //    })
            //    .FirstOrDefaultAsync();
            //if (orderDetails is null) return;
            // Render the HTML content using RazorLight
            string templatePath = "Invoice.cshtml"; // Name of your template file

            var items = new List<InvoiceItem>
            {
                new InvoiceItem { Description = "Item 1", Quantity = 2, UnitPrice = 10 },
                new InvoiceItem { Description = "Item 2", Quantity = 1, UnitPrice = 20 },
                new InvoiceItem { Description = "Item 3", Quantity = 3, UnitPrice = 30 },
                new InvoiceItem { Description = "Item 4", Quantity = 4, UnitPrice = 40 },
                new InvoiceItem { Description = "Item 5", Quantity = 5, UnitPrice = 50 },
                new InvoiceItem { Description = "Item 6", Quantity = 6, UnitPrice = 60 },
                new InvoiceItem { Description = "Item 7", Quantity = 7, UnitPrice = 70 },
                new InvoiceItem { Description = "Item 8", Quantity = 8, UnitPrice = 80 },
                new InvoiceItem { Description = "Item 1", Quantity = 2, UnitPrice = 10 },
                new InvoiceItem { Description = "Item 2", Quantity = 1, UnitPrice = 20 },
                new InvoiceItem { Description = "Item 3", Quantity = 3, UnitPrice = 30 },
                new InvoiceItem { Description = "Item 4", Quantity = 4, UnitPrice = 40 },
                new InvoiceItem { Description = "Item 5", Quantity = 5, UnitPrice = 50 },
                new InvoiceItem { Description = "Item 6", Quantity = 6, UnitPrice = 60 },
                new InvoiceItem { Description = "Item 7", Quantity = 7, UnitPrice = 70 },
                new InvoiceItem { Description = "Item 8", Quantity = 8, UnitPrice = 80 },
            };
            var model = new InvoiceModel
            {
                CompanyName = "Company Name",
                CompanyEmail = "email@email.com",
                InvoiceDate = DateTime.Now,
                CustomerName = "Customer Name",
                CustomerAddress = "Customer Address",
                Items = items
            };
            string htmlContent = await _razorLightEngine.CompileRenderAsync(templatePath, model);

            // Generate PDF using PuppeteerSharp
            // await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
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
                Page <span class='pageNumber'></span> of <span class='totalPages'></span>
            </div>",
                MarginOptions = new PuppeteerSharp.Media.MarginOptions { Top = "40px", Bottom = "60px" }
            });
            return file;
        }
    }
}
