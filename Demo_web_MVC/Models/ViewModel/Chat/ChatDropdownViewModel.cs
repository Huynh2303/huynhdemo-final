namespace Demo_web_MVC.Models.ViewModel.Chat
{
    public class ChatDropdownViewModel
    {
        public int ConversationId { get; set; }

        public string DisplayName { get; set; } = "";
        public string LastMessage { get; set; } = "";

        public DateTime? LastMessageAt { get; set; }

        public int UnreadCount { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
