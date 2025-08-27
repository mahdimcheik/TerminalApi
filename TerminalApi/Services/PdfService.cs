using PuppeteerSharp;
using PuppeteerSharp.Media;
using RazorLight;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TerminalApi.Services
{
    public class PdfService : IPdfService
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

        public async Task<byte[]> GeneratePdfAsync(OrderResponseForStudentDTO order)
        {
            //var res = Guid.TryParse(orderId, out Guid guidId);

            try
            {
                string templatePath = "Invoice.cshtml"; // Name of your template file


                string htmlContent = await _razorLightEngine.CompileRenderAsync(templatePath, order);

                bool inDebugMode = false;
#if DEBUG
                inDebugMode = true;
#endif

                using var browser = await Puppeteer.LaunchAsync(
                    new LaunchOptions { Headless = true,
                        Args =inDebugMode ? [] : new[] {
                                "--no-sandbox",
                                "--disable-setuid-sandbox",
                                "--disable-dev-shm-usage",
                                "--disable-gpu",
                                "--single-process",
                                "--no-zygote"
    }
                    }
                );
                using var page = await browser.NewPageAsync();

                await page.SetContentAsync(htmlContent);
                var file = await page.PdfDataAsync(new PdfOptions
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
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<byte[]> GenerateQuestPdfAsync(OrderResponseForStudentDTO orderInput)
        {
            try
            {
                var file = Create(orderInput);
                return file;
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new byte[0]);
            }
        }

        public byte[] Create(OrderResponseForStudentDTO model)
        {
            return Document.Create(container =>
            {
                var path = _env.WebRootPath;
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Page Header & Footer
                    page.Header()
                        .Column(column =>
                        {
                            // Use a Row to place the image and text side-by-side
                            column.Item().Row(row =>
                            {
                                // Left column for the image
                                row.RelativeItem(2).AlignCenter().Image($"{_env.WebRootPath}/icon.png").FitWidth();

                                // Right column for the text
                                row.RelativeItem(5).Column(textColumn =>
                                {
                                    textColumn.Item().Text("Facture").FontSize(24).Bold().FontColor(Colors.Grey.Darken2).AlignRight();
                                    textColumn.Item().Text("Skill Hive").FontSize(14).Bold().AlignRight();
                                    textColumn.Item().Text("Numéro Siret: 123-5845-859").AlignRight();
                                    textColumn.Item().Text("Email: teacher@skillhive.fr").AlignRight();
                                    textColumn.Item().Text($"Date: {model.PaymentDate?.ToString("yyyy-MM-dd")}").AlignRight();
                                });
                            });
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            // Customer Information
                            column.Item().PaddingTop(20).Text("Coordonnées client:").FontSize(14).Bold();
                            column.Item().PaddingTop(5).Text(text =>
                            {
                                text.Span("Client: ").Bold();
                                text.Span($"{model.Bookings?.First().StudentFirstName} {model.Bookings?.First().StudentLastName}");
                            });
                            column.Item().Text(text =>
                            {
                                text.Span("Address: ").Bold();
                                text.Span("1 rue Honoré Daumier, Talence 33400");
                            });
                            column.Item().Text(text =>
                            {
                                text.Span("Transaction: ").Bold();
                                text.Span(model.PaymentIntent);
                            });
                            column.Item().Text(text =>
                            {
                                text.Span("Numéro facture: ").Bold();
                                text.Span(model.OrderNumber);
                            });

                            // Invoice Items Table
                            column.Item().PaddingTop(20).Table(table =>
                            {
                                // Define table columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Table Headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderStyle).Text("#");
                                    header.Cell().Element(HeaderStyle).Text("Description");
                                    header.Cell().Element(HeaderStyle).Text("Date");
                                    header.Cell().Element(HeaderStyle).AlignRight().Text("Prix");
                                    header.Cell().Element(HeaderStyle).AlignRight().Text("Prix réduit");

                                    static IContainer HeaderStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3);
                                    }
                                });

                                // Table Rows (Data)
                                foreach (var item in model.Bookings)
                                {
                                    table.Cell().Element(CellStyle).Text(model.Bookings.ToList().IndexOf(item) + 1);
                                    table.Cell().Element(CellStyle).Text(item.Subject);
                                    table.Cell().Element(CellStyle).Text(item.StartAt.ToString("yyyy-MM-dd"));
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.Price.ToString("F2"));
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.DiscountedPrice.ToString("F2"));

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                    }
                                }
                            });

                            // Total and Tax Rows
                            column.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(8);
                                    columns.RelativeColumn(2);
                                });

                                // Total
                                table.Cell().AlignRight().Text("Total payé:").Bold();
                                table.Cell().AlignRight().Text($"{model.Bookings?.Sum(i => i.DiscountedPrice).ToString("F2")} €").Bold();

                                // TVA
                                table.Cell().PaddingTop(5).AlignRight().Text("Dont TVA:").Bold();
                                table.Cell().PaddingTop(5).AlignRight().Text($"{(model.Bookings?.Sum(i => i.DiscountedPrice) * 0.2m)?.ToString("F2")} €").Bold();
                            });
                        });

                    // Page Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Merci de votre achat\n");
                            x.Span("Page : ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }
    }
}
