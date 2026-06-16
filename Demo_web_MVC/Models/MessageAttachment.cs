namespace Demo_web_MVC.Models
{
    public class MessageAttachment
    {
        public int Id { get; set; }

        public int MessageId { get; set; }
        public ChatMessage? Message { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
