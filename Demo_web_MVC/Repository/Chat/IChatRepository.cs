using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Chat;

namespace Demo_web_MVC.Repository.Chat
{
    public interface IChatRepository
    {
        Task<Conversation?> GetConversationByIdAsync(int conversationId);

        Task<List<Conversation>> GetConversationsByUserAsync(int userId);

        Task<List<Conversation>> GetSystemSupportConversationsAsync();
        Task<Conversation?> GetSystemSupportConversationByUserAsync(int userId);
        Task<Conversation?> GetOrderSellerConversationAsync(int orderId, int userId, int sellerId);
        Task<List<ChatDropdownViewModel>> GetChatDropdownAsync(int userId);
    }
}
