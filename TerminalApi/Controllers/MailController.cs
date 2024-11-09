using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TerminalApi.Models;
using TerminalApi.Models.Mail;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    public class MailController : Controller
    {
        private readonly SendMailService mailService;

        public MailController(SendMailService mailService)
        {
            this.mailService = mailService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<Mail>> SendMail(Mail mail)
        {
            try
            {
                await mailService.SendEmail(mail);
                return Ok(
                    new ResponseDTO
                    {
                        Message = "Email est envoyé avec succés ",
                        Status = 200,
                        Data = mail
                    }
                );
            }
            catch
            {
                return BadRequest(
                    new ResponseDTO { Message = "Email n'est pas envoyé  ", Status = 400 }
                );
            }
        }
    }
}
