using Demo_web_MVC.Migrations;

namespace Demo_web_MVC.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        // UserAdmin, SellerAdmin, OrderSeller
        public string Type { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        // Chỉ dùng khi Type = OrderSeller
        public int? OrderId { get; set; }
        public Order? Order { get; set; }

        // Chỉ dùng khi liên quan seller
        public int? SellerId { get; set; }
        public User? Seller { get; set; }

        // Để hiển thị danh sách chat nhanh
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
