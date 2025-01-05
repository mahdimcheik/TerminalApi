using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
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

        public async Task GeneratePdfAsync(string orderId)
        {
            var guid = Guid.Parse(orderId);
            //var order = await context.Orders
            //    .Include(x => x.Booker)
            //    .Include(x => x.Bookings)
            //        .ThenInclude(b => b.Slot)
            //    //.FirstOrDefaultAsync(x => x.Id == guid)
            //                    .Select(x => new OrderDetailsDto
            //                    {
            //                        BookerImgUrl = x.Booker.ImgUrl ?? "",
            //                        Bookings = x.Bookings.Select(b => new BookingDetailsDto
            //                        {
            //                            BookingCreatedAt = b.CreatedAt,
            //                            SlotStartAt = b.Slot.StartAt,
            //                            SlotEndAt = b.Slot.EndAt
            //                        }).ToList()
            //                    })
            //    .FirstOrDefaultAsync();

            //var orderDetails = order.Select(x => new OrderDetailsDto
            //{
            //    BookerImgUrl = x.Booker.ImgUrl ?? "",
            //    Bookings = x.Bookings.Select(b => new BookingDetailsDto
            //    {
            //        BookingCreatedAt = b.CreatedAt,
            //        SlotStartAt = b.Slot.StartAt,
            //        SlotEndAt = b.Slot.EndAt
            //    }).ToList()
            //})
            var orderDetails = await context.Orders
                .Where(x => x.Id == guid)
                .Select(x => new OrderDetailsDto
                {
                    BookerImgUrl = x.Booker.ImgUrl ?? "",
                    Bookings = x.Bookings.Select(b => new BookingDetailsDto
                    {
                        BookingCreatedAt = b.CreatedAt,
                        SlotStartAt = b.Slot.StartAt,
                        SlotEndAt = b.Slot.EndAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            if (orderDetails is null) return;
            // Render the HTML content using RazorLight
            string templatePath = "Invoice.cshtml"; // Name of your template file
            var model = new InvoiceViewModel(orderDetails);
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
