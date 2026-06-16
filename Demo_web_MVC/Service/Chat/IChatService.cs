using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Chat;

namespace Demo_web_MVC.Service.Chat
{
    public interface IChatService
    {
        Task<Conversation> GetOrCreateSystemSupportConversationAsync(int userId);
        Task<Conversation> GetOrCreateOrderSellerConversationAsync(int orderId, int userId, int sellerId);
        Task<List<Conversation>> GetConversationsAsync(int userId, string role);
        Task<Conversation?> GetConversationDetailAsync(int conversationId, int userId, string role);
        Task<ChatMessage> SendMessageAsync(int conversationId, int senderId, string role, string content);
        Task<List<Conversation>>GetOrderSellerConversationsAsync(int sellerId);
        Task<List<ChatDropdownViewModel>> GetChatDropdownAsync(int userId);
    }
}
