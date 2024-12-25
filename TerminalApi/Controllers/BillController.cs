﻿using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> Get()
        {
            await pdfService.GeneratePdfAsync(null);
            return Ok();
        }
    }
}