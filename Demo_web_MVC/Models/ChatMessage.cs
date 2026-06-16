namespace Demo_web_MVC.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        public int SenderId { get; set; }
        public User? Sender { get; set; }

        // Text, Image, File, Mixed, System
        public string MessageType { get; set; } = "Text";

        public string? Content { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    }
}
