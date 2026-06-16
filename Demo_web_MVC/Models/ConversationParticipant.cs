namespace Demo_web_MVC.Models
{
    public class ConversationParticipant
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        // User, Seller
        public string RoleInConversation { get; set; } = string.Empty;

        public DateTime? LastReadAt { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
