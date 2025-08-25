namespace TerminalApi.Models
{
    public class FileInfoResponse
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTimeOffset? UploadDate { get; set; }
    }
}
