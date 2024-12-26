using PuppeteerSharp;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;

namespace TerminalApi.Services
{
    public class PdfService
    {
        public PdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task GeneratePdfAsync(Order order)
        {
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            string path = Path.Combine(_env.WebRootPath, "TemplatesInvoice", "Invoice.html");

            if (File.Exists(path))
            {
                var tmp = string.Join("\n", File.ReadAllLines(path));tmp = string.Format(tmp,"lol");
                await page.SetContentAsync(tmp);
                await page.PdfAsync("customContent.pdf");
            }
        }
        private string ArticleDetails(Booking article)
        {
            return @"<tr class=""item""><td>{{article.Slot.StartAt}}-{{article.Slot.EndAt}}</td><td>{{article.Slot.Price}}</td></tr>";
        }
        private string template = @"<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <title>A simple, clean, and responsive HTML invoice template</title>

    <style>
      .invoice-box {
        max-width: 800px;
        margin: auto;
        padding: 30px;
        border: 1px solid #eee;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.15);
        font-size: 16px;
        line-height: 24px;
        font-family: ""Helvetica Neue"", ""Helvetica"", Helvetica, Arial, sans-serif;
        color: #555;
        position: relative;
         height: 250mm;
      }

      .invoice-box table {
        width: 100%;
        line-height: inherit;
        text-align: left;
      }

      .invoice-box table td {
        padding: 5px;
        vertical-align: top;
      }

      .invoice-box table tr td:nth-child(2) {
        text-align: right;
      }

      .invoice-box table tr.top table td {
        padding-bottom: 20px;
      }

      .invoice-box table tr.top table td.title {
        font-size: 45px;
        line-height: 45px;
        color: #333;
      }

      .invoice-box table tr.information table td {
        padding-bottom: 40px;
      }

      .invoice-box table tr.heading td {
        background: #eee;
        border-bottom: 1px solid #ddd;
        font-weight: bold;
      }

      .invoice-box table tr.details td {
        padding-bottom: 20px;
      }

      .invoice-box table tr.item td {
        border-bottom: 1px solid #eee;
      }

      .invoice-box table tr.item.last td {
        border-bottom: none;
      }

      .invoice-box table tr.total td:nth-child(2) {
        border-top: 2px solid #eee;
        font-weight: bold;
      }
      .invoice-box footer {
        text-align: center;
        margin-top: 20px;
        border-top: #333 1px solid;
        position: absolute;
        width: calc(100% - 60px);
        bottom: 0;
      }
      .invoice-box .date-signature {
        display: flex;
        justify-content: space-between;
        padding: 50px;
        text-align: center;
        padding: 20px;
        border-top: #333 1px solid;
      }

      @media only screen and (max-width: 600px) {
        .invoice-box table tr.top table td {
          width: 100%;
          display: block;
          text-align: center;
        }

        .invoice-box table tr.information table td {
          width: 100%;
          display: block;
          text-align: center;
        }
      }

      /** RTL **/
      .invoice-box.rtl {
        direction: rtl;
        font-family: Tahoma, ""Helvetica Neue"", ""Helvetica"", Helvetica, Arial,
          sans-serif;
      }

      .invoice-box.rtl table {
        text-align: right;
      }

      .invoice-box.rtl table tr td:nth-child(2) {
        text-align: left;
      }
    </style>
  </head>

  <body>
    <div class=""invoice-box"">
      <table cellpadding=""0"" cellspacing=""0"">
        <tr class=""top"">
          <td colspan=""2"">
            <table>
              <tr>
                <td class=""title"">
                  <img
                    src=""https://sparksuite.github.io/simple-html-invoice-template/images/logo.png""
                    style=""width: 100%; max-width: 300px""
                  />
                </td>

                <td>
                  Facture #: {0}<br />
                  Date: {{CreatedAt}}<br />
                </td>
              </tr>
            </table>
          </td>
        </tr>

        <tr class=""information"">
          <td colspan=""2"">
            <table>
              <tr>
                <td>
                  {{StreetNumber}} {{Street}}<br />
                  {{StreetLine2}}<br />
                  {{PostalCode}} {{City}}
                </td>

                <td>
                  {{FirstName}} {{LastName}}<br />
                  {{Email}} <br />
                </td>
              </tr>
            </table>
          </td>
        </tr>

        <tr class=""heading"">
          <td>Article</td>

          <td>Prix</td>
        </tr>

        <!-- <tr class=""item"">
          <td>{{StartAt}}-{{EndAt}}</td>

          <td>{{Price}}</td>
        </tr> -->
        {{Bookings}}

        <tr class=""total"">
          <td></td>

          <td>Total payé: {{Total}}</td>
        </tr>
      </table>
      <div class=""date-signature"">
        <p>Fait le: {{CreatedAt}} à Bordeaux</p>
        <img
          src=""https://cdn.prod.website-files.com/61d7de73eec437f52da6d699/62161cf7328ad280841f653f_esignature-signature.png""
          style=""width: 40mm; height: 30mm""
        />
      </div>
      <footer>
        <strong>{{Inspire - 2025}}</strong>
      </footer>
    </div>
  </body>
</html>
";
        private readonly IWebHostEnvironment _env;
    }
}
