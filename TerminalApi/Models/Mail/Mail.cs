using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerminalApi.Models.Mail
{
    /// <summary>
    /// Class minimaliste, contient les variables nécessaires à l'envoi de mail
    /// </summary>
    public class Mail
    {
        public string MailTo { get; set; }
        public string MailSubject { get; set; }
        public string? MailBody { get; set; }
        public string MailFrom { get; set; }
    }
}
