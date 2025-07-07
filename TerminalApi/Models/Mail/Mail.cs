namespace TerminalApi.Models
{
    /// <summary>
    /// Class minimaliste, contient les variables nécessaires à l'envoi de mail
    /// </summary>
    public class Mail
    {
        public string? MailTo { get; set; }
        public string? MailSubject { get; set; }
        public string? MailBody { get; set; }
        public string? MailFrom { get; set; }
        public bool? SendtoSender { get; set; } = false;
        public UserApp? Sender { get; set; }
        public UserApp? Reciever { get; set; }

    }
}
